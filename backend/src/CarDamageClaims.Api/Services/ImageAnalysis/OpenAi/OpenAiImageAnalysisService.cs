using System.Net.Http.Headers;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using CarDamageClaims.Api.Services.ImageAnalysis;
using CarDamageClaims.Api.Services.ImageAnalysis.Exceptions;
using Microsoft.Extensions.Options;

namespace CarDamageClaims.Api.Services.ImageAnalysis.OpenAi;

public class OpenAiImageAnalysisService(
    IOptions<OpenAiOptions> options,
    DamageAnalysisPrompts damageAnalysisPrompts,
    RepairPricingPrompts repairPricingPrompts,
    RepairPricingResponseReader repairPricingResponseReader,
    RepairCostMatcher repairCostMatcher,
    DamageInferenceService damageInferenceService,
    DamagePartNormalizer damagePartNormalizer,
    OpenAiResponsesClient openAiResponsesClient,
    OpenAiResponsesPayload openAiResponsesPayload,
    OpenAiResponseContentReader openAiResponseContentReader,
    ILogger<OpenAiImageAnalysisService> logger
) : IImageAnalysisService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    private static readonly JsonSerializerOptions JsonNamingOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    private readonly OpenAiOptions openAiOptions = options.Value;
    private readonly DamageAnalysisPrompts damageAnalysisPrompts = damageAnalysisPrompts;
    private readonly RepairPricingPrompts repairPricingPrompts = repairPricingPrompts;
    private readonly RepairPricingResponseReader repairPricingResponseReader = repairPricingResponseReader;
    private readonly RepairCostMatcher repairCostMatcher = repairCostMatcher;
    private readonly DamageInferenceService damageInferenceService = damageInferenceService;
    private readonly DamagePartNormalizer damagePartNormalizer = damagePartNormalizer;
    private readonly OpenAiResponsesClient openAiResponsesClient = openAiResponsesClient;
    private readonly OpenAiResponsesPayload openAiResponsesPayload = openAiResponsesPayload;
    private readonly OpenAiResponseContentReader openAiResponseContentReader =
        openAiResponseContentReader;

    public async Task<ImageAnalysisResult> AnalyzeAsync(
        IReadOnlyList<string> filePaths,
        string? carBrand = null,
        string? carModel = null,
        int? carYear = null,
        CancellationToken cancellationToken = default
    )
    {
        if (filePaths.Count == 0)
        {
            throw new NotCarDetectedException("No images were provided for analysis.");
        }

        var apiKey = openAiOptions.ApiKey?.Trim();
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            throw new InvalidOperationException("OpenAI API key is not configured.");
        }

        using var httpClient = CreateOpenAiHttpClient(apiKey);

        var analysis = await AnalyzeDamageAsync(httpClient, filePaths, cancellationToken);

        if (!analysis.IsCar || analysis.Confidence < 0.55)
        {
            throw new NotCarDetectedException(
                string.IsNullOrWhiteSpace(analysis.NotCarReason)
                    ? "Images do not contain a recognizable car."
                    : analysis.NotCarReason
            );
        }

        var items = analysis
            .Damages.Where(ShouldIncludeInEstimate)
            .Select(d =>
            {
                var side = damagePartNormalizer.NormalizeSide(
                    d.Side,
                    d.SideAmbiguous,
                    d.SideConfidence
                );
                var normalizedPartName = damagePartNormalizer.ResolvePartName(
                    d.PartName,
                    d.DamageType,
                    d.Evidence,
                    side
                );
                side = damagePartNormalizer.AdjustSideForPart(normalizedPartName, side);
                var partName = string.IsNullOrWhiteSpace(normalizedPartName)
                    ? "Неизвестная деталь"
                    : $"{normalizedPartName} ({damagePartNormalizer.FormatSideForDisplay(side)})";

                var confidence = Math.Clamp(d.Confidence, 0, 1);

                return new ImageDamageItemResult
                {
                    PartName = partName,
                    DamageDescription = string.IsNullOrWhiteSpace(d.DamageType)
                        ? "Повреждение обнаружено"
                        : damagePartNormalizer.NormalizeDamageDescriptionToRussian(d.DamageType),
                    Severity = damagePartNormalizer.NormalizeSeverity(d.Severity),
                    EstimatedCost = 0,
                    Confidence = confidence,
                };
            })
            .Where(x => x.EstimatedCost >= 0)
            .ToList();

        damageInferenceService.AddInferredFenderIfLikely(items);
        damageInferenceService.AddInferredFrontAdjacentParts(items);
        damageInferenceService.AddInferredRearAdjacentParts(items);
        damageInferenceService.AddInferredGlassParts(items);
        damageInferenceService.AddInferredRolloverParts(items, analysis.Summary);
        items = damageInferenceService.DeduplicateItems(items);

        var pricing = await EstimatePricingAsync(
            httpClient,
            carBrand,
            carModel,
            carYear,
            items,
            cancellationToken
        );

        repairCostMatcher.ApplyPricing(items, pricing);

        var missingPriceItems = items.Where(x => x.EstimatedCost <= 0).ToList();
        if (missingPriceItems.Count > 0)
        {
            var retryPricing = await EstimatePricingAsync(
                httpClient,
                carBrand,
                carModel,
                carYear,
                missingPriceItems,
                cancellationToken
            );
            repairCostMatcher.ApplyPricing(items, retryPricing);
        }

        repairCostMatcher.ApplyCategoryFallbackPrices(items);

        if (items.All(x => x.EstimatedCost <= 0))
        {
            throw new AiServiceUnavailableException("AI pricing is temporarily unavailable.");
        }

        var total = items.Sum(x => x.EstimatedCost);

        var summary = BuildRussianSummary(analysis.Summary, items);

        if (pricing.RepairTotalMax > 0)
        {
            summary +=
                $" Ориентир по стоимости ремонтных работ (отдельно от запчастей): {pricing.RepairTotalMin:N0}-{pricing.RepairTotalMax:N0} ₽.";
        }

        return new ImageAnalysisResult
        {
            IsCar = true,
            Summary = summary,
            EstimatedTotalCost = total,
            Confidence = Math.Clamp(analysis.Confidence, 0, 1),
            DamageItems = items,
        };
    }

    private async Task<DamageAnalysisResponse> AnalyzeDamageAsync(
        HttpClient httpClient,
        IReadOnlyList<string> filePaths,
        CancellationToken cancellationToken
    )
    {
        var request = openAiResponsesPayload.CreateResponsesRequest(
            filePaths,
            damageAnalysisPrompts.GetAnalysisPrompt(),
            openAiOptions.Model,
            openAiOptions.AnalysisMaxOutputTokens,
            includeWebSearch: false,
            forceJsonFormat: true
        );

        var response = await openAiResponsesClient.SendAsync(
            httpClient,
            request,
            JsonNamingOptions,
            cancellationToken
        );

        var payload =
            await response.Content.ReadFromJsonAsync<ResponsesApiPayload>(
                JsonOptions,
                cancellationToken
            ) ?? throw new InvalidOperationException("OpenAI returned empty analysis payload.");

        var json = openAiResponseContentReader.ExtractJson(
            payload.OutputText,
            payload.Output?.SelectMany(x => x.Content ?? []).Select(x => x.Text).ToList() ?? []
        );
        if (!json.TrimStart().StartsWith("{", StringComparison.Ordinal))
        {
            logger.LogWarning("OpenAI returned non-JSON payload text: {Payload}", json);
        }
        DamageAnalysisResponse result;
        try
        {
            result =
                JsonSerializer.Deserialize<DamageAnalysisResponse>(json, JsonOptions)
                ?? throw new InvalidOperationException("OpenAI analysis JSON could not be parsed.");
        }
        catch (JsonException)
        {
            logger.LogWarning("OpenAI returned unparsable JSON payload: {Payload}", json);

            var retryRequest = openAiResponsesPayload.CreateResponsesRequest(
                filePaths,
                damageAnalysisPrompts.GetJsonRetryPrompt(),
                openAiOptions.Model,
                openAiOptions.AnalysisMaxOutputTokens,
                includeWebSearch: false,
                forceJsonFormat: true
            );

            var retryResponse = await openAiResponsesClient.SendAsync(
                httpClient,
                retryRequest,
                JsonNamingOptions,
                cancellationToken
            );

            var retryPayload =
                await retryResponse.Content.ReadFromJsonAsync<ResponsesApiPayload>(
                    JsonOptions,
                    cancellationToken
                ) ?? throw new InvalidOperationException("OpenAI retry returned empty payload.");

            var retryJson = openAiResponseContentReader.ExtractJson(
                retryPayload.OutputText,
                retryPayload.Output?.SelectMany(x => x.Content ?? []).Select(x => x.Text).ToList()
                    ?? []
            );
            try
            {
                result =
                    JsonSerializer.Deserialize<DamageAnalysisResponse>(retryJson, JsonOptions)
                    ?? throw new InvalidOperationException(
                        "OpenAI retry JSON could not be parsed."
                    );
            }
            catch (JsonException)
            {
                logger.LogWarning(
                    "OpenAI retry returned unparsable JSON payload: {Payload}",
                    retryJson
                );
                throw new InvalidOperationException("OpenAI retry JSON could not be parsed.");
            }
        }

        logger.LogInformation(
            "AI analysis completed. IsCar={IsCar}, Items={Count}, Confidence={Confidence}",
            result.IsCar,
            result.Damages.Count,
            result.Confidence
        );

        return result;
    }

    private HttpClient CreateOpenAiHttpClient(string apiKey)
    {
        HttpClient httpClient;
        var proxyUrl = openAiOptions.ProxyUrl?.Trim();

        if (string.IsNullOrWhiteSpace(proxyUrl))
        {
            httpClient = new HttpClient();
        }
        else
        {
            var handler = new HttpClientHandler
            {
                Proxy = new WebProxy(proxyUrl),
                UseProxy = true,
            };

            httpClient = new HttpClient(handler, disposeHandler: true);
            logger.LogInformation("OpenAI proxy is enabled for OpenAI requests.");
        }

        httpClient.BaseAddress = new Uri(openAiOptions.BaseUrl.TrimEnd('/') + "/");
        httpClient.Timeout = TimeSpan.FromSeconds(60);
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            apiKey
        );

        return httpClient;
    }

    private async Task<PricingResponse> EstimatePricingAsync(
        HttpClient httpClient,
        string? carBrand,
        string? carModel,
        int? carYear,
        List<ImageDamageItemResult> items,
        CancellationToken cancellationToken
    )
    {
        if (items.Count == 0)
        {
            return new PricingResponse();
        }

        var request = openAiResponsesPayload.CreateResponsesRequest(
            Array.Empty<string>(),
            repairPricingPrompts.GetPricingPrompt(carBrand, carModel, carYear, items),
            openAiOptions.Model,
            openAiOptions.PricingMaxOutputTokens,
            includeWebSearch: true,
            forceJsonFormat: false
        );

        var response = await openAiResponsesClient.SendAsync(
            httpClient,
            request,
            JsonNamingOptions,
            cancellationToken
        );
        var payload =
            await response.Content.ReadFromJsonAsync<ResponsesApiPayload>(
                JsonOptions,
                cancellationToken
            ) ?? throw new InvalidOperationException("OpenAI pricing returned empty payload.");
        var json = openAiResponseContentReader.ExtractJson(
            payload.OutputText,
            payload.Output?.SelectMany(x => x.Content ?? []).Select(x => x.Text).ToList() ?? []
        );
        logger.LogInformation("OpenAI pricing raw payload: {Payload}", json);

        if (!json.TrimEnd().EndsWith("}", StringComparison.Ordinal))
        {
            logger.LogWarning("OpenAI pricing payload seems truncated, using flexible parser.");
            return repairPricingResponseReader.ParsePricingResponseFlexible(json);
        }

        try
        {
            var parsed = JsonSerializer.Deserialize<PricingResponse>(json, JsonOptions);
            if (parsed is not null && parsed.PartPrices.Count > 0)
            {
                return parsed;
            }

            var normalized = repairPricingResponseReader.ParsePricingResponseFlexible(json);
            if (normalized.PartPrices.Count > 0)
            {
                return normalized;
            }

            return parsed ?? new PricingResponse();
        }
        catch (JsonException)
        {
            logger.LogWarning("OpenAI pricing JSON parse failed. Payload: {Payload}", json);
            return repairPricingResponseReader.ParsePricingResponseFlexible(json);
        }
    }

    private static bool ShouldIncludeInEstimate(DamageItemResponse item)
    {
        var status = item.DetectionStatus?.Trim().ToLowerInvariant();
        if (status == "not_visible")
        {
            return false;
        }

        if (status == "likely" && item.Confidence < 0.55)
        {
            return false;
        }

        return true;
    }

    private string BuildRussianSummary(
        string? rawSummary,
        IReadOnlyCollection<ImageDamageItemResult> items
    )
    {
        if (!string.IsNullOrWhiteSpace(rawSummary) && ContainsCyrillic(rawSummary))
        {
            return rawSummary.Trim();
        }

        var parts = items
            .Select(x => damagePartNormalizer.NormalizePartNameToRussian(x.PartName))
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Take(5)
            .ToArray();

        if (parts.Length == 0)
        {
            return "Обнаружены повреждения автомобиля.";
        }

        return $"Повреждены элементы: {string.Join(", ", parts)}.";
    }

    private static bool ContainsCyrillic(string value)
    {
        return value.Any(ch => (ch >= 'А' && ch <= 'я') || ch == 'Ё' || ch == 'ё');
    }

    private sealed class ResponsesApiPayload
    {
        public string? OutputText { get; set; }

        public List<OutputItem>? Output { get; set; }
    }

    private sealed class OutputItem
    {
        public List<OutputContentItem>? Content { get; set; }
    }

    private sealed class OutputContentItem
    {
        public string? Type { get; set; }

        public string? Text { get; set; }
    }

    private sealed class DamageAnalysisResponse
    {
        [JsonPropertyName("is_car")]
        public bool IsCar { get; set; }

        [JsonPropertyName("not_car_reason")]
        public string NotCarReason { get; set; } = string.Empty;

        [JsonPropertyName("summary")]
        public string Summary { get; set; } = string.Empty;

        [JsonPropertyName("confidence")]
        public double Confidence { get; set; }

        [JsonPropertyName("damages")]
        public List<DamageItemResponse> Damages { get; set; } = new();
    }

    private sealed class DamageItemResponse
    {
        [JsonPropertyName("part_name")]
        public string PartName { get; set; } = string.Empty;

        [JsonPropertyName("detection_status")]
        public string DetectionStatus { get; set; } = "detected";

        [JsonPropertyName("side")]
        public string Side { get; set; } = "unknown";

        [JsonPropertyName("side_confidence")]
        public double SideConfidence { get; set; }

        [JsonPropertyName("side_ambiguous")]
        public bool SideAmbiguous { get; set; }

        [JsonPropertyName("damage_type")]
        public string DamageType { get; set; } = string.Empty;

        [JsonPropertyName("evidence")]
        public string Evidence { get; set; } = string.Empty;

        [JsonPropertyName("severity")]
        public string Severity { get; set; } = "medium";

        [JsonPropertyName("confidence")]
        public double Confidence { get; set; }
    }
}

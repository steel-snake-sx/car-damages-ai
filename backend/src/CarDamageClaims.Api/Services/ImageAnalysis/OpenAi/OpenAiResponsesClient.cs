using System.Net.Http.Json;
using System.Text.Json;
using CarDamageClaims.Api.Services.ImageAnalysis.Exceptions;

namespace CarDamageClaims.Api.Services.ImageAnalysis.OpenAi;

public class OpenAiResponsesClient(ILogger<OpenAiResponsesClient> logger)
{
    public async Task<HttpResponseMessage> SendAsync(
        HttpClient httpClient,
        object request,
        JsonSerializerOptions jsonNamingOptions,
        CancellationToken cancellationToken
    )
    {
        var response = await httpClient.PostAsJsonAsync(
            "responses",
            request,
            jsonNamingOptions,
            cancellationToken
        );

        await EnsureSuccessOrThrowAiExceptionAsync(response, cancellationToken);
        return response;
    }

    public async Task EnsureSuccessOrThrowAiExceptionAsync(
        HttpResponseMessage response,
        CancellationToken cancellationToken
    )
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
        logger.LogError(
            "OpenAI request failed. StatusCode={StatusCode}. Body={Body}",
            (int)response.StatusCode,
            responseBody
        );

        if ((int)response.StatusCode == 429 || (int)response.StatusCode >= 500)
        {
            throw new AiServiceUnavailableException("OpenAI service is temporarily unavailable.");
        }

        throw new InvalidOperationException($"OpenAI API error: {(int)response.StatusCode}");
    }
}

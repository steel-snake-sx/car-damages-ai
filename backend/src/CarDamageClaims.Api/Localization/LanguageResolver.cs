using Microsoft.AspNetCore.Http;

namespace CarDamageClaims.Api.Localization;

public static class LanguageResolver
{
    public static AppLanguage Resolve(HttpContext httpContext)
    {
        var explicitHeader = httpContext.Request.Headers["X-Language"].ToString();
        if (TryParse(explicitHeader, out var explicitLang))
        {
            return explicitLang;
        }

        var acceptLanguage = httpContext.Request.Headers.AcceptLanguage.ToString();
        if (TryParse(acceptLanguage, out var acceptLang))
        {
            return acceptLang;
        }

        return AppLanguage.Ru;
    }

    private static bool TryParse(string input, out AppLanguage language)
    {
        language = AppLanguage.Ru;

        if (string.IsNullOrWhiteSpace(input))
        {
            return false;
        }

        var normalized = input.Trim().ToLowerInvariant();

        if (normalized.StartsWith("en"))
        {
            language = AppLanguage.En;
            return true;
        }

        if (normalized.StartsWith("ru"))
        {
            language = AppLanguage.Ru;
            return true;
        }

        var segment = normalized
            .Split(new[] { ',', ';', '-', '_', ' ' }, StringSplitOptions.RemoveEmptyEntries)
            .FirstOrDefault();
        if (segment == "en")
        {
            language = AppLanguage.En;
            return true;
        }

        if (segment == "ru")
        {
            language = AppLanguage.Ru;
            return true;
        }

        return false;
    }
}

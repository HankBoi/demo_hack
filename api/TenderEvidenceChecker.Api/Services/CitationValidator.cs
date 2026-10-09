namespace TenderEvidenceChecker.Api.Services;

public static class CitationValidator
{
    public static readonly HashSet<string> RequirementClasses = new(StringComparer.OrdinalIgnoreCase)
    {
        "mandatory", "evaluated", "informational", "uncertain"
    };

    public static readonly HashSet<string> MandatoryLabels = new(StringComparer.OrdinalIgnoreCase)
    {
        "explicit", "conditional", "unclear"
    };

    public static bool QuoteSupported(string? pageText, string? quote)
    {
        var page = TextNorm.Normalize(pageText);
        var cited = TextNorm.Normalize(quote);
        if (cited.Length < 12 || page.Length == 0)
        {
            return false;
        }

        return page.Contains(cited, StringComparison.Ordinal);
    }
}

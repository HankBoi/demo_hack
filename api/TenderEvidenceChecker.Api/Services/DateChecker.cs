using System.Text.RegularExpressions;

namespace TenderEvidenceChecker.Api.Services;

public sealed record DateCheckResult(string Kind, string? ObservedDate, string Basis);

public static partial class DateChecker
{
    private static readonly string[] ExpiryCues =
    [
        "bitmə tarixi",
        "bitme tarixi",
        "expiry",
        "valid until",
        "expires"
    ];

    public static DateCheckResult Inspect(string? text, DateOnly referenceDate)
    {
        var source = text ?? "";
        var folded = TextNorm.Fold(source);
        var hasCue = ExpiryCues.Any(cue => folded.Contains(TextNorm.Fold(cue), StringComparison.Ordinal));
        var iso = IsoDate().Match(source);
        var ambiguous = AmbiguousDate().IsMatch(source);

        if (hasCue && iso.Success && DateOnly.TryParse(iso.Value, out var date))
        {
            if (date < referenceDate)
            {
                return new DateCheckResult(
                    "expired",
                    date.ToString("yyyy-MM-dd"),
                    $"The uploaded file shows an explicit expiry date of {date:yyyy-MM-dd}, which is before the reference date {referenceDate:yyyy-MM-dd}. This is a date observation, not a legal eligibility decision.");
            }

            return new DateCheckResult(
                "dated",
                date.ToString("yyyy-MM-dd"),
                $"The uploaded file shows an explicit expiry date of {date:yyyy-MM-dd}, which is on or after the reference date {referenceDate:yyyy-MM-dd}.");
        }

        if (ambiguous || hasCue)
        {
            return new DateCheckResult(
                "ambiguous",
                null,
                "A date in the uploaded file is ambiguous or not an explicit ISO expiry. It needs a person to read it, and it is not marked expired.");
        }

        return new DateCheckResult("none", null, "");
    }

    [GeneratedRegex(@"\b(20\d{2})-(\d{2})-(\d{2})\b")]
    private static partial Regex IsoDate();

    [GeneratedRegex(@"\b\d{1,2}[./]\d{1,2}[./]\d{4}\b")]
    private static partial Regex AmbiguousDate();
}

using System.Text;

namespace TenderEvidenceChecker.Api.Services;

public static class TextNorm
{
    public static string Normalize(string? text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return "";
        }

        var form = text.Normalize(NormalizationForm.FormC)
            .Replace('\u00a0', ' ')
            .Replace("\r\n", "\n")
            .Replace('\r', '\n');
        var builder = new StringBuilder(form.Length);
        var pendingSpace = false;
        foreach (var ch in form)
        {
            if (char.IsWhiteSpace(ch))
            {
                pendingSpace = builder.Length > 0;
                continue;
            }

            if (pendingSpace)
            {
                builder.Append(' ');
                pendingSpace = false;
            }

            builder.Append(ch);
        }

        return builder.ToString().Trim();
    }

    public static string Fold(string? text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return "";
        }

        var builder = new StringBuilder(text.Length);
        foreach (var ch in text)
        {
            builder.Append(ch switch
            {
                'İ' => 'i',
                'I' => 'ı',
                _ => char.ToLowerInvariant(ch)
            });
        }

        return builder.ToString();
    }

    public static bool HasReadableLetters(string? text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return false;
        }

        var letters = 0;
        foreach (var ch in text)
        {
            if (char.IsLetter(ch))
            {
                letters++;
                if (letters >= 20)
                {
                    return true;
                }
            }
        }

        return false;
    }

    public static IEnumerable<string> Paragraphs(string text)
    {
        var normalized = text.Replace("\r\n", "\n").Replace('\r', '\n').Trim();
        if (normalized.Length == 0)
        {
            yield break;
        }

        if (normalized.Contains("\n\n", StringComparison.Ordinal))
        {
            foreach (var block in normalized.Split("\n\n", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                var flat = Normalize(block);
                if (flat.Length > 0)
                {
                    yield return flat;
                }
            }

            yield break;
        }

        if (normalized.Contains('\n'))
        {
            foreach (var line in normalized.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                var flat = Normalize(line);
                if (flat.Length > 0)
                {
                    yield return flat;
                }
            }

            yield break;
        }

        yield return Normalize(normalized);
    }

    public static List<string> Tokens(string text)
    {
        var folded = Fold(Normalize(text));
        var tokens = new List<string>();
        var current = new StringBuilder();
        foreach (var ch in folded)
        {
            if (char.IsLetterOrDigit(ch))
            {
                current.Append(ch);
                continue;
            }

            Flush(current, tokens);
        }

        Flush(current, tokens);
        return tokens;
    }

    private static void Flush(StringBuilder current, List<string> tokens)
    {
        if (current.Length >= 5)
        {
            tokens.Add(current.ToString());
        }

        current.Clear();
    }

    public static string Preview(string? text, int max)
    {
        var normalized = Normalize(text);
        if (normalized.Length <= max)
        {
            return normalized;
        }

        return normalized[..max].TrimEnd() + "…";
    }

    public static string SafeFileName(string? name)
    {
        var file = Path.GetFileName(name ?? "");
        file = file.Replace("\r", "").Replace("\n", "");
        if (string.IsNullOrWhiteSpace(file))
        {
            return "document.pdf";
        }

        return file.Length <= 180 ? file : file[..180];
    }
}

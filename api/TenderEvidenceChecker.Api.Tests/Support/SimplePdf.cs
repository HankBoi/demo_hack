using System.Text;

namespace TenderEvidenceChecker.Api.Tests.Support;

public static class SimplePdf
{
    public static byte[] Create(params string[] pageTexts)
    {
        if (pageTexts.Length == 0)
        {
            pageTexts = [""];
        }

        var fontObject = 3 + pageTexts.Length * 2;
        var objects = new List<string>
        {
            "<< /Type /Catalog /Pages 2 0 R >>",
            $"<< /Type /Pages /Count {pageTexts.Length} /Kids [{string.Join(' ', Enumerable.Range(0, pageTexts.Length).Select(index => $"{3 + index * 2} 0 R"))}] >>"
        };

        for (var index = 0; index < pageTexts.Length; index++)
        {
            var contentObject = 4 + index * 2;
            var stream = string.IsNullOrEmpty(pageTexts[index])
                ? "72 700 m 140 700 l S"
                : $"BT /F1 12 Tf 72 700 Td ({Escape(pageTexts[index])}) Tj ET";
            objects.Add($"<< /Type /Page /Parent 2 0 R /MediaBox [0 0 612 792] /Contents {contentObject} 0 R /Resources << /Font << /F1 {fontObject} 0 R >> >> >>");
            objects.Add($"<< /Length {Encoding.ASCII.GetByteCount(stream)} >>\nstream\n{stream}\nendstream");
        }

        objects.Add("<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica >>");
        return Assemble(objects, encrypted: false);
    }

    public static byte[] CreateEncryptedPlaceholder()
    {
        var plain = Create("Secret page");
        var text = Encoding.ASCII.GetString(plain);
        var encrypted = text.Replace("/Root 1 0 R", "/Root 1 0 R /Encrypt 6 0 R", StringComparison.Ordinal);
        if (encrypted == text)
        {
            throw new InvalidOperationException("Could not mark the test PDF as encrypted.");
        }

        var dictionary = Encoding.ASCII.GetBytes(
            "6 0 obj\n<< /Filter /Standard /V 1 /R 2 /O <00000000000000000000000000000000> /U <00000000000000000000000000000000> /P -4 >>\nendobj\n");
        var eof = encrypted.IndexOf("xref", StringComparison.Ordinal);
        var head = Encoding.ASCII.GetBytes(encrypted[..eof]);
        var tail = Encoding.ASCII.GetBytes(encrypted[eof..]);
        var combined = new byte[head.Length + dictionary.Length + tail.Length];
        head.CopyTo(combined, 0);
        dictionary.CopyTo(combined, head.Length);
        tail.CopyTo(combined, head.Length + dictionary.Length);
        return combined;
    }

    private static byte[] Assemble(List<string> objects, bool encrypted)
    {
        var builder = new StringBuilder();
        builder.Append("%PDF-1.4\n");
        var offsets = new List<int> { 0 };
        for (var index = 0; index < objects.Count; index++)
        {
            offsets.Add(Encoding.ASCII.GetByteCount(builder.ToString()));
            builder.Append(index + 1).Append(" 0 obj\n").Append(objects[index]).Append("\nendobj\n");
        }

        var xref = Encoding.ASCII.GetByteCount(builder.ToString());
        builder.Append("xref\n0 ").Append(objects.Count + 1).Append('\n');
        builder.Append("0000000000 65535 f \n");
        foreach (var offset in offsets.Skip(1))
        {
            builder.Append(offset.ToString("D10")).Append(" 00000 n \n");
        }

        builder.Append("trailer\n<< /Size ").Append(objects.Count + 1).Append(" /Root 1 0 R >>\nstartxref\n");
        builder.Append(xref).Append("\n%%EOF");
        return Encoding.ASCII.GetBytes(builder.ToString());
    }

    private static string Escape(string value) =>
        value.Replace("\\", "\\\\", StringComparison.Ordinal)
            .Replace("(", "\\(", StringComparison.Ordinal)
            .Replace(")", "\\)", StringComparison.Ordinal);
}

using System.Text;
using TenderEvidenceChecker.Api.Services;

namespace TenderEvidenceChecker.Api.Tests;

public sealed class PdfTextTests
{
    // Every word is placed by position and the PDF has no space characters at all.
    private static byte[] PositionedWordsPdf(string[] words)
    {
        var content = new StringBuilder("BT /F1 12 Tf\n");
        var x = 40;
        foreach (var word in words)
        {
            content.Append($"1 0 0 1 {x} 700 Tm ({word}) Tj\n");
            x += word.Length * 7 + 12;
        }

        content.Append("ET");

        var objects = new[]
        {
            "<< /Type /Catalog /Pages 2 0 R >>",
            "<< /Type /Pages /Kids [3 0 R] /Count 1 >>",
            "<< /Type /Page /Parent 2 0 R /MediaBox [0 0 842 842] /Contents 4 0 R /Resources << /Font << /F1 5 0 R >> >> >>",
            $"<< /Length {content.Length} >>\nstream\n{content}\nendstream",
            "<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica >>",
        };

        var pdf = new StringBuilder("%PDF-1.4\n");
        var offsets = new List<int>();
        for (var i = 0; i < objects.Length; i++)
        {
            offsets.Add(pdf.Length);
            pdf.Append($"{i + 1} 0 obj\n{objects[i]}\nendobj\n");
        }

        var xref = pdf.Length;
        pdf.Append($"xref\n0 {objects.Length + 1}\n0000000000 65535 f \n");
        foreach (var offset in offsets)
        {
            pdf.Append($"{offset:D10} 00000 n \n");
        }

        pdf.Append($"trailer\n<< /Size {objects.Length + 1} /Root 1 0 R >>\nstartxref\n{xref}\n%%EOF");
        return Encoding.ASCII.GetBytes(pdf.ToString());
    }

    [Fact]
    public void Words_placed_by_position_keep_their_spaces()
    {
        var words = new[] { "Last", "Application", "Date", "is", "09", "November", "2026", "at", "15:00", "for", "all", "bidders", "and", "suppliers" };
        var path = Path.Combine(Path.GetTempPath(), $"tec-{Guid.NewGuid():N}.pdf");
        File.WriteAllBytes(path, PositionedWordsPdf(words));
        try
        {
            var page = Assert.Single(new PdfTextService().Extract(path, ocrEnabled: false));
            Assert.True(page.Usable);
            Assert.Contains("Last Application Date is 09 November 2026 at 15:00", page.Text);
        }
        finally
        {
            File.Delete(path);
        }
    }
}

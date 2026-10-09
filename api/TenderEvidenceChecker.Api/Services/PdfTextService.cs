using UglyToad.PdfPig;
using UglyToad.PdfPig.Exceptions;

namespace TenderEvidenceChecker.Api.Services;

public sealed record ExtractedPage(int PageNumber, string Text, bool Usable, string ExtractionMethod, string OcrStatus);

public sealed class PdfTextService
{
    public int CountPages(string path)
    {
        using var document = Open(path);
        return document.NumberOfPages;
    }

    public IReadOnlyList<ExtractedPage> Extract(string path, bool ocrEnabled)
    {
        using var document = Open(path);
        var pages = new List<ExtractedPage>();
        foreach (var page in document.GetPages())
        {
            var text = page.Text ?? "";
            if (text.Length > 80_000)
            {
                text = text[..80_000];
            }

            var usable = TextNorm.HasReadableLetters(text);
            pages.Add(new ExtractedPage(
                page.Number,
                text,
                usable,
                usable ? "pdfpig" : "none",
                usable ? "not_needed" : ocrEnabled ? "unavailable" : "unavailable"));
        }

        return pages;
    }

    private static PdfDocument Open(string path)
    {
        try
        {
            return PdfDocument.Open(path);
        }
        catch (PdfDocumentEncryptedException)
        {
            throw new AppException(
                "pdf_encrypted",
                "This PDF is password-protected. Remove the password and upload it again. The file was not kept.",
                retryable: false);
        }
        catch (AppException)
        {
            throw;
        }
        catch (Exception)
        {
            throw new AppException(
                "pdf_unreadable",
                "This PDF could not be read. It may be corrupt. Try another export of the file. The file was not kept.",
                retryable: false);
        }
    }
}

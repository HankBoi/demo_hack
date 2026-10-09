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
            var text = ReadableText(page);
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

    /// <summary>
    /// Some PDFs place every word by position and contain no space characters, so the raw page text comes out as
    /// "SonMüraciətTarixi:09Noyabr2026". When the raw text has too few spaces, rebuild the words from glyph positions.
    /// </summary>
    private static string ReadableText(UglyToad.PdfPig.Content.Page page)
    {
        var raw = page.Text ?? "";
        if (!LooksSpaceStarved(raw))
        {
            return raw;
        }

        try
        {
            var rebuilt = UglyToad.PdfPig.DocumentLayoutAnalysis.TextExtractor.ContentOrderTextExtractor.GetText(page);
            return string.IsNullOrWhiteSpace(rebuilt) ? raw : rebuilt;
        }
        catch (Exception)
        {
            return raw;
        }
    }

    private static bool LooksSpaceStarved(string text)
    {
        var letters = 0;
        var spaces = 0;
        foreach (var ch in text)
        {
            if (char.IsLetter(ch))
            {
                letters++;
            }
            else if (char.IsWhiteSpace(ch))
            {
                spaces++;
            }
        }

        return letters >= 40 && spaces * 100 < letters * 6;
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

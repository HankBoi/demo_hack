using System.Security.Cryptography;
using Microsoft.Extensions.Options;
using TenderEvidenceChecker.Api.Data;
using TenderEvidenceChecker.Api.Options;

namespace TenderEvidenceChecker.Api.Services;

public sealed class DocumentUpload(IOptions<AppOptions> options, PdfTextService pdf, FileStore store)
{
    public int FileLimit => Math.Max(1, options.Value.MaxEvidenceFiles);

    public async Task<StoredFileEntity> SaveAsync(
        Guid analysisId,
        IFormFile file,
        string role,
        bool extractText,
        CancellationToken cancellationToken)
    {
        var (original, bytes) = await ReadValidatedAsync(file, cancellationToken);
        return await SaveBytesAsync(analysisId, original, bytes, role, extractText, cancellationToken);
    }

    /// <summary>Stores validated PDF bytes under an analysis with an opaque generated filename.</summary>
    public async Task<StoredFileEntity> SaveBytesAsync(
        Guid analysisId,
        string originalName,
        byte[] bytes,
        string role,
        bool extractText,
        CancellationToken cancellationToken)
    {
        var settings = options.Value;
        var storedName = Guid.NewGuid().ToString("N") + ".pdf";
        var path = store.PathFor(analysisId, storedName);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        await File.WriteAllBytesAsync(path, bytes, cancellationToken);

        try
        {
            var pageCount = CheckPages(path);
            var entity = new StoredFileEntity
            {
                Id = Guid.NewGuid(),
                AnalysisId = analysisId,
                Role = role,
                OriginalName = originalName,
                StoredName = storedName,
                Sha256 = Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant(),
                SizeBytes = bytes.Length,
                PageCount = pageCount,
                Status = "stored"
            };

            if (extractText)
            {
                var pages = pdf.Extract(path, settings.OcrEnabled);
                entity.Status = pages.Any(page => page.Usable) ? "extracted" : "unreadable";
                foreach (var page in pages)
                {
                    entity.Pages.Add(new PageTextEntity
                    {
                        Id = Guid.NewGuid(),
                        PageNumber = page.PageNumber,
                        Text = page.Text,
                        Usable = page.Usable,
                        ExtractionMethod = page.ExtractionMethod,
                        OcrStatus = page.OcrStatus
                    });
                }
            }

            return entity;
        }
        catch
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }

            throw;
        }
    }

    /// <summary>Validates and stores a company document in the workspace library (outside any analysis).</summary>
    public async Task<LibraryDocumentEntity> SaveLibraryAsync(
        Guid workspaceId,
        IFormFile file,
        CancellationToken cancellationToken)
    {
        var (original, bytes) = await ReadValidatedAsync(file, cancellationToken);
        var storedName = Guid.NewGuid().ToString("N") + ".pdf";
        var path = store.LibraryPath(storedName);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        await File.WriteAllBytesAsync(path, bytes, cancellationToken);
        try
        {
            var pageCount = CheckPages(path);
            return new LibraryDocumentEntity
            {
                Id = Guid.NewGuid(),
                WorkspaceId = workspaceId,
                OriginalName = original,
                StoredName = storedName,
                Sha256 = Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant(),
                SizeBytes = bytes.Length,
                PageCount = pageCount,
                CreatedAt = DateTime.UtcNow
            };
        }
        catch
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }

            throw;
        }
    }

    private int CheckPages(string path)
    {
        var settings = options.Value;
        var pageCount = pdf.CountPages(path);
        if (pageCount <= 0)
        {
            throw new AppException("pdf_unreadable", "The PDF has no pages. Try another file.", false);
        }

        if (pageCount > settings.MaxTenderPages)
        {
            throw new AppException(
                "page_limit_exceeded",
                $"This PDF has {pageCount} pages. The demo limit is {settings.MaxTenderPages} pages.",
                false);
        }

        return pageCount;
    }

    private async Task<(string Original, byte[] Bytes)> ReadValidatedAsync(IFormFile file, CancellationToken cancellationToken)
    {
        var settings = options.Value;
        if (file.Length <= 0)
        {
            throw new AppException("pdf_unreadable", "The file is empty. Choose a PDF with text.", false);
        }

        if (file.Length > settings.MaxUploadBytes)
        {
            throw new AppException(
                "file_too_large",
                $"The file is larger than {settings.MaxUploadMb} MB. Upload a smaller PDF.",
                false);
        }

        var original = TextNorm.SafeFileName(file.FileName);
        if (!original.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
        {
            throw new AppException(
                "unsupported_file_type",
                "Only PDF files are accepted. The extension and the file contents need to match.",
                false);
        }

        await using var buffer = new MemoryStream();
        await file.CopyToAsync(buffer, cancellationToken);
        if (buffer.Length > settings.MaxUploadBytes)
        {
            throw new AppException(
                "file_too_large",
                $"The file is larger than {settings.MaxUploadMb} MB. Upload a smaller PDF.",
                false);
        }

        var bytes = buffer.ToArray();
        if (bytes.Length < 5 || bytes[0] != (byte)'%' || bytes[1] != (byte)'P' || bytes[2] != (byte)'D' || bytes[3] != (byte)'F')
        {
            throw new AppException(
                "unsupported_file_type",
                "The file does not look like a PDF. Check the export and try again. It was not kept.",
                false);
        }

        return (original, bytes);
    }
}

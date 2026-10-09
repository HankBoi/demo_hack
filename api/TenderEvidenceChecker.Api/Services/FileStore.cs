using Microsoft.Extensions.Options;
using TenderEvidenceChecker.Api.Options;

namespace TenderEvidenceChecker.Api.Services;

/// <summary>Private upload storage. Everything lives under PrivateUploadDir, which is never served as static files.</summary>
public sealed class FileStore(IOptions<AppOptions> options)
{
    private const string LibraryFolder = "library";

    public string PathFor(Guid analysisId, string storedName)
    {
        return Path.Combine(options.Value.PrivateUploadDir, analysisId.ToString("N"), storedName);
    }

    public string LibraryPath(string storedName)
    {
        return Path.Combine(options.Value.PrivateUploadDir, LibraryFolder, storedName);
    }

    public void DeleteLibraryFile(string storedName)
    {
        var path = LibraryPath(storedName);
        if (File.Exists(path))
        {
            File.Delete(path);
        }
    }

    public void DeleteAnalysis(Guid analysisId)
    {
        var directory = Path.Combine(options.Value.PrivateUploadDir, analysisId.ToString("N"));
        if (Directory.Exists(directory))
        {
            Directory.Delete(directory, recursive: true);
        }
    }
}

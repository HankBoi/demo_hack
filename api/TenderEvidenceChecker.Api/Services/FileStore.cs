using Microsoft.Extensions.Options;
using TenderEvidenceChecker.Api.Options;

namespace TenderEvidenceChecker.Api.Services;

public sealed class FileStore(IOptions<AppOptions> options)
{
    public string PathFor(Guid analysisId, string storedName)
    {
        return Path.Combine(options.Value.PrivateUploadDir, analysisId.ToString("N"), storedName);
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

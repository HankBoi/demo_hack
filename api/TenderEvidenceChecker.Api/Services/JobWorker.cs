namespace TenderEvidenceChecker.Api.Services;

public sealed class JobWorker(AnalysisProcessor processor, ILogger<JobWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Tender evidence worker started");
        while (!stoppingToken.IsCancellationRequested)
        {
            var worked = false;
            try
            {
                worked = await processor.TryProcessNext(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogWarning("Worker loop error of type {ExceptionType}", ex.GetType().Name);
            }

            if (!worked)
            {
                await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
            }
        }
    }
}

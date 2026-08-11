namespace LedgerLens.SlipImport.Worker;

public sealed class FoundationWorker(ILogger<FoundationWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Slip import foundation worker started; no business subscriptions are registered.");
        await Task.Delay(Timeout.InfiniteTimeSpan, stoppingToken);
    }
}

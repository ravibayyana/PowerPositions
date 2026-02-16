using PowerPositions.Services;

namespace PowerPositions;

public class Worker(IJobScheduler jobScheduler) : BackgroundService
{
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        jobScheduler.Start(stoppingToken);

        stoppingToken.Register(() => (jobScheduler as IDisposable)?.Dispose());

        return Task.CompletedTask;
    }
}
namespace PowerPositions.Services;

public interface IJobScheduler
{
    void Start(CancellationToken stoppingToken);
}
namespace PowerPositions.Services;

public interface IPowerPositionService
{
    Task Run(CancellationToken token);
}
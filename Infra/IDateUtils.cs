namespace PowerPositions.Infra;

public interface IDateUtils
{
    //DateTime Now { get;  }
    DateTime UtcNow { get; }
    Task Delay(TimeSpan delay, CancellationToken cancellationToken);
}
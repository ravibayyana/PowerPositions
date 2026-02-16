namespace PowerPositions.Infra;

public class DateUtils : IDateUtils
{
    public DateTime Now => DateTime.Now;
    public DateTime UtcNow => DateTime.UtcNow;

    public Task Delay(TimeSpan delay, CancellationToken cancellationToken)
    {
        return Task.Delay(delay, cancellationToken);
    }
}
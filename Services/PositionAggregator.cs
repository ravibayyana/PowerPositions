using PowerPositions.Models;
using PowerPositions.Utils;
using Services;

namespace PowerPositions.Services;

public class PositionAggregator(ILogger<PositionAggregator> logger) : IPositionAggregator
{
    public Task<List<HourlyPowerPosition>> AggregatePositionsAsync(IEnumerable<PowerTrade> trades,
        DateTime referenceDate,
        CancellationToken cancellationToken)
    {
        logger.LogDebug($"Starting aggregation for {referenceDate.ToDD_MMM_YYYY()}");

        var tradesList = trades.ToList();

        if (!tradesList.Any())
        {
            logger.LogWarning($"No trades found for date: {referenceDate.ToDD_MMM_YYYY()}");
            return Task.FromResult(new List<HourlyPowerPosition>());
        }
        
        var periodVolumes = new Dictionary<int, double>();
        foreach (var trade in tradesList)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (trade.Periods is not { Length: > 0 })
                continue;

            foreach (var period in trade.Periods)
            {
                cancellationToken.ThrowIfCancellationRequested();

                periodVolumes[period.Period] = periodVolumes.TryGetValue(period.Period, out var existing)
                    ? existing + period.Volume
                    : period.Volume;
            }
        }

        var hourlyPositions = new List<HourlyPowerPosition>();
        var startDateTime = referenceDate.Date.ToLocalTime().AddDays(-1).AddHours(23);

        foreach (var kvp in periodVolumes.OrderBy(x => x.Key))
        {
            cancellationToken.ThrowIfCancellationRequested();

            var period = kvp.Key;
            var volume = kvp.Value;

            var periodDateTime = startDateTime.AddHours(period - 1);
            var localTime = periodDateTime.ToString("HH:mm");

            var hourlyPosition = new HourlyPowerPosition
            {
                Period = period,
                LocalTime = localTime,
                Volume = volume
            };

            hourlyPositions.Add(hourlyPosition);
            logger.LogDebug($"{hourlyPosition}");
        }

        return Task.FromResult(hourlyPositions);
    }
}
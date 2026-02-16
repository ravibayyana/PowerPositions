using PowerPositions.Models;
using Services;

namespace PowerPositions.Services;

public interface IPositionAggregator
{
    Task<List<HourlyPowerPosition>> AggregatePositionsAsync(
        IEnumerable<PowerTrade> trades, 
        DateTime referenceDate,
        CancellationToken cancellationToken = default);
}
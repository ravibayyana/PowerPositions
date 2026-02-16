using Services;

namespace PowerPositions.Services;

public interface IPowerTradeService
{
    Task<IEnumerable<PowerTrade>> GetTradesAsync(DateTime date, CancellationToken cancellationToken = default);
}
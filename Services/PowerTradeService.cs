using Services;

namespace PowerPositions.Services;

public class PowerTradeService(IPowerService powerService) : IPowerTradeService
{ 
    public async Task<IEnumerable<PowerTrade>> GetTradesAsync(DateTime date, CancellationToken cancellationToken)
    {
        try
        {
            var trades = await powerService.GetTradesAsync(date).ConfigureAwait(false);
            var tradesList = trades.ToList();
            
            cancellationToken.ThrowIfCancellationRequested();

            return tradesList;
        }
        catch
        {
            throw;
        }
    }
}
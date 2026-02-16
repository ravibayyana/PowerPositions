using Microsoft.Extensions.Logging;
using PowerPositions.Services;
using Services;

namespace PowerPositions.Tests;

public class PositionAggregatorTests
{
    [Fact]
    public async Task NoTrades_ReturnsEmptyAndLogsWarning()
    {
        // Arrange
        var logger = new TestLogger<PositionAggregator>();
        var sut = new PositionAggregator(logger);

        var trades = Array.Empty<PowerTrade>();
        var referenceDate = new DateTime(2026, 01, 02);

        // Act
        var result = await sut.AggregatePositionsAsync(trades, referenceDate, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
        Assert.Contains(logger.Entries, e =>
            e.Level == LogLevel.Warning && e.Message.Contains("No trades found", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task  MultipleTrades_AggregatesVolumesAndProducesLocalTimes()
    {
        // Arrange
        var logger = new TestLogger<PositionAggregator>();
        var sut = new PositionAggregator(logger);

        var referenceDate = new DateTime(2026, 01, 02);
        
        var trade1 = CreatePowerTrade(referenceDate, 10d, 5d); 
        var trade2 = CreatePowerTrade(referenceDate, 3d);   


        var trades = new[] { trade1, trade2 };

        // Act
        var result = await sut.AggregatePositionsAsync(trades, referenceDate, CancellationToken.None);

        Assert.Equal(2, result.Count);

        var p1 = result[0];
        var p2 = result[1];

        Assert.Equal(1, p1.Period);
        Assert.Equal(13d, p1.Volume); 
        Assert.Equal("23:00", p1.LocalTime); 

        Assert.Equal(2, p2.Period);
        Assert.Equal(5d, p2.Volume);
        Assert.Equal("00:00", p2.LocalTime); 
    }


    private static PowerTrade CreatePowerTrade(DateTime date, params double[] volumes)
    {
        var trade = PowerTrade.Create(date, volumes.Length);

        for (var i = 0; i < volumes.Length; i++)
        {
            trade.Periods[i].Volume = volumes[i];
        }

        return trade;
    }
}
using Microsoft.Extensions.Options;
using Moq;
using PowerPositions.Infra;
using PowerPositions.Models;
using PowerPositions.Services;
using Services;
using System;

namespace PowerPositions.Tests;
public class PowerPositionServiceTests
{
    private readonly PowerPositionSettings _settings = new()
    {
        CsvOutputPath = "out/path",
        ExtractIntervalMinutes = 5
    };

    private IOptions<PowerPositionSettings> CreateOptions()
    {
        return Options.Create(_settings);
    }

    [Fact]
    public async Task Run_WhenNoPositions_LogsWarningAndDoesNotCallExporter()
    {
        // Arrange
        var jobStart = new DateTime(2024, 01, 02, 03, 04, 05, DateTimeKind.Utc);

        var mockTradeService = new Mock<IPowerTradeService>();
        mockTradeService
            .Setup(s => s.GetTradesAsync(It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Enumerable.Empty<PowerTrade>());

        var mockAggregator = new Mock<IPositionAggregator>();
        mockAggregator
            .Setup(a => a.AggregatePositionsAsync(It.IsAny<IEnumerable<PowerTrade>>(), It.IsAny<DateTime>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<HourlyPowerPosition>()); 

        var mockCsv = new Mock<ICsvExporter>(MockBehavior.Strict);

        var mockDateUtils = new Mock<IDateUtils>();
        mockDateUtils.SetupSequence(d => d.UtcNow).Returns(jobStart);

        var logger = new TestLogger<PowerPositionService>();

        var sut = new PowerPositionService(
            mockTradeService.Object,
            mockAggregator.Object,
            mockCsv.Object,
            mockDateUtils.Object,
            CreateOptions(),
            logger);

        var token = CancellationToken.None;

        // Act
        await sut.Run(token);

        // Assert
        mockTradeService.Verify(s => s.GetTradesAsync(jobStart.Date, token), Times.Once);
        mockAggregator.Verify(a => a.AggregatePositionsAsync(It.IsAny<IEnumerable<PowerTrade>>(), jobStart.Date, token),
            Times.Once);
        mockCsv.Verify(
            c => c.ExportToCsvAsync(It.IsAny<DateTime>(), It.IsAny<List<HourlyPowerPosition>>(),
                It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);

        Assert.Contains(logger.Entries,
            e => e.Level == Microsoft.Extensions.Logging.LogLevel.Warning && e.Message.Contains("No positions to export"));
    }

    [Fact]
    public async Task Run_WhenPositionsExist_ExportsCsvAndLogsCompletion()
    {
        // Arrange
        var jobStart = new DateTime(2024, 02, 10, 10, 0, 0, DateTimeKind.Utc);
        var jobEnd = jobStart.AddSeconds(2);

        var trades = Array.Empty<PowerTrade>();
        var positions = new List<HourlyPowerPosition>
        {
            new() 
        };

        var mockTradeService = new Mock<IPowerTradeService>();
        mockTradeService
            .Setup(s => s.GetTradesAsync(It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(trades);

        var mockAggregator = new Mock<IPositionAggregator>();
        mockAggregator
            .Setup(a => a.AggregatePositionsAsync(trades, jobStart.Date, It.IsAny<CancellationToken>()))
            .ReturnsAsync(positions);

        var expectedCsvPath = "out/path/file.csv";
        var mockCsv = new Mock<ICsvExporter>();
        mockCsv
            .Setup(c => c.ExportToCsvAsync(jobStart, positions, _settings.CsvOutputPath, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedCsvPath);

        var mockDateUtils = new Mock<IDateUtils>();
        mockDateUtils.SetupSequence(d => d.UtcNow).Returns(jobStart).Returns(jobEnd);

        var logger = new TestLogger<PowerPositionService>();

        var sut = new PowerPositionService(
            mockTradeService.Object,
            mockAggregator.Object,
            mockCsv.Object,
            mockDateUtils.Object,
            CreateOptions(),
            logger);

        var token = CancellationToken.None;

        // Act
        await sut.Run(token);

        // Assert
        mockCsv.Verify(c => c.ExportToCsvAsync(jobStart, positions, _settings.CsvOutputPath, token), Times.Once);

        Assert.Contains(logger.Entries,
            e => e.Level == Microsoft.Extensions.Logging.LogLevel.Information && e.Message.Contains("Power Position Job Completed Successfully"));
        
    }

    [Fact]
    public async Task Run_WhenException_LogsError()
    {
        // Arrange
        var jobStart = DateTime.UtcNow;
        var thrown = new InvalidOperationException("boom");

        var mockTradeService = new Mock<IPowerTradeService>();
        mockTradeService
            .Setup(s => s.GetTradesAsync(It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(thrown);

        var mockAggregator = new Mock<IPositionAggregator>(MockBehavior.Loose);
        var mockCsv = new Mock<ICsvExporter>(MockBehavior.Loose);

        var mockDateUtils = new Mock<IDateUtils>();
        mockDateUtils.Setup(d => d.UtcNow).Returns(jobStart);

        var logger = new TestLogger<PowerPositionService>();

        var sut = new PowerPositionService(
            mockTradeService.Object,
            mockAggregator.Object,
            mockCsv.Object,
            mockDateUtils.Object,
            CreateOptions(),
            logger);

        // Act
        await sut.Run(CancellationToken.None);

        var errorEntry = logger.Entries.FirstOrDefault(e => e.Level == Microsoft.Extensions.Logging.LogLevel.Error);
        Assert.NotNull(errorEntry);
        Assert.Same(thrown, errorEntry!.Exception);
        Assert.Contains("CRITICAL ERROR", errorEntry.Message, StringComparison.OrdinalIgnoreCase);
    }
}
using Microsoft.Extensions.Options;
using Microsoft.Reactive.Testing;
using Moq;
using PowerPositions.Infra;
using PowerPositions.Services;

namespace PowerPositions.Tests;

public class JobSchedulerTests
{
    [Fact]
    public void Start_SchedulesImmediateAndPeriodicExecution()
    {
        // Arrange
        var settings = new PowerPositionSettings {ExtractIntervalMinutes = 1};
        var options = Options.Create(settings);
        var testScheduler = new TestScheduler();

        var runCount = 0;
        var capturedTokens = new List<CancellationToken>();
        var mockService = new Mock<IPowerPositionService>();
        mockService
            .Setup(s => s.Run(It.IsAny<CancellationToken>()))
            .Returns<CancellationToken>(ct =>
            {
                runCount++;
                capturedTokens.Add(ct);
                return Task.CompletedTask;
            });

        var sut = new JobScheduler(options, mockService.Object, testScheduler);

        // Act
        sut.Start(CancellationToken.None);


        testScheduler.AdvanceTo(1);

        Assert.Equal(1, runCount);
        Assert.Single(capturedTokens);
        Assert.False(capturedTokens[0].IsCancellationRequested);


        testScheduler.AdvanceBy(TimeSpan.FromMinutes(settings.ExtractIntervalMinutes).Ticks);
        Assert.Equal(2, runCount);
    }

    [Fact]
    public void Dispose_UnsubscribesAndPreventsFurtherRuns()
    {
        var settings = new PowerPositionSettings {ExtractIntervalMinutes = 1};
        var options = Options.Create(settings);
        var testScheduler = new TestScheduler();

        var runCount = 0;
        var mockService = new Mock<IPowerPositionService>();
        mockService
            .Setup(s => s.Run(It.IsAny<CancellationToken>()))
            .Returns(() =>
            {
                runCount++;
                return Task.CompletedTask;
            });

        var sut = new JobScheduler(options, mockService.Object, testScheduler);

        // Act
        sut.Start(CancellationToken.None);

        testScheduler.AdvanceTo(1);
        Assert.Equal(1, runCount);

        sut.Dispose();

        testScheduler.AdvanceBy(TimeSpan.FromMinutes(settings.ExtractIntervalMinutes).Ticks);
        Assert.Equal(1, runCount);
    }
}
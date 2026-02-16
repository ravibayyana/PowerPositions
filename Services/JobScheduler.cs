using System.Reactive.Concurrency;
using System.Reactive.Linq;
using Microsoft.Extensions.Options;
using PowerPositions.Infra;

namespace PowerPositions.Services;

public sealed class JobScheduler(
    IOptions<PowerPositionSettings> settings,
    IPowerPositionService positionService,
    IScheduler scheduler)
    : IDisposable, IJobScheduler
{
    private IDisposable? _subscription;

    public void Start(CancellationToken stoppingToken)
    {
        var interval = TimeSpan.FromMinutes(settings.Value.ExtractIntervalMinutes);

        _subscription = Observable
            .Timer(TimeSpan.Zero, interval, scheduler)
            .SelectMany(_ =>
                Observable.FromAsync(() => positionService.Run(stoppingToken)))
            .Subscribe();
    }

    public void Dispose()
    {
        _subscription?.Dispose();
    }
}
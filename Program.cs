using Microsoft.Extensions.Options;
using PowerPositions;
using PowerPositions.Infra;
using PowerPositions.Services;
using PowerPositions.Utils;
using Serilog;
using Services;
using System.Reactive.Concurrency;

try
{
    var baseDir = AppContext.BaseDirectory;

    var builder = Host.CreateApplicationBuilder(args);

    builder.RegisterAsWindowService(baseDir);
    builder.RegisterLogging(baseDir);

    Log.Information("**** Power Position Service Starting ****");
    Log.Information($"CurrentDir: {baseDir}");

    builder.Services.AddHostedService<Worker>();
    builder.Services.AddSingleton<IPowerTradeService, PowerTradeService>();
    builder.Services.AddSingleton<IPositionAggregator, PositionAggregator>();
    builder.Services.AddSingleton<ICsvExporter, CsvExporter>();
    builder.Services.AddSingleton<IPowerService, PowerService>();
    builder.Services.AddSingleton<IDateUtils, DateUtils>();
    builder.Services.AddSingleton<IPowerPositionService, PowerPositionService>();
    builder.Services.AddSingleton<IJobScheduler, JobScheduler>();
    builder.Services.AddSingleton<IScheduler>(TaskPoolScheduler.Default);
    builder.Services.Configure<PowerPositionSettings>(builder.Configuration.GetSection("PowerPositionSettings"));

    var host = builder.Build();

    var settings = host.Services.GetRequiredService<IOptions<PowerPositionSettings>>().Value;
    settings.Validate();

    Log.Information("Configuration validated successfully");
    Log.Information("CSV Output Path: {Path}", settings.CsvOutputPath);
    Log.Information("Extract Interval: {Interval} minutes", settings.ExtractIntervalMinutes);

    await host.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.Information("Power Position Service Shutting Down");
    await Log.CloseAndFlushAsync();
}
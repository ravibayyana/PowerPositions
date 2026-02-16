using Serilog;

namespace PowerPositions.Utils;

public static class Extensions
{
    public static void RegisterLogging(this HostApplicationBuilder builder, string baseDir)
    {
        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(builder.Configuration)
            .Enrich.WithProperty("BASEDIR", baseDir)
            .CreateLogger();

        builder.Logging.ClearProviders();
        builder.Logging.AddSerilog(Log.Logger);
    }

    public static void RegisterAsWindowService(this HostApplicationBuilder builder, string baseDir)
    {
        builder.Services.AddWindowsService(options =>
        {
            options.ServiceName = "PowerPositionService";
        });

        Directory.SetCurrentDirectory(baseDir);
    }

    public static string ToDD_MMM_YYYY(this DateTime date)
    {
        return $"{date:dd-MMM-yyyy}";
    }

    public static string To_DD_MMM_YYYY_Time(this DateTime date)
    {
        return $"{date:dd-MMM-yyy HH:mm:ss}";
    }

    public static void CustomLogInfo<T>(this ILogger<T> logger, DateTime date, string msg)
    {
        logger.LogInformation($"[{date.To_DD_MMM_YYYY_Time()}] - {msg}");
    }


    public static void CustomLogWarning<T>(this ILogger<T> logger, DateTime date, string msg)
    {
        logger.LogWarning($"[{date.To_DD_MMM_YYYY_Time()}] - {msg}");
    }

}
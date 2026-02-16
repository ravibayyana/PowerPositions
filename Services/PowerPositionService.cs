using Microsoft.Extensions.Options;
using PowerPositions.Infra;
using PowerPositions.Utils;

namespace PowerPositions.Services;

public class PowerPositionService(
    IPowerTradeService powerTradeService,
    IPositionAggregator positionAggregator,
    ICsvExporter csvExporter,
    IDateUtils dateUtils,
    IOptions<PowerPositionSettings> settings,
    ILogger<PowerPositionService> logger)
    : IPowerPositionService
{
    public async Task Run(CancellationToken token)
    {
        var jobStart = dateUtils.UtcNow;
        
        logger.CustomLogInfo(jobStart, "========================================");
        logger.CustomLogInfo(jobStart, "Power Position Job Started");

        try
        {
            var jobStartDate = jobStart.Date;

            logger.LogDebug($"Extracting power positions for date: {jobStartDate.ToDD_MMM_YYYY()}");
            logger.LogDebug($"Retrieving power trades for date: {jobStartDate.ToDD_MMM_YYYY()}");

            var trades = await powerTradeService.GetTradesAsync(jobStartDate, token)
                                            .ConfigureAwait(false);

            logger.CustomLogInfo(jobStart, $"Got {trades.Count()} power trades");

            var hourlyPositions = await positionAggregator.AggregatePositionsAsync(trades, jobStartDate, token)
                .ConfigureAwait(false);

            logger.CustomLogInfo(jobStart, "Aggregation complete");

            if (!hourlyPositions.Any())
            {
                logger.CustomLogWarning(jobStart, "No positions to export");
                return;
            }

            var csvFilePath = await csvExporter.ExportToCsvAsync(jobStart, hourlyPositions, 
                settings.Value.CsvOutputPath, token).ConfigureAwait(false);

            var jobEnd = dateUtils.UtcNow;
            var duration = jobEnd - jobStart;
            
            logger.CustomLogInfo(jobStart, $"CSV File: {csvFilePath}");
            logger.CustomLogInfo(jobStart, $"Duration: {duration.TotalMilliseconds}ms");
            logger.CustomLogInfo(jobStart, "Power Position Job Completed Successfully");
        }
        catch (OperationCanceledException) when (token.IsCancellationRequested)
        {
            logger.CustomLogInfo(jobStart, $"Power Position Job cancelled");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, $"[{jobStart.To_DD_MMM_YYYY_Time()}]- CRITICAL ERROR: Power Position Job Failed");
        }
        finally
        {
            logger.CustomLogInfo(jobStart, $"========================================{Environment.NewLine}");
        }
    }
}
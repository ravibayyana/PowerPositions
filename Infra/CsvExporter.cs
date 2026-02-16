using System.Globalization;
using System.Text;
using PowerPositions.Models;

namespace PowerPositions.Infra;

public class CsvExporter(ILogger<CsvExporter> logger) : ICsvExporter
{
    public async Task<string> ExportToCsvAsync(DateTime date, List<HourlyPowerPosition> positions, string outputPath,
        CancellationToken cancellationToken = default)
    {
        var fileName = $"PowerPosition_{date:yyyyMMdd}_{date:HHmm}.csv";
        var fullPath = Path.Combine(outputPath, fileName);

        logger.LogDebug($"Exporting {positions.Count} positions to {fullPath}");

        try
        {
            Directory.CreateDirectory(outputPath);
            var csvContent = new StringBuilder();

            csvContent.AppendLine("Local Time,Volume");

            foreach (var position in positions.OrderBy(p => p.Period))
            {
                var volumeFormatted = position.Volume.ToString("F4", CultureInfo.InvariantCulture);
                csvContent.AppendLine($"{position.LocalTime},{volumeFormatted}");
            }

            await File.WriteAllTextAsync(fullPath, csvContent.ToString(), Encoding.UTF8, cancellationToken)
                .ConfigureAwait(false);

            logger.LogDebug($"Successfully exported CSV to {fullPath}");

            return fullPath;
        }
        catch (Exception)
        {
            throw;
        }
    }
}
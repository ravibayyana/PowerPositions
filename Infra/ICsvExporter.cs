using PowerPositions.Models;

namespace PowerPositions.Infra;

public interface ICsvExporter
{
    Task<string> ExportToCsvAsync(DateTime date, List<HourlyPowerPosition> positions, string outputPath,
        CancellationToken cancellationToken = default);
}
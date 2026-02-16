namespace PowerPositions.Infra;

public class PowerPositionSettings
{
    public string CsvOutputPath { get; set; } = string.Empty;
    public int ExtractIntervalMinutes { get; set; } = 5;

    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(CsvOutputPath))
            throw new InvalidOperationException("CsvOutputPath must be configured in AppSettings.json");

        if (ExtractIntervalMinutes <= 0)
            throw new InvalidOperationException("ExtractIntervalMinutes must be greater than 0");
    }
}
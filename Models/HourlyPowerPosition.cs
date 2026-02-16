namespace PowerPositions.Models;

public class HourlyPowerPosition
{
    public string LocalTime { get; set; } = string.Empty;

    public double Volume { get; set; }

    public int Period { get; set; }

    public override string ToString()
    {
        return $"Period {Period}: Time={LocalTime}, Volume={Volume}";
    }
}
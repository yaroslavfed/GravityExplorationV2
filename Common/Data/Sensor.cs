namespace Common.Data;

public record Sensor
{
    public double X { get; init; }

    public double Y { get; init; }

    public double Z { get; init; } = 0;

    public double Value { get; set; }
}
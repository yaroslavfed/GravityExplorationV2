namespace Common.Data;

public record Sensor
{
    public double X { get; init; }

    public double Y { get; init; }

    public double Z { get; init; }

    public double Value { get; init; } = new Random().Next(0, 7) + new Random().NextDouble();
}
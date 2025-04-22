namespace Common.Data;

public record Cell
{
    public required double CenterX { get; init; }

    public required double CenterY { get; init; }

    public required double CenterZ { get; init; }

    public required double BoundX { get; init; }

    public required double BoundY { get; init; }

    public required double BoundZ { get; init; }

    public double Density { get; set; }
}
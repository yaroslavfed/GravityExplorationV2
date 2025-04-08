namespace Client.Core.Data;

public record SensorsGrid
{
    public required double StartX { get; init; }

    public required double EndX { get; init; }

    public required double SplitsXCount { get; init; }

    public required double StartY { get; init; }

    public required double EndY { get; init; }

    public required double SplitsYCount { get; init; }

    public double StartZ { get; init; } = 0;

    public double EndZ { get; init; } = 0;
}
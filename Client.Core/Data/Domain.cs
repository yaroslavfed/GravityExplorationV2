namespace Client.Core.Data;

public record Domain
{
    public required double StartX { get; init; }

    public required double EndX { get; init; }

    public required double SplitsXCount { get; init; }

    public required double StartY { get; init; }

    public required double EndY { get; init; }

    public required double SplitsYCount { get; init; }

    public required double StartZ { get; init; }

    public required double EndZ { get; init; }

    public required double SplitsZCount { get; init; }
}
namespace Client.Core.Data;

public record Placement
{
    public (double min, double max) BoundsX { get; init; }

    public (double min, double max) BoundsY { get; init; }

    public (double min, double max) BoundsZ { get; init; }
}
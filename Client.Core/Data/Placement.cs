namespace Client.Core.Data;

public record Placement
{
    public (double Min, double Max) BoundsX { get; init; }

    public (double Min, double Max) BoundsY { get; init; }

    public (double Min, double Max) BoundsZ { get; init; }
}
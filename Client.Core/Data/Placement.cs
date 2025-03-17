namespace Client.Core.Data;

public record Placement
{
    public Point3D Position { get; init; }

    public Point3D StepToBound { get; init; }
}
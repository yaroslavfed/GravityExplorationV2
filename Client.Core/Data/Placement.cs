namespace Client.Core.Data;

public record Placement
{
    public required Point3D Position { get; init; }

    public required Point3D StepToBound { get; init; }
}
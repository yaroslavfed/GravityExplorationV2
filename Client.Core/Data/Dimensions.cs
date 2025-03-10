namespace Client.Core.Data;

public class Dimensions
{
    public required Point3D Center { get; init; }

    public Point3D Bounds { get; init; } = new() { X = 0, Y = 0, Z = 0 };
}
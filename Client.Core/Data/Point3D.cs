namespace Client.Core.Data;

public record Point3D : IEquatable<Point3D>
{
    public required double X { get; init; }

    public required double Y { get; init; }

    public required double Z { get; init; }

    public virtual bool Equals(Point3D? other)
    {
        return other is { } &&
               X.Equals(other.X) &&
               Y.Equals(other.Y) &&
               Z.Equals(other.Z);
    }

    public override int GetHashCode() => HashCode.Combine(X, Y, Z);
}
using Client.Core.Data;

namespace Client.Core.Services.MeshService.OctreeStructure;

public class BoundingBox
{
    public double MinX { get; }

    public double MaxX { get; }

    public double MinY { get; }

    public double MaxY { get; }

    public double MinZ { get; }

    public double MaxZ { get; }

    public BoundingBox(double minX, double maxX, double minY, double maxY, double minZ, double maxZ)
    {
        MinX = minX;
        MaxX = maxX;
        MinY = minY;
        MaxY = maxY;
        MinZ = minZ;
        MaxZ = maxZ;
    }

    public bool Contains(double x, double y, double z) =>
        x >= MinX && x <= MaxX && y >= MinY && y <= MaxY && z >= MinZ && z <= MaxZ;

    public bool Intersects(Stratum s) =>
        MaxX >= s.StartX && MinX <= s.EndX && MaxY >= s.StartY && MinY <= s.EndY && MaxZ >= s.StartZ && MinZ <= s.EndZ;

    public BoundingBox[] Subdivide()
    {
        var mx = (MinX + MaxX) / 2;
        var my = (MinY + MaxY) / 2;
        var mz = (MinZ + MaxZ) / 2;

        return
        [
            new(MinX, mx, MinY, my, MinZ, mz),
            new(mx, MaxX, MinY, my, MinZ, mz),
            new(MinX, mx, my, MaxY, MinZ, mz),
            new(mx, MaxX, my, MaxY, MinZ, mz),
            new(MinX, mx, MinY, my, mz, MaxZ),
            new(mx, MaxX, MinY, my, mz, MaxZ),
            new(MinX, mx, my, MaxY, mz, MaxZ),
            new(mx, MaxX, my, MaxY, mz, MaxZ)
        ];
    }
}
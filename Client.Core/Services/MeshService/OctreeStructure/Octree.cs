using Client.Core.Data;

namespace Client.Core.Services.MeshService.OctreeStructure;

public class Octree
{
    private readonly int        _maxDepth;
    private readonly int        _maxStratumPerNode;
    private readonly OctreeNode _root;

    public Octree(BoundingBox bounds, int maxDepth = 8, int maxStratumPerNode = 10)
    {
        _maxDepth = maxDepth;
        _maxStratumPerNode = maxStratumPerNode;
        _root = new(bounds, 0, maxDepth, maxStratumPerNode);
    }

    public void Insert(Stratum stratum) => _root.Insert(stratum);

    public double? FindDensity(double x, double y, double z)
    {
        var s = _root.FindStratumContaining(x, y, z);
        return s?.Density;
    }
}
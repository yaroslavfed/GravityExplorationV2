using Client.Core.Data;

namespace Client.Core.Services.MeshService.OctreeStructure;

internal class OctreeNode
{
    private readonly List<Stratum> _stratums = new();
    private          OctreeNode[]? _children;

    private readonly BoundingBox _bounds;
    private readonly int         _depth;
    private readonly int         _maxDepth;
    private readonly int         _maxStratumPerNode;

    public OctreeNode(BoundingBox bounds, int depth, int maxDepth, int maxStratumPerNode)
    {
        _bounds = bounds;
        _depth = depth;
        _maxDepth = maxDepth;
        _maxStratumPerNode = maxStratumPerNode;
    }

    public void Insert(Stratum stratum)
    {
        if (!_bounds.Intersects(stratum)) return;

        if (_children == null && (_depth >= _maxDepth || _stratums.Count < _maxStratumPerNode))
        {
            _stratums.Add(stratum);
            return;
        }

        if (_children == null)
            Subdivide();

        foreach (var child in _children!)
        {
            child.Insert(stratum);
        }
    }

    public Stratum? FindStratumContaining(double x, double y, double z)
    {
        if (!_bounds.Contains(x, y, z))
            return null;

        if (_children == null)
        {
            return _stratums.FirstOrDefault(
                s => x >= s.StartX && x <= s.EndX && y >= s.StartY && y <= s.EndY && z >= s.StartZ && z <= s.EndZ
            );
        }

        foreach (var child in _children)
        {
            var result = child.FindStratumContaining(x, y, z);
            if (result != null) return result;
        }

        return null;
    }

    private void Subdivide()
    {
        _children = _bounds
                    .Subdivide()
                    .Select(b => new OctreeNode(b, _depth + 1, _maxDepth, _maxStratumPerNode))
                    .ToArray();
    }
}
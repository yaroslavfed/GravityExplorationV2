namespace ReverseProblem.Core.Services.ConverterService;

public record Point3D
{
    public double X { get; init; }

    public double Y { get; init; }

    public double Z { get; init; }
}

public record Node
{
    public int NodeIndex { get; init; }

    public required Point3D Coordinate { get; init; }
}

public record Edge
{
    public int EdgeIndex { get; init; }

    public List<Node> Nodes { get; init; } = [];
}

public record FiniteElement
{
    public List<Edge> Edges { get; init; } = [];

    public double Mu { get; set; }
}

public record Mesh
{
    public List<FiniteElement> Elements { get; init; } = [];
}
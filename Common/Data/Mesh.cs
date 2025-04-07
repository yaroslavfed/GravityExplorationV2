namespace Common.Data;

public record Mesh
{
    public IReadOnlyList<Cell> Cells { get; init; } = [];
}
namespace Common.Models;

/// <summary>
/// Параметры для управления адаптивным уточнением и объединением ячеек сетки.
/// </summary>
public record MeshRefinementOptions
{
    /// <summary>
    /// Максимальный уровень дробления ячейки.
    /// </summary>
    public int MaxSubdivisionLevel { get; init; } = 5;

    /// <summary>
    /// Минимальная доля размера оси для ячеек (например, 0.1 = 10% от размера области).
    /// </summary>
    public double MinCellSizeFraction { get; init; } = 0.1;

    /// <summary>
    /// Максимальная доля размера оси для ячеек (например, 0.5 = 50% от размера области).
    /// </summary>
    public double MaxCellSizeFraction { get; init; } = 0.5;

    public double RefineThreshold { get; init; } = 0.3;

    public double MergeThreshold { get; init; } = 0.01;
}
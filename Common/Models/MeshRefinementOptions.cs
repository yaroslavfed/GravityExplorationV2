namespace Common.Models;

/// <summary>
/// Параметры для управления адаптивным уточнением и объединением ячеек сетки.
/// </summary>
public class MeshRefinementOptions
{
    /// <summary>
    /// Максимально допустимый уровень разбиения ячеек (глубина уточнения).
    /// </summary>
    public int MaxSubdivisionLevel { get; init; } = 5;

    /// <summary>
    /// Минимальный допустимый размер ячейки как доля общей длины оси (например, 0.1 = 10% от длины).
    /// </summary>
    public double MinCellSizeFraction { get; init; } = 0.1;

    /// <summary>
    /// Максимальный размер ячейки для объединения, выраженный как доля общей длины оси.
    /// </summary>
    public double MaxCellSizeFraction { get; init; } = 0.5;

    /// <summary>
    /// Порог величины остаточной невязки, выше которого происходит уточнение ячеек.
    /// </summary>
    public double ResidualThresholdRefine { get; init; } = 1e-6;

    /// <summary>
    /// Порог величины остаточной невязки, ниже которого происходит объединение ячеек.
    /// </summary>
    public double ResidualThresholdMerge { get; init; } = 1e-8;
}
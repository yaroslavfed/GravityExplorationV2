namespace Common.Models;

/// <summary>
/// Параметры для управления инверсией методом Гаусса–Ньютона.
/// </summary>
public record InverseOptions
{
    /// <summary>
    /// Искомый функционал
    /// </summary>
    public double FunctionalThreshold { get; init; }

    /// <summary>
    /// Начальный коэффициент регуляризации.
    /// </summary>
    public double Lambda { get; init; }

    /// <summary>
    /// Минимально допустимое значение регуляризации (защита от нуля).
    /// </summary>
    public double MinLambda { get; init; }

    /// <summary>
    /// Коэффициент уменьшения регуляризации на итерацию (например, 0.9 = уменьшение на 10% за итерацию).
    /// </summary>
    public double LambdaDecay { get; init; }

    /// <summary>
    /// Использовать автоматическую подстройку регуляризации.
    /// </summary>
    public bool AutoAdjustRegularization { get; init; }

    /// <summary>
    /// Использовать регуляризацию первого порядка (амплитудную).
    /// </summary>
    public bool UseTikhonovFirstOrder { get; init; }

    /// <summary>
    /// Использовать регуляризацию второго порядка (сглаживающую).
    /// </summary>
    public bool UseTikhonovSecondOrder { get; init; }

    /// <summary>
    /// Порог для автоматического включения сглаживания.
    /// </summary>
    public double GradientThreshold { get; init; }
}
namespace Common.Models;

/// <summary>
/// Параметры для управления инверсией методом Гаусса–Ньютона.
/// </summary>
public record InversionOptions
{
    /// <summary>
    /// Коэффициент регуляризации (сглаживание решения и стабилизация матрицы).
    /// </summary>
    public double Lambda { get; init; } = 1e-6;

    /// <summary>
    /// Использовать регуляризацию первого порядка (подавление амплитуд параметров).
    /// </summary>
    public bool UseTikhonovFirstOrder { get; init; } = true;

    /// <summary>
    /// Использовать регуляризацию второго порядка (сглаживание перепадов между параметрами).
    /// </summary>
    public bool UseTikhonovSecondOrder { get; init; } = false;

    /// <summary>
    /// Автоматически включать регуляризацию второго порядка, если обнаружены резкие перепады между параметрами.
    /// </summary>
    public bool AutoAdjustRegularization { get; init; } = true;

    /// <summary>
    /// Порог средней разности между соседними параметрами для активации сглаживающей регуляризации.
    /// </summary>
    public double GradientThreshold { get; init; } = 5.0;
}
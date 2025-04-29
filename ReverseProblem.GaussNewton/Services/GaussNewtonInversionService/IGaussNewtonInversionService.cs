using Common.Models;

namespace ReverseProblem.GaussNewton.Services.GaussNewtonInversionService;

public interface IGaussNewtonInversionService
{
    /// <summary>
    /// Решает обратную задачу методом Гаусса–Ньютона с регуляризацией первого и второго порядка.
    /// </summary>
    /// <param name="modelValues">Значения аномалий от текущей модели.</param>
    /// <param name="observedValues">Наблюдённые значения.</param>
    /// <param name="jacobianRaw">Матрица Якобиана (m x n).</param>
    /// <param name="initialParameters">Текущие параметры модели (плотности).</param>
    /// <param name="options">Параметры инверсии.</param>
    /// <param name="iterationNumber">Номер текущей итерации.</param>
    /// <param name="effectiveLambda">Фактическое значение регуляризации (λ), применённое на итерации.</param>
    /// <returns>Обновлённый массив параметров модели.</returns>
    double[] Invert(
        double[] modelValues,
        double[] observedValues,
        double[,] jacobian,
        double[] initialParameters,
        InverseOptions options,
        int iterationNumber,
        out double effectiveLambda
    );
}
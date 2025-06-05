using Common.Models;
using MathNet.Numerics.LinearAlgebra;

// ReSharper disable InconsistentNaming

namespace ReverseProblem.GaussNewton.Services.GaussNewtonInversionService;

public class GaussNewtonInversionService : IGaussNewtonInversionService
{
    /// <inheritdoc cref="IGaussNewtonInversionService.Invert"/>
    public double[] Invert(
        double[] modelValues,
        double[] observedValues,
        double[,] jacobianRaw,
        double[] parameters,
        InverseOptions options,
        int iterationNumber,
        bool iterationStagnation
    )
    {
        var n = parameters.Length;

        // Вычисляем вектор невязки r = g_obs - g_calc
        var residual = Vector<double>.Build.DenseOfEnumerable(
            observedValues.Zip(modelValues, (obs, calc) => obs - calc)
        );

        // Преобразуем матрицу Якобиана в Math.NET
        var J = Matrix<double>.Build.DenseOfArray(jacobianRaw);

        // Вычисляем A = J^T * J и b = J^T * r
        var JT = J.Transpose();
        var JTJ = JT * J;
        var JTr = JT * residual;

        // Динамическая регуляризация
        var baseLambda = options.Lambda;
        double effectiveLambda;
        if (options.AutoAdjustRegularization)
        {
            effectiveLambda = iterationStagnation
                ? Math.Min(baseLambda / Math.Pow(options.LambdaDecay, iterationNumber), options.MaxLambda)
                : Math.Max(baseLambda * Math.Pow(options.LambdaDecay, iterationNumber), options.MinLambda);
        }
        else
            effectiveLambda = baseLambda;

        // Регуляризация Тихонова 1-го порядка
        if (options.UseTikhonovFirstOrder)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"Using Tikhonov first order: alfa = {effectiveLambda}");
            for (int i = 0; i < n; i++)
                JTJ[i, i] += effectiveLambda;
            Console.ResetColor();
        }

        // Регуляризация Тихонова 2-го порядка
        if (options.UseTikhonovSecondOrder)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            double gamma = effectiveLambda * options.SecondOrderRegularizationLambdaMultiplier;
            Console.WriteLine($"Using Tikhonov second order: gamma = {gamma}");
            AddTikhonovSecondOrderRegularization(
                JTJ,
                parameters,
                options.GradientThreshold,
                effectiveLambda * options.SecondOrderRegularizationLambdaMultiplier
            );
            Console.ResetColor();
        }

        // Решаем систему нормальных уравнений
        var delta = JTJ.Solve(JTr);

        // Обновляем параметры модели
        return parameters.Zip(delta, (p, d) => p + d).ToArray();
    }

    /// <summary>
    /// Добавляет регуляризацию второго порядка, основанную на второй производной (кривизне).
    /// </summary>
    /// <param name="A">Матрица A = JᵗJ, в которую добавляется регуляризация.</param>
    /// <param name="parameters">Текущие параметры модели.</param>
    /// <param name="threshold">Порог чувствительности к кривизне.</param>
    /// <param name="lambda">Вес регуляризации.</param>
    private void AddTikhonovSecondOrderRegularization(
        Matrix<double> A,
        double[] parameters,
        double threshold,
        double lambda
    )
    {
        int n = parameters.Length;

        for (int i = 1; i < n - 1; i++)
        {
            double curvature = parameters[i - 1] - 2 * parameters[i] + parameters[i + 1];

            if (Math.Abs(curvature) > threshold)
                A[i, i] += lambda;
        }
    }
}
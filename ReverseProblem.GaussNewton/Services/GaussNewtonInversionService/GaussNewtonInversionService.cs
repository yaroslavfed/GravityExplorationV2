using Common.Models;
using MathNet.Numerics.LinearAlgebra;

namespace ReverseProblem.GaussNewton.Services.GaussNewtonInversionService;

public class GaussNewtonInversionService : IGaussNewtonInversionService
{
    /// <inheritdoc cref="IGaussNewtonInversionService.Invert"/>
    public double[] Invert(
        double[] modelValues,
        double[] observedValues,
        double[,] jacobianRaw,
        double[] initialParameters,
        InverseOptions options,
        int iterationNumber,
        out double effectiveLambda
    )
    {
        int m = observedValues.Length;
        int n = initialParameters.Length;

        // 1. Вычисляем вектор невязки r = g_obs - g_calc
        var residual = Vector<double>.Build.DenseOfEnumerable(
            observedValues.Zip(modelValues, (obs, calc) => obs - calc)
        );

        // 1. Преобразуем матрицу Якобиана в Math.NET
        var J = Matrix<double>.Build.DenseOfArray(jacobianRaw);

        // 2. Вычисляем A = J^T * J и b = J^T * r
        var JT = J.Transpose();
        var JTJ = JT * J;
        var JTr = JT * residual;

        // 3. Вычисляем лямбду с учётом динамического затухания
        double baseLambda = options.Lambda;
        effectiveLambda = baseLambda;

        if (options.AutoAdjustRegularization)
        {
            effectiveLambda = baseLambda * Math.Pow(options.LambdaDecay, iterationNumber);
            effectiveLambda = Math.Max(effectiveLambda, options.MinLambda);
        }

        // 4. Добавляем амплитудную регуляризацию (первого порядка)
        if (options.UseTikhonovFirstOrder)
        {
            for (int i = 0; i < n; i++)
                JTJ[i, i] += effectiveLambda;
        }

        // 5. Добавляем сглаживающую регуляризацию (второго порядка)
        if (options.UseTikhonovSecondOrder)
        {
            AddTikhonovSecondOrderRegularization(
                JTJ,
                initialParameters,
                options.GradientThreshold,
                effectiveLambda * options.SecondOrderRegularizationLambdaMultiplier
            );
        }

        // 6. Решаем систему нормальных уравнений
        var delta = JTJ.Solve(JTr);

        // 7. Обновляем параметры модели
        return initialParameters.Zip(delta, (p, d) => p + d).ToArray();
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
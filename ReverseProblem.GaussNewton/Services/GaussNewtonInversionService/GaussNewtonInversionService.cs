using Common.Models;

namespace ReverseProblem.GaussNewton.Services.GaussNewtonInversionService;

public class GaussNewtonInversionService : IGaussNewtonInversionService
{
    public double[] Invert(
        double[] modelValues,
        double[] observedValues,
        double[,] jacobian,
        double[] initialParameters,
        InverseOptions options,
        int iterationNumber,
        out double effectiveLambda
    )
    {
        int m = observedValues.Length;
        int n = initialParameters.Length;

        // 1. Вычисляем вектор невязки r = g_obs - g_calc
        var residuals = observedValues.Zip(modelValues, (obs, calc) => obs - calc).ToArray();

        // 2. Формируем матрицу A = Jᵀ * J и вектор b = Jᵀ * r
        var JTJ = MultiplyTransposed(jacobian, jacobian, n, m);
        var JTr = MultiplyTransposeVector(jacobian, residuals, n, m);

        // 3. Автоматическая настройка регуляризации
        double lambda = options.Lambda;
        if (options.AutoAdjustRegularization)
        {
            lambda *= Math.Pow(options.LambdaDecay, iterationNumber);
            lambda = Math.Max(lambda, options.MinLambda);
        }

        // 4. Добавляем регуляризацию
        if (options.UseTikhonovFirstOrder)
            AddLambdaToDiagonal(JTJ, lambda);

        if (options.UseTikhonovSecondOrder)
            AddTikhonovSecondOrderRegularization(JTJ, initialParameters, options.GradientThreshold, lambda);

        // 5. Решаем систему A Δp = b
        var delta = SolveLinearSystem(JTJ, JTr);

        // 6. Обновляем параметры модели: p_new = p_old + Δp
        var updatedParameters = new double[n];
        for (int i = 0; i < n; i++)
        {
            updatedParameters[i] = initialParameters[i] + delta[i];
        }

        effectiveLambda = lambda;
        return updatedParameters;
    }

    private double[,] MultiplyTransposed(double[,] J, double[,] J2, int n, int m)
    {
        var result = new double[n, n];

        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j <= i; j++)
            {
                double sum = 0;
                for (int k = 0; k < m; k++)
                {
                    sum += J[k, i] * J2[k, j];
                }

                result[i, j] = sum;
                result[j, i] = sum;
            }
        }

        return result;
    }

    private double[] MultiplyTransposeVector(double[,] J, double[] r, int n, int m)
    {
        var result = new double[n];

        for (int i = 0; i < n; i++)
        {
            double sum = 0;
            for (int k = 0; k < m; k++)
            {
                sum += J[k, i] * r[k];
            }

            result[i] = sum;
        }

        return result;
    }

    private void AddLambdaToDiagonal(double[,] A, double lambda)
    {
        int n = A.GetLength(0);
        for (int i = 0; i < n; i++)
        {
            A[i, i] += lambda;
        }
    }

    private void AddTikhonovSecondOrderRegularization(
        double[,] A,
        double[] parameters,
        double gradientThreshold,
        double lambda
    )
    {
        int n = parameters.Length;

        for (int i = 1; i < n - 1; i++)
        {
            double gradient = parameters[i + 1] - 2 * parameters[i] + parameters[i - 1];

            if (Math.Abs(gradient) > gradientThreshold)
            {
                A[i, i] += lambda;
            }
        }
    }

    private double[] SolveLinearSystem(double[,] A, double[] b)
    {
        int n = b.Length;
        var x = new double[n];
        var AClone = (double[,])A.Clone();
        var bClone = (double[])b.Clone();

        // Прямой ход (приведение к верхнетреугольному виду)
        for (int k = 0; k < n; k++)
        {
            for (int i = k + 1; i < n; i++)
            {
                double factor = AClone[i, k] / AClone[k, k];
                for (int j = k; j < n; j++)
                {
                    AClone[i, j] -= factor * AClone[k, j];
                }

                bClone[i] -= factor * bClone[k];
            }
        }

        // Обратный ход
        for (int i = n - 1; i >= 0; i--)
        {
            double sum = 0;
            for (int j = i + 1; j < n; j++)
            {
                sum += AClone[i, j] * x[j];
            }

            x[i] = (bClone[i] - sum) / AClone[i, i];
        }

        return x;
    }
}
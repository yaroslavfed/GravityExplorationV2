using ReverseProblem.GaussNewton.Models;

namespace ReverseProblem.GaussNewton.Services.GaussNewtonInversionService;

public class GaussNewtonInversionService : IGaussNewtonInversionService
{
    public double[] Invert(
        double[] modelValues,
        double[] observedValues,
        double[,] jacobian,
        double[] initialParameters,
        GaussNewtonInversionOptions options
    )
    {
        int m = observedValues.Length;
        int n = initialParameters.Length;

        if (modelValues.Length != m)
            throw new ArgumentException("modelValues and observedValues must have the same length.");
        if (jacobian.GetLength(0) != m || jacobian.GetLength(1) != n)
            throw new ArgumentException("jacobian size must match (m x n).");

        // 1. Вычисляем остаточную невязку
        double[] residual = new double[m];
        for (int i = 0; i < m; i++)
            residual[i] = observedValues[i] - modelValues[i];

        // 2. Составляем A = Jᵗ J
        double[,] A = new double[n, n];
        for (int q = 0; q < n; q++)
            for (int s = 0; s < n; s++)
            {
                double sum = 0;
                for (int i = 0; i < m; i++)
                    sum += jacobian[i, q] * jacobian[i, s];
                A[q, s] = sum;
            }

        // 3. Составляем b = Jᵗ r
        double[] b = new double[n];
        for (int q = 0; q < n; q++)
        {
            double sum = 0;
            for (int i = 0; i < m; i++)
                sum += jacobian[i, q] * residual[i];
            b[q] = sum;
        }

        // 4. Регуляризация
        if (options.UseTikhonovFirstOrder)
        {
            for (int i = 0; i < n; i++)
                A[i, i] += options.Lambda;
        }

        if (options.UseTikhonovSecondOrder)
        {
            // Регуляризация второго порядка (сглаживание — дискретный Лапласиан)
            for (int i = 0; i < n; i++)
            {
                if (i > 0) A[i, i] += options.Lambda;
                if (i < n - 1) A[i, i] += options.Lambda;
                if (i > 0) A[i, i - 1] -= options.Lambda;
                if (i < n - 1) A[i, i + 1] -= options.Lambda;
            }
        }

        // 5. Решаем систему A Δp = b
        double[] delta = SolveLinearSystem(A, b);

        // 6. Обновляем параметры
        double[] updatedParameters = new double[n];
        for (int i = 0; i < n; i++)
            updatedParameters[i] = initialParameters[i] + delta[i];

        return updatedParameters;
    }

    private static double[] SolveLinearSystem(double[,] A, double[] b)
    {
        int n = b.Length;
        var x = new double[n];
        var A_ = (double[,])A.Clone();
        var b_ = (double[])b.Clone();

        // Прямой ход
        for (int i = 0; i < n; i++)
        {
            int maxRow = i;
            for (int k = i + 1; k < n; k++)
                if (Math.Abs(A_[k, i]) > Math.Abs(A_[maxRow, i]))
                    maxRow = k;

            for (int k = i; k < n; k++)
                (A_[i, k], A_[maxRow, k]) = (A_[maxRow, k], A_[i, k]);
            (b_[i], b_[maxRow]) = (b_[maxRow], b_[i]);

            for (int k = i + 1; k < n; k++)
            {
                double factor = A_[k, i] / A_[i, i];
                b_[k] -= factor * b_[i];
                for (int j = i; j < n; j++)
                    A_[k, j] -= factor * A_[i, j];
            }
        }

        // Обратный ход
        for (int i = n - 1; i >= 0; i--)
        {
            x[i] = b_[i];
            for (int j = i + 1; j < n; j++)
                x[i] -= A_[i, j] * x[j];
            x[i] /= A_[i, i];
        }

        return x;
    }
}
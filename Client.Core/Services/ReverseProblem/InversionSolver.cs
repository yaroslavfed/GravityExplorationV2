using Client.Core.Services.MeshService;
using Common.Constants;
using Common.Data;
using DirectTask.Core.Services;

namespace Client.Core.Services.ReverseProblem;

/// ОСНОВНОЙ РЕШАТЕЛЬ
public class InversionSolver : IInversionSolver
{
    private readonly IAnomalyService _anomalyService;
    private readonly IMeshService    _meshService;

    public InversionSolver(IAnomalyService anomalyService, IMeshService meshService)
    {
        _anomalyService = anomalyService;
        _meshService = meshService;
    }

    public async Task<double[]> Invert(Mesh mesh, List<Sensor> sensors, double lambda)
    {
        int m = sensors.Count;
        int n = mesh.Cells.Count;

        double[] gCalc = await CalculateForward(mesh, sensors);
        double[] residual = sensors.Select((s, i) => s.Value - gCalc[i]).ToArray();

        double misfit = residual.Select(r => r * r).Sum();
        if (misfit <= 1e-16)
            return [];

        Console.WriteLine($"  The functional of the discrepancy: {misfit:E5}");

        double[,] J = BuildJacobian(mesh, sensors);
        double[,] JTJ = MultiplyTransposed(J, J, n, m);
        AddLambdaToDiagonal(JTJ, lambda);

        double[] JTresidual = MultiplyTransposeVector(J, residual, n, m);
        double[] delta = SolveLinearSystem(JTJ, JTresidual);

        for (int j = 0; j < n; j++)
            mesh.Cells[j].Density += delta[j];

        return residual;
    }

    // === Заглушки ===
    public async Task<double[]> CalculateForward(Mesh mesh, List<Sensor> sensors)
    {
        var baseDensity = await _meshService.GetBaseDensityAsync();
        var anomalyMap = _anomalyService.GetAnomalyMapAsync(mesh, sensors, baseDensity);

        double[] anomalies = new double[sensors.Count];
        int index = 0;

        await foreach (var anomaly in anomalyMap)
            anomalies[index++] = anomaly.Value;

        return anomalies;
    }

    public double[,] BuildJacobian(Mesh mesh, List<Sensor> sensors)
    {
        int m = sensors.Count;
        int n = mesh.Cells.Count;
        double[,] J = new double[m, n];

        for (int i = 0; i < m; i++)
            for (int j = 0; j < n; j++)
            {
                // ∂g_i/∂ρ_j = влияние j-ой ячейки на i-й сенсор
                J[i, j] = ComputePartialDerivative(sensors[i], mesh.Cells[j]);
            }

        return J;
    }

    public double ComputePartialDerivative(Sensor s, Cell c)
    {
        // Используем реализацию прямой задачи для одной ячейки при единичной плотности
        double x0 = c.CenterX - c.BoundX;
        double x1 = c.CenterX + c.BoundX;
        double y0 = c.CenterY - c.BoundY;
        double y1 = c.CenterY + c.BoundY;
        double z0 = c.CenterZ - c.BoundZ;
        double z1 = c.CenterZ + c.BoundZ;

        return PhysicalQuantities.GravitationalConstant
               * _anomalyService.IntegralCalculation(s.X, s.Y, s.Z, x0, x1, y0, y1, z0, z1);
    }

    // === Линейная алгебра ===
    public double[,] MultiplyTransposed(double[,] A, double[,] B, int n, int m)
    {
        double[,] result = new double[n, n];
        for (int i = 0; i < n; i++)
            for (int j = 0; j < n; j++)
            {
                double sum = 0;
                for (int k = 0; k < m; k++)
                    sum += A[k, i] * B[k, j];
                result[i, j] = sum;
            }

        return result;
    }

    public void AddLambdaToDiagonal(double[,] A, double lambda)
    {
        int n = A.GetLength(0);
        for (int i = 0; i < n; i++)
            A[i, i] += lambda;
    }

    public double[] MultiplyTransposeVector(double[,] J, double[] v, int n, int m)
    {
        double[] result = new double[n];
        for (int i = 0; i < n; i++)
        {
            double sum = 0;
            for (int k = 0; k < m; k++)
                sum += J[k, i] * v[k];
            result[i] = sum;
        }

        return result;
    }

    public double[] SolveLinearSystem(double[,] A, double[] b)
    {
        int n = b.Length;
        var x = new double[n];

        // Простой метод Гаусса (можно заменить на библиотеку)
        var A_ = (double[,])A.Clone();
        var b_ = (double[])b.Clone();

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
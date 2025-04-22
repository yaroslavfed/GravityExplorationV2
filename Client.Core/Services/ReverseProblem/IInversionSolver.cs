using Common.Data;

namespace Client.Core.Services.ReverseProblem;

public interface IInversionSolver
{
    public Task Invert(Mesh mesh, List<Sensor> sensors, int maxIterations, double lambda, double tolerance = 1e-6);
}
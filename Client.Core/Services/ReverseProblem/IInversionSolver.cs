using Common.Data;

namespace Client.Core.Services.ReverseProblem;

public interface IInversionSolver
{
    public Task<double[]> Invert(Mesh mesh, List<Sensor> sensors, double lambda);
}
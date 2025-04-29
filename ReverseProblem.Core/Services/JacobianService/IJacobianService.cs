using Common.Data;

namespace ReverseProblem.Core.Services.JacobianService;

public interface IJacobianService
{
    public double[,] BuildJacobian(Mesh mesh, List<Sensor> sensors);
}
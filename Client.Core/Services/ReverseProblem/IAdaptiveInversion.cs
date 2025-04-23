using Common.Data;

namespace Client.Core.Services.ReverseProblem;

public interface IAdaptiveInversion
{
    Task AdaptiveInvert(
        Mesh initialMesh,
        List<Sensor> sensors,
        int totalIterations,
        double lambda
    );
}
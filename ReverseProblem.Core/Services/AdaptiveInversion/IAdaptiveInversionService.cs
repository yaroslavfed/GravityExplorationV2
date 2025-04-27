using Common.Data;
using ReverseProblem.GaussNewton.Models;

namespace ReverseProblem.Core.Services.AdaptiveInversion;

public interface IAdaptiveInversionService
{
    Task AdaptiveInvertAsync(
        Mesh initialMesh,
        List<Sensor> sensors,
        int totalIterations,
        GaussNewtonInversionOptions inversionOptions,
        double baseDensity
    );
}
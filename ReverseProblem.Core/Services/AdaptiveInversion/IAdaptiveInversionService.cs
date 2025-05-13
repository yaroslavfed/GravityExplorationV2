using Common.Data;
using Common.Models;

namespace ReverseProblem.Core.Services.AdaptiveInversion;

public interface IAdaptiveInversionService
{
    Task AdaptiveInvertAsync(
        Mesh initialMesh,
        List<Sensor> sensors,
        SensorsGrid sensorsGrid,
        InverseOptions inversionOptions,
        MeshRefinementOptions refinementOptions,
        double baseDensity
    );
}
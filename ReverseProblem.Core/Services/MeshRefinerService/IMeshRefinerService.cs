using Common.Data;
using Common.Models;

namespace ReverseProblem.Core.Services.MeshRefinerService;

public interface IMeshRefinerService
{
    List<Cell> RefineOrMergeCellsAdvanced(
        Mesh mesh,
        List<Sensor> sensors,
        double[] residuals,
        double thresholdRefine,
        double thresholdMerge,
        double maxResidual,
        MeshRefinementOptions refinementOptions,
        SensorsGrid sensorGrid
    );
}
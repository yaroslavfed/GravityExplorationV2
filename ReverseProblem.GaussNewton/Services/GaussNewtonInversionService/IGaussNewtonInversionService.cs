using Common.Models;

namespace ReverseProblem.GaussNewton.Services.GaussNewtonInversionService;

public interface IGaussNewtonInversionService
{
    double[] Invert(
        double[] modelValues,
        double[] observedValues,
        double[,] jacobian,
        double[] initialParameters,
        InverseOptions options,
        int iterationNumber = 0
    );
}
using ReverseProblem.GaussNewton.Models;

namespace ReverseProblem.GaussNewton.Services.GaussNewtonInversionService;

public interface IGaussNewtonInversionService
{
    double[] Invert(
        double[] modelValues,
        double[] observedValues,
        double[,] jacobian,
        double[] initialParameters,
        GaussNewtonInversionOptions options
    );
}
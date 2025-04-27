namespace ReverseProblem.GaussNewton.Models;

public record GaussNewtonInversionOptions
{
    public double Lambda { get; init; } = 1e-6;

    public bool UseTikhonovFirstOrder { get; init; } = true;

    public bool UseTikhonovSecondOrder { get; init; } = false;
}
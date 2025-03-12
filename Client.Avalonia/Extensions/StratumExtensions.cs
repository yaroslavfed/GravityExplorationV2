using Client.Avalonia.Models;

namespace Client.Avalonia.Extensions;

public static class StratumExtensions
{
    public static double GetWidth(this Stratum stratum) => stratum.StepX * 2;

    public static double GetDepth(this Stratum stratum) => stratum.StepY * 2;

    public static double GetHeight(this Stratum stratum) => stratum.StepZ * 2;
}
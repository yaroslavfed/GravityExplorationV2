using Client.Core.Data.Entities;

namespace Client.Core.Extensions;

public static class StratumExtensions
{
    public static double GetWidth(this Stratum stratum) => stratum.Dimensions.Bounds.X * 2;

    public static double GetDepth(this Stratum stratum) => stratum.Dimensions.Bounds.Y * 2;

    public static double GetHeight(this Stratum stratum) => stratum.Dimensions.Bounds.Z * 2;
}
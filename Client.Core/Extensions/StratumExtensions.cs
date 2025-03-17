using Client.Core.Data;

namespace Client.Core.Extensions;

public static class StratumExtensions
{
    public static double GetWidth(this Stratum stratum) => stratum.EndX - stratum.StartX;

    public static double GetDepth(this Stratum stratum) => stratum.EndY - stratum.StartY;

    public static double GetHeight(this Stratum stratum) => stratum.EndZ - stratum.StartZ;
}
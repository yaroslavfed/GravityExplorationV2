using Client.Core.Data;
using SkiaSharp;

namespace Client.Avalonia.Extensions;

public static class SkExtensions
{
    /// <summary>
    ///  Метод для получения Skia-прямоугольника
    /// </summary>
    /// <returns></returns>
    public static SKRect ToSkRect(this Stratum stratumClientDto)
    {
        return new SKRect(
            (float)(stratumClientDto.CenterX - stratumClientDto.StepX),
            (float)(stratumClientDto.CenterY - stratumClientDto.StepY),
            (float)(stratumClientDto.CenterX + stratumClientDto.StepX),
            (float)(stratumClientDto.CenterY + stratumClientDto.StepY)
        );
    }
}
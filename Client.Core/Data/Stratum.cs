using Client.Core.Extensions;

namespace Client.Core.Data;

public record Stratum
{
    public required Guid Id { get; init; }

    public double StartX { get; init; }

    public double EndX { get; init; }

    public double StartY { get; init; }

    public double EndY { get; init; }

    public double StartZ { get; init; }

    public double EndZ { get; init; }

    public double Density { get; init; } = 0.0;

    public bool IsActive { get; set; } = false;

    public Placement Placement =>
        new()
        {
            Position = new()
            {
                X = (EndX - StartX) / 2 + StartX,
                Y = (EndY - StartY) / 2 + StartY,
                Z = (EndZ - StartZ) / 2 + StartZ
            },
            StepToBound = new() { X = (EndX - StartX) / 2, Y = (EndY - StartY) / 2, Z = (EndZ - StartZ) / 2 }
        };

    public string DimensionLabel => $"[{this.GetWidth():0.##} x {this.GetDepth():0.##} x {this.GetHeight():0.##}]";

    public string PlacementLabel => $"OX: {StartX} -> {EndX}\nOY: {StartY} -> {EndY}\nOZ: {StartZ} -> {EndZ}";

    public string DensityLabel => $"{Density:0.##} г/см\u00B3";
}
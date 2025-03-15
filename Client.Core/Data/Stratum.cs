using Client.Core.Extensions;

namespace Client.Core.Data;

public record Stratum
{
    public required Guid Id { get; init; }

    public double CenterX { get; init; } = .0;

    public double StepX { get; init; } = 1.0;

    public double CenterY { get; init; } = .0;

    public double StepY { get; init; } = 1.0;

    public double CenterZ { get; init; } = .0;

    public double StepZ { get; init; } = 1.0;

    public double Density { get; init; } = 0.0;

    public bool IsActive { get; set; } = false;
    
    public Placement Placement =>
        new()
        {
            BoundsX = (CenterX - StepX, CenterX + StepX),
            BoundsY = (CenterY - StepY, CenterY + StepY),
            BoundsZ = (CenterZ - StepZ, CenterZ + StepZ)
        };

    public string DimensionLabel => $"[{this.GetWidth():0.##} x {this.GetDepth():0.##} x {this.GetHeight():0.##}]";

    public string PlacementLabel =>
        $"OX: {Placement.BoundsX.Min} -> {Placement.BoundsX.Max}\nOY: {Placement.BoundsY.Min} -> {Placement.BoundsY.Max}\nOZ: {Placement.BoundsZ.Min} -> {Placement.BoundsZ.Max}";

    public string DensityLabel => $"{Density:0.##} г/см\u00B3";
}
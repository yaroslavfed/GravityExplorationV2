using System;
using Client.Avalonia.Extensions;
using Client.Core.Data;
using ReactiveUI.Fody.Helpers;

namespace Client.Avalonia.Models;

public record Stratum
{
    public required Guid Id { get; init; }

    public double CenterX { get; init; } = 0.0;

    public double StepX { get; init; } = 1.0;

    public double CenterY { get; init; } = 0.0;

    public double StepY { get; init; } = 1.0;

    public double CenterZ { get; init; } = 0.0;

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
    
    public string DimensionLabel => $"{this.GetWidth():0.##} x {this.GetDepth():0.##} x {this.GetHeight():0.##}";
    
    public string PlacementLabel => $"{Placement.BoundsX}, {Placement.BoundsY}, {Placement.BoundsZ}";
    
    public string DensityLabel => $"{Density:0.##} г/см\u00B3";
}
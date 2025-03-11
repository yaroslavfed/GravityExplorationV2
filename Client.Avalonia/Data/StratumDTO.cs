using Client.Core.Extensions;

namespace Client.Avalonia.Data;

public record StratumDto : Core.Data.Entities.Stratum
{
    public bool IsActive { get; set; } = false;

    public string DimensionLabel => $"{this.GetWidth():0.##} x {this.GetDepth():0.##} x {this.GetHeight():0.##}";

    public string PlacementLabel => $"{Placement.BoundsX}, {Placement.BoundsY}, {Placement.BoundsZ}";

    public string DensityLabel => $"{Density:0.##} г/см\u00B3";
}
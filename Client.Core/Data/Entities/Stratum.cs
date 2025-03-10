using Client.Core.Extensions;

namespace Client.Core.Data.Entities;

public record Stratum
{
    public required Guid Id { get; init; }

    public required Dimensions Dimensions { get; init; }

    public Placement Placement =>
        new()
        {
            BoundsX = (Dimensions.Center.X - Dimensions.Bounds.X, Dimensions.Center.X + Dimensions.Bounds.X),
            BoundsY = (Dimensions.Center.Y - Dimensions.Bounds.Y, Dimensions.Center.Y + Dimensions.Bounds.Y),
            BoundsZ = (Dimensions.Center.Z - Dimensions.Bounds.Z, Dimensions.Center.Z + Dimensions.Bounds.Z)
        };

    public double Density { get; init; } = .0;

    public string DimensionLabel => $"{this.GetWidth():0.##} x {this.GetDepth():0.##} x {this.GetHeight():0.##}";

    public string PlacementLabel => $"{Placement.BoundsX}, {Placement.BoundsY}, {Placement.BoundsZ}";

    public string DensityLabel => $"{Density:0.##} г/см\u00B3";

    public virtual bool Equals(Stratum? obj)
    {
        if (obj is not { }) return false;

        return Id == obj.Id && Math.Abs(Density - obj.Density) < 0e-16; // Сравниваем по значимым полям
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Id, Density);
    }
}
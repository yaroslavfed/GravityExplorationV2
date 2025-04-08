using Client.Core.Data;
using Common.Data;

namespace Client.Core.Services.PlotService;

public record MeshParams
{
    public required Mesh Mesh { get; init; }

    public required SensorsGrid SensorsGrid { get; init; }

    public required bool SelectedProjection { get; init; }

    public required string Projection { get; init; }
}
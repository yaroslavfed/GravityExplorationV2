using Client.Core.Enums;

namespace Client.Core.Data;

public record PlotProjection(
    string Name,
    EProjection Projection,
    bool IsActivated
);
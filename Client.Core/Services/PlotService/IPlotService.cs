using Client.Core.Data;
using Client.Core.Enums;

namespace Client.Core.Services.PlotService;

public interface IPlotService
{
    // Domain
    // Stratums
    // SensorsGrid
    // IsSensorsGridTurnedOn
    // SelectedProjection
    Task<string> GenerateChartAsync(
        Domain domain,
        IReadOnlyList<Stratum> strata,
        SensorsGrid sensorsGrid,
        bool selectedProjection,
        EProjection projection
    );
}
using Client.Core.Data;
using Client.Core.Enums;
using Common.Data;

namespace Client.Core.Services.PlotService;

public interface IMeshPlotHelper
{
    Task<string> GenerateChartAsync(
        SensorsGrid sensorsGrid,
        bool selectedProjection,
        EProjection projection
    );
}
using Common.Data;

namespace Client.Core.Services.AnomalyPlotHelper;

public interface IAnomalyPlotHelper
{
    Task<string> GenerateChartAsync(List<Sensor> sensors);
}
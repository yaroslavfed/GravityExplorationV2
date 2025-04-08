using Client.Core.Services.PlotHelper;
using Common.Data;

namespace Client.Core.Services.AnomalyPlotHelper;

internal class AnomalyPlotHelper : PlotHelperBase<List<Sensor>>, IAnomalyPlotHelper
{
    public AnomalyPlotHelper()
    {
        _jsonFile = "anomaly_data.json";
        _pythonPath = "python";
        _outputImage = "anomaly_chart.png";
        _scriptName = "anomaly_chart.py";
    }

    public async Task<string> GenerateChartAsync(List<Sensor> sensors)
    {
        return await GeneratePlotAsync(sensors);
    }
}
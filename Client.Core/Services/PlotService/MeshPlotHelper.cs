using Client.Core.Data;
using Client.Core.Enums;
using Client.Core.Services.MeshService;
using Client.Core.Services.PlotHelper;
using Common.Data;

namespace Client.Core.Services.PlotService;

internal class MeshPlotHelper : PlotHelperBase<Mesh>, IMeshPlotHelper
{
    private readonly IMeshService _meshService;

    public MeshPlotHelper(IMeshService meshService)
    {
        _meshService = meshService;

        _jsonFile = "mesh_data.json";
        _pythonPath = "python";
        _outputImage = "mesh_chart.png";
        _scriptName = "testing_chart.py";
    }

    public async Task<string> GenerateChartAsync(
        SensorsGrid sensorsGrid,
        bool selectedProjection,
        EProjection projection
    )
    {
        var mesh = await _meshService.GetMeshAsync();
        var data = new MeshParams
        {
            Mesh = mesh,
            SensorsGrid = sensorsGrid,
            SelectedProjection = selectedProjection,
            Projection = projection switch
            {
                EProjection.XY => "XY",
                EProjection.XZ => "XZ",
                EProjection.YZ => "YZ",
                _              => throw new ArgumentOutOfRangeException(nameof(projection), "Неизвестная проекция")
            }
        };

        return await GeneratePlotAsync(mesh);
    }
}
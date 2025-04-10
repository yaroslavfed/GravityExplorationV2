using Client.Core.Services.MeshService;
using Client.Core.Services.SensorsService;
using Common.Data;
using DirectTask.Core.Services;

namespace Client.Core.Services.ForwardTaskService;

internal class ForwardTaskService : IForwardTaskService
{
    private readonly IAnomalyService _anomalyService;
    private readonly IMeshService    _meshService;
    private readonly ISensorsService _sensorsService;

    public ForwardTaskService(IAnomalyService anomalyService, IMeshService meshService, ISensorsService sensorsService)
    {
        _anomalyService = anomalyService;
        _meshService = meshService;
        _sensorsService = sensorsService;
    }

    public Task<IAsyncEnumerable<Sensor>> GetAnomalyMapAsync()
    {
        return Core();

        async Task<IAsyncEnumerable<Sensor>> Core()
        {
            var mesh = await _meshService.GetMeshAsync();
            var baseDensity = await _meshService.GetBaseDensityAsync();
            var sensors = await _sensorsService.GetSensorsAsync();

            return _anomalyService.GetAnomalyMapAsync(mesh, sensors, baseDensity);
        }
    }
}
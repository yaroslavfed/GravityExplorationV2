using Client.Core.Extensions;
using Client.Core.Services.ComputationalDomainService;
using Common.Data;

namespace Client.Core.Services.SensorsService;

public class SensorsService : ISensorsService
{
    private readonly IComputationalDomainService _computationalDomainService;
    private          List<Sensor>                _sensorsList = [];

    public SensorsService(IComputationalDomainService computationalDomainService)
    {
        _computationalDomainService = computationalDomainService;
    }

    public Task<Sensor> GetAsync(Guid id) => throw new NotImplementedException();

    public async Task<IReadOnlyList<Sensor>> GetAllAsync()
    {
        if (_sensorsList.Count == 0)
            await CalculateSensorsAsync();

        return _sensorsList;
    }

    private async Task CalculateSensorsAsync()
    {
        var domain = await _computationalDomainService.Domain.Value();

        var sensors = new List<Sensor>();

        var stepX = (domain.EndX - domain.StartX) / domain.SplitsXCount;
        var stepY = (domain.EndY - domain.StartY) / domain.SplitsYCount;

        for (var i = 0; i <= domain.SplitsXCount; i++)
        {
            var x = domain.StartX + i * stepX;
            for (var j = 0; j <= domain.SplitsYCount; j++)
            {
                var y = domain.StartY + j * stepY;
                sensors.Add(new() { X = x, Y = y, Z = domain.StartZ });
            }
        }

        _sensorsList = sensors;
    }
}
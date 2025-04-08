using System.Reactive.Linq;
using System.Reactive.Subjects;
using Client.Core.Data;
using Common.Data;

namespace Client.Core.Storages.SensorsStorage;

internal class SensorsStorage : ISensorsStorage
{
    private readonly BehaviorSubject<SensorsGrid> _sensors;

    public SensorsStorage()
    {
        _sensors = new(
            new()
            {
                StartX = 0,
                EndX = 1,
                SplitsXCount = 1,
                StartY = 0,
                EndY = 1,
                SplitsYCount = 1
            }
        );
    }

    public IObservable<SensorsGrid> SensorsList => _sensors.AsObservable();

    public async Task<IReadOnlyList<Sensor>> GetSensorsAsync()
    {
        var pointsX = await GetPointsAsync(_sensors.Value.StartX, _sensors.Value.EndX, _sensors.Value.SplitsXCount);
        var pointsY = await GetPointsAsync(_sensors.Value.StartY, _sensors.Value.EndY, _sensors.Value.SplitsYCount);

        var sensors = (from y in pointsY from x in pointsX select new Sensor { X = x, Y = y, Value = 0 }).ToList();
        return sensors;
    }

    public Task UpdateAsync(SensorsGrid data)
    {
        _sensors.OnNext(data);
        return Task.CompletedTask;
    }

    private Task<List<double>> GetPointsAsync(double start, double end, double divisions)
    {
        var points = new List<double>();

        if (divisions <= 0)
            throw new ArgumentException("Количество разбиений должно быть больше 0.");

        var step = (end - start) / divisions;

        for (var i = 0; i <= divisions; i++)
            points.Add(start + i * step);

        return Task.FromResult(points);
    }
}
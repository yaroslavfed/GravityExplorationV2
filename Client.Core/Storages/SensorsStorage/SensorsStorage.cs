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
                EndX = 0,
                SplitsXCount = 1,
                StartY = 0,
                EndY = 0,
                SplitsYCount = 1
            }
        );
    }

    public IObservable<SensorsGrid> SensorsList => _sensors.AsObservable();

    public async Task<IReadOnlyList<Sensor>> GetSensorsAsync()
    {
        var pointsX = await GetPointsAsync(
            _sensors.Value.StartX,
            _sensors.Value.EndX,
            (int)_sensors.Value.SplitsXCount
        );
        var pointsY = await GetPointsAsync(
            _sensors.Value.StartY,
            _sensors.Value.EndY,
            (int)_sensors.Value.SplitsYCount
        );

        var sensors = (from y in pointsY from x in pointsX select new Sensor { X = x, Y = y, Value = 0 }).ToList();
        return sensors;
    }

    public Task UpdateAsync(SensorsGrid data)
    {
        _sensors.OnNext(data);
        return Task.CompletedTask;
    }

    private Task<List<double>> GetPointsAsync(double start, double end, int pointsCount)
    {
        if (pointsCount < 1)
            throw new ArgumentException("Количество точек должно быть не менее 1");

        var points = new List<double>(pointsCount);

        if (pointsCount == 1 && Math.Abs(start - end) < 1e-16)
        {
            points.Add(start);
            return Task.FromResult(points);
        }

        var step = (end - start) / (pointsCount - 1);

        for (var i = 0; i < pointsCount; i++)
            points.Add(start + i * step);

        return Task.FromResult(points);
    }
}
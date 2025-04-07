using System.Reactive.Linq;
using System.Reactive.Subjects;
using Common.Data;

namespace Client.Core.Storages.SensorsStorage;

internal class SensorsStorage : ISensorsStorage
{
    private readonly BehaviorSubject<List<Sensor>> _data = new([]);

    public IObservable<IReadOnlyList<Sensor>> SensorsList => _data.AsObservable();

    public Task UpdateAsync(IReadOnlyList<Sensor> data)
    {
        throw new NotImplementedException();
    }
}
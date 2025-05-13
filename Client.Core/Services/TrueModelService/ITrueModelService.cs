using Common.Data;

namespace Client.Core.Services.TrueModelService;

public interface ITrueModelService
{
    Task SaveSolutionAsync(IEnumerable<Sensor>? solution);

    Task<List<Sensor>?> GetSolutionAsync();
}
using Common.Data;

namespace Client.Core.Services.TrueModelService;

public interface ITrueModelService
{
    Task SaveTaskSolutionAsync(IEnumerable<Sensor>? taskSolution);

    Task<List<Sensor>?> GetTaskSolutionAsync();
}
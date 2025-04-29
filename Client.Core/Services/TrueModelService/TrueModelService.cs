using Common.Data;

namespace Client.Core.Services.TrueModelService;

public class TrueModelService : ITrueModelService
{
    private List<Sensor>? _taskSolution;

    public Task SaveSolutionAsync(IEnumerable<Sensor>? solution)
    {
        _taskSolution = solution?.ToList();
        return Task.CompletedTask;
    }

    public Task<List<Sensor>?> GetSolutionAsync()
    {
        return Task.FromResult(_taskSolution);
    }
}
using Common.Data;

namespace Client.Core.Services.TrueModelService;

public class TrueModelService : ITrueModelService
{
    private List<Sensor>? _taskSolution;
    
    public Task SaveTaskSolutionAsync(IEnumerable<Sensor>? taskSolution)
    {
        _taskSolution = taskSolution?.ToList();
        return Task.CompletedTask;
    }

    public Task<List<Sensor>?> GetTaskSolutionAsync()
    {
        return Task.FromResult(_taskSolution);
    }
}
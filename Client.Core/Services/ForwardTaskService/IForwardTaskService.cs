using Common.Data;

namespace Client.Core.Services.ForwardTaskService;

public interface IForwardTaskService
{
    Task<IAsyncEnumerable<Sensor>> GetAnomalyMapAsync();
}
using Common.Data;

namespace Client.Core.Services.ForwardTaskService;

public interface IForwardTaskService
{
    IAsyncEnumerable<Sensor> GetAnomalyMapAsync();
}
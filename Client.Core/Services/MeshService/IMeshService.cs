using Common.Data;

namespace Client.Core.Services.MeshService;

public interface IMeshService
{
    Task<double> GetBaseDensityAsync();

    Task<Mesh> GetMeshAsync();
}
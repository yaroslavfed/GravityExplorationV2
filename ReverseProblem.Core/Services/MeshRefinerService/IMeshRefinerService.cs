using Common.Data;

namespace ReverseProblem.Core.Services.MeshRefinerService;

public interface IMeshRefinerService
{
    List<Cell> RefineOrMergeCellsAdvanced(Mesh mesh);
}
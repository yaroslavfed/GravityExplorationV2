using System.Diagnostics;
using System.Text.Json;
using Client.Core.Extensions;
using Client.Core.Services.ComputationalDomainService;
using Client.Core.Services.MeshService.OctreeStructure;
using Client.Core.Services.StratumService;
using Common.Data;

namespace Client.Core.Services.MeshService;

public class MeshService : IMeshService
{
    private readonly IComputationalDomainService _computationalDomainService;
    private readonly IStratumService             _stratumService;

    public MeshService(IStratumService stratumService, IComputationalDomainService computationalDomainService)
    {
        _stratumService = stratumService;
        _computationalDomainService = computationalDomainService;
    }

    public async Task<Mesh> GetMeshAsync()
    {
        var stratums = (await _stratumService.StratumsList.Value()).ToList();
        var domain = await _computationalDomainService.Domain.Value();

        var pointsX = GetMergedPoints(
            stratums.Select(s => s.StartX),
            stratums.Select(s => s.EndX),
            domain.StartX,
            domain.EndX,
            domain.SplitsXCount
        );
        var pointsY = GetMergedPoints(
            stratums.Select(s => s.StartY),
            stratums.Select(s => s.EndY),
            domain.StartY,
            domain.EndY,
            domain.SplitsYCount
        );
        var pointsZ = GetMergedPoints(
            stratums.Select(s => s.StartZ),
            stratums.Select(s => s.EndZ),
            domain.StartZ,
            domain.EndZ,
            domain.SplitsZCount
        );

        var domainBounds = new BoundingBox(
            domain.StartX,
            domain.EndX,
            domain.StartY,
            domain.EndY,
            domain.StartZ,
            domain.EndZ
        );
        var octree = new Octree(domainBounds);

        foreach (var stratum in stratums)
            octree.Insert(stratum);

        var cells = new List<Cell>();

        for (int i = 0; i < pointsX.Count - 1; i++)
        for (int j = 0; j < pointsY.Count - 1; j++)
        for (int k = 0; k < pointsZ.Count - 1; k++)
        {
            var minX = pointsX[i];
            var maxX = pointsX[i + 1];
            var minY = pointsY[j];
            var maxY = pointsY[j + 1];
            var minZ = pointsZ[k];
            var maxZ = pointsZ[k + 1];

            var centerX = (minX + maxX) / 2;
            var centerY = (minY + maxY) / 2;
            var centerZ = (minZ + maxZ) / 2;

            var density = octree.FindDensity(centerX, centerY, centerZ) ?? domain.DensityBase;

            cells.Add(
                new Cell
                {
                    CenterX = centerX,
                    CenterY = centerY,
                    CenterZ = centerZ,
                    BoundX = (maxX - minX) / 2,
                    BoundY = (maxY - minY) / 2,
                    BoundZ = (maxZ - minZ) / 2,
                    Density = density
                }
            );
        }

        var mesh = new Mesh { Cells = cells };

#if false
        const string JSON_FILE = "mesh.json";
        const string PYTHON_PATH = "python";
        const string OUTPUT_IMAGE = "chart.png";

        await File.WriteAllTextAsync(JSON_FILE, JsonSerializer.Serialize(mesh));
        var currentDirectory = Directory.GetCurrentDirectory();
        var scriptPath = Path.Combine(currentDirectory, "Scripts\\mesh.py");

        var psi = new ProcessStartInfo
        {
            FileName = PYTHON_PATH,
            Arguments = $"{scriptPath} {JSON_FILE} {OUTPUT_IMAGE}",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        var process = Process.Start(psi);
        var output = await process?.StandardOutput.ReadToEndAsync()!;
        var error = await process?.StandardError.ReadToEndAsync()!;
        await process?.WaitForExitAsync()!;
#endif

        return mesh;
    }

    private static List<double> GetMergedPoints(
        IEnumerable<double> stratumStarts,
        IEnumerable<double> stratumEnds,
        double domainStart,
        double domainEnd,
        double divisions
    )
    {
        var stratumPoints = stratumStarts.Concat(stratumEnds);
        var domainPoints = GetPointsByDomain(domainStart, domainEnd, divisions);
        return stratumPoints.Concat(domainPoints).Distinct().OrderBy(p => p).ToList();
    }

    private static List<double> GetPointsByDomain(double start, double end, double divisions)
    {
        if (divisions <= 0)
            throw new ArgumentException("Количество разбиений должно быть больше 0.");

        var step = (end - start) / divisions;
        var points = new List<double>();
        for (var i = 0; i <= divisions; i++)
            points.Add(start + i * step);
        return points;
    }

}
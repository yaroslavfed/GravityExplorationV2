using Client.Core.Extensions;
using Client.Core.Services.ComputationalDomainService;
using Client.Core.Services.MeshService.OctreeStructure;
using Client.Core.Services.StratumService;
using Common.Data;

namespace Client.Core.Services.MeshService;

internal class MeshService : IMeshService
{
    private readonly IComputationalDomainService _computationalDomainService;
    private readonly IStratumService             _stratumService;

    public MeshService(IStratumService stratumService, IComputationalDomainService computationalDomainService)
    {
        _stratumService = stratumService;
        _computationalDomainService = computationalDomainService;
    }

    public async Task<double> GetBaseDensityAsync()
    {
        var domain = await _computationalDomainService.Domain.Value();
        return domain.DensityBase;
    }

    public async Task<Mesh> GetMeshAsync()
    {
        var stratums = (await _stratumService.StratumsList.Value()).ToList();
        var domain = await _computationalDomainService.Domain.Value();

        var pointsX = await GetMergedPointsAsync(
            stratums.Select(s => s.StartX),
            stratums.Select(s => s.EndX),
            domain.StartX,
            domain.EndX,
            domain.SplitsXCount
        );
        var pointsY = await GetMergedPointsAsync(
            stratums.Select(s => s.StartY),
            stratums.Select(s => s.EndY),
            domain.StartY,
            domain.EndY,
            domain.SplitsYCount
        );
        var pointsZ = await GetMergedPointsAsync(
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

        for (var i = 0; i < pointsX.Count - 1; i++)
        {
            for (var j = 0; j < pointsY.Count - 1; j++)
            {
                for (var k = 0; k < pointsZ.Count - 1; k++)
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
                        new()
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
            }
        }

        var mesh = new Mesh { Cells = cells };
        return mesh;
    }

    private async Task<List<double>> GetMergedPointsAsync(
        IEnumerable<double> stratumStarts,
        IEnumerable<double> stratumEnds,
        double domainStart,
        double domainEnd,
        double divisions
    )
    {
        var stratumPoints = stratumStarts.Concat(stratumEnds);
        var domainPoints = await GetPointsByDomainAsync(domainStart, domainEnd, divisions);

        var minDomain = domainPoints.Min(p => p); // Минимальная точка в domain
        var maxDomain = domainPoints.Max(p => p); // Максимальная точка в domain

        return stratumPoints
               .Concat(domainPoints)
               .Where(p => p >= minDomain && p <= maxDomain)
               .Distinct()
               .OrderBy(p => p)
               .ToList();
    }

    private Task<List<double>> GetPointsByDomainAsync(double start, double end, double divisions)
    {
        if (divisions <= 0)
            throw new ArgumentException("Количество разбиений должно быть больше 0.");

        var step = (end - start) / divisions;
        var points = new List<double>();
        for (var i = 0; i <= divisions; i++)
            points.Add(start + i * step);
        return Task.FromResult(points);
    }
}
using System.Drawing;
using Client.Core.Data;
using Client.Core.Enums;

namespace Client.Core.Services.PlotService;

public interface IPlotService
{
    Task<string> GenerateChartAsync(Domain domain, IReadOnlyList<Stratum> strata, EProjection projection);
}
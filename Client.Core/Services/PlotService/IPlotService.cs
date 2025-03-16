using System.Drawing;
using Client.Core.Data;

namespace Client.Core.Services.PlotService;

public interface IPlotService
{
    Task<string> GenerateChartAsync(Domain domain, IReadOnlyList<Stratum> strata);
}
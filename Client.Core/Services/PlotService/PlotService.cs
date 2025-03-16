using System.Diagnostics;
using System.Text.Json;
using Client.Core.Data;

namespace Client.Core.Services.PlotService;

public class PlotService : IPlotService
{
    private const string JSON_FILE    = "data.json";
    private const string PYTHON_PATH  = "python";
    private const string OUTPUT_IMAGE = "chart.png";

    public Task<string> GenerateChartAsync(Domain domain, IReadOnlyList<Stratum> strata)
    {
        var data = new { domain, strata };
        File.WriteAllText(JSON_FILE, JsonSerializer.Serialize(data));

        var currentDirectory = Directory.GetCurrentDirectory();
        var scriptPath = Path.Combine(currentDirectory, "Scripts\\chart.py");

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
        var output = process?.StandardOutput.ReadToEnd();
        var error = process?.StandardError.ReadToEnd();
        process?.WaitForExit();

        Console.WriteLine($"Python output: {output}");
        Console.WriteLine($"Python error: {error}");

        if (!File.Exists(OUTPUT_IMAGE))
            throw new($"{OUTPUT_IMAGE} not found");

        return Task.FromResult(OUTPUT_IMAGE);
    }
}
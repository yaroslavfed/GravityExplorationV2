using System.Diagnostics;
using System.Text.Json;

namespace Client.Core.Services.PlotHelperBase;

public abstract class PlotHelperBase<TData>
{
    protected string _jsonFile    = string.Empty;
    protected string _pythonPath  = string.Empty;
    protected string _outputImage = string.Empty;
    protected string _scriptName  = string.Empty;

    protected async Task<string> GeneratePlotAsync(TData @params)
    {
        try
        {
            await File.WriteAllTextAsync(_jsonFile, JsonSerializer.Serialize(@params));
        } catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }

        var currentDirectory = Directory.GetCurrentDirectory();
        var scriptPath = Path.Combine(currentDirectory, $"Scripts\\{_scriptName}");

        var psi = new ProcessStartInfo
        {
            FileName = _pythonPath,
            Arguments = $"{scriptPath} {_jsonFile} {_outputImage}",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        var process = Process.Start(psi);
        var output = await process?.StandardOutput.ReadToEndAsync()!;
        var error = await process.StandardError.ReadToEndAsync()!;
        await process.WaitForExitAsync()!;

        Console.WriteLine($"Python output: {output}");
        Console.WriteLine($"Python error: {error}");

        if (!File.Exists(_outputImage))
            throw new($"{_outputImage} not found");

        return _outputImage;
    }
}
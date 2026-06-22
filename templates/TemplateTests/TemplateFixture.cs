using System.Diagnostics;

namespace TemplateTests;

public sealed class TemplateFixture : IDisposable
{
    private readonly string _templatesRoot;
    private bool _disposed;

    public TemplateFixture()
    {
        _templatesRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", ".."));

        InstallTemplate("vsa-webapi");
        InstallTemplate("vsa-feature");
    }

    public string TemplatesRoot => _templatesRoot;

    public string CreateTempDirectory(string name)
    {
        var path = Path.Combine(Path.GetTempPath(), "net-vertical-slice-tests", name, Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(path);
        return path;
    }

    public void RunDotnet(string arguments, string workingDirectory)
    {
        using var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = arguments,
                WorkingDirectory = workingDirectory,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        process.Start();

        var standardOutput = process.StandardOutput.ReadToEnd();
        var standardError = process.StandardError.ReadToEnd();

        process.WaitForExit();

        if (process.ExitCode != 0)
        {
            throw new Xunit.Sdk.XunitException(
                $"dotnet {arguments} failed with exit code {process.ExitCode}{Environment.NewLine}" +
                $"Working directory: {workingDirectory}{Environment.NewLine}" +
                $"STDOUT:{Environment.NewLine}{standardOutput}{Environment.NewLine}" +
                $"STDERR:{Environment.NewLine}{standardError}");
        }
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        UninstallTemplate("vsa-webapi");
        UninstallTemplate("vsa-feature");
        _disposed = true;
    }

    private void InstallTemplate(string templateFolder)
    {
        var path = Path.Combine(_templatesRoot, templateFolder);
        TryRunDotnet($"new uninstall \"{path}\"", _templatesRoot);
        RunDotnet($"new install \"{path}\" --force", _templatesRoot);
    }

    private void UninstallTemplate(string templateFolder)
    {
        var path = Path.Combine(_templatesRoot, templateFolder);
        TryRunDotnet($"new uninstall \"{path}\"", _templatesRoot);
    }

    private static void TryRunDotnet(string arguments, string workingDirectory)
    {
        using var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = arguments,
                WorkingDirectory = workingDirectory,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        process.Start();
        process.WaitForExit();
    }
}

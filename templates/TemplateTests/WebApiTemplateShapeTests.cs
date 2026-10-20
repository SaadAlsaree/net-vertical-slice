using System.Diagnostics;
using System.Text.RegularExpressions;

namespace TemplateTests;

[Collection(TemplateCollection.Name)]
public sealed partial class WebApiTemplateShapeTests(TemplateFixture fixture)
{
    private static readonly string[] DockerFiles =
    [
        "Dockerfile",
        ".dockerignore",
        "docker-compose.yml",
        "docker-compose.override.yml",
        "docker-compose/docker-compose.dcproj",
        "docker-compose/launchSettings.json",
        ".env"
    ];

    public static TheoryData<string, string, string, string, string, bool> OptionMatrix => new()
    {
        { "PostgreSQL", "Scalar", "OutputCache", "None", "None", false },
        { "SqlServer", "Swagger", "None", "Keycloak", "None", false },
        { "Sqlite", "Swagger", "None", "Identity", "OpenTelemetry", false },
        { "PostgreSQL", "Swagger", "OutputCache", "Keycloak", "OpenTelemetry", true },
        { "SqlServer", "Scalar", "None", "Identity", "OpenTelemetry", true },
        { "Sqlite", "Scalar", "None", "None", "None", true }
    };

    [Theory]
    [MemberData(nameof(OptionMatrix))]
    public void WebApiTemplateLeavesNoTemplateArtifacts(
        string dbProvider,
        string apiDocsTool,
        string cacheTool,
        string authProvider,
        string observability,
        bool docker)
    {
        var root = Generate("artifacts", dbProvider, apiDocsTool, cacheTool, authProvider, observability, docker);

        var leftovers = EnumerateGeneratedFiles(root)
            .Where(path => !path.EndsWith(".md", StringComparison.OrdinalIgnoreCase))
            .SelectMany(path => File.ReadLines(path).Select((line, index) => (path, line, index)))
            .Where(entry =>
                TemplateDirective().IsMatch(entry.line) ||
                UnreplacedToken().IsMatch(entry.line))
            .Select(entry => $"{Path.GetRelativePath(root, entry.path)}:{entry.index + 1}: {entry.line.Trim()}")
            .ToList();

        Assert.True(leftovers.Count == 0, string.Join(Environment.NewLine, leftovers));
    }

    [Theory]
    [MemberData(nameof(OptionMatrix))]
    public void WebApiTemplateReferencesOnlySelectedPackages(
        string dbProvider,
        string apiDocsTool,
        string cacheTool,
        string authProvider,
        string observability,
        bool docker)
    {
        var root = Generate("packages", dbProvider, apiDocsTool, cacheTool, authProvider, observability, docker);
        var project = File.ReadAllText(Path.Combine(root, "TestApi.csproj"));
        var centralPackages = File.ReadAllText(Path.Combine(root, "Directory.Packages.props"));

        var expectedPackages = new Dictionary<string, bool>
        {
            ["Npgsql.EntityFrameworkCore.PostgreSQL"] = dbProvider == "PostgreSQL",
            ["Microsoft.EntityFrameworkCore.SqlServer"] = dbProvider == "SqlServer",
            ["Microsoft.EntityFrameworkCore.Sqlite"] = dbProvider == "Sqlite",
            ["Scalar.AspNetCore"] = apiDocsTool == "Scalar",
            ["Microsoft.AspNetCore.OpenApi"] = apiDocsTool == "Scalar",
            ["Swashbuckle.AspNetCore"] = apiDocsTool == "Swagger",
            ["Microsoft.AspNetCore.Authentication.JwtBearer"] = authProvider != "None",
            ["Microsoft.AspNetCore.Identity.EntityFrameworkCore"] = authProvider == "Identity",
            ["OpenTelemetry.Extensions.Hosting"] = observability == "OpenTelemetry",
            ["OpenTelemetry.Exporter.OpenTelemetryProtocol"] = observability == "OpenTelemetry"
        };

        foreach (var (package, expected) in expectedPackages)
        {
            var include = $"Include=\"{package}\"";
            Assert.True(
                project.Contains(include) == expected,
                $"{package} PackageReference expected={expected}");
            Assert.True(
                centralPackages.Contains(include) == expected,
                $"{package} PackageVersion expected={expected}");
        }

        var expectedConstants = new Dictionary<string, bool>
        {
            ["DB_POSTGRESQL"] = dbProvider == "PostgreSQL",
            ["DB_SQLSERVER"] = dbProvider == "SqlServer",
            ["DB_SQLITE"] = dbProvider == "Sqlite",
            ["APIDOCS_SCALAR"] = apiDocsTool == "Scalar",
            ["APIDOCS_SWAGGER"] = apiDocsTool == "Swagger",
            ["CACHE_OUTPUT"] = cacheTool == "OutputCache",
            ["AUTH_KEYCLOAK"] = authProvider == "Keycloak",
            ["AUTH_IDENTITY"] = authProvider == "Identity",
            ["AUTH_ENABLED"] = authProvider != "None",
            ["OTEL_ENABLED"] = observability == "OpenTelemetry"
        };

        foreach (var (constant, expected) in expectedConstants)
        {
            Assert.True(
                DefinedConstant(constant).IsMatch(project) == expected,
                $"{constant} DefineConstants expected={expected}");
        }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void WebApiTemplateEmitsSingleSlnxSolution(bool docker)
    {
        var root = Generate("solution", "Sqlite", "Scalar", "None", "None", "None", docker);

        var solution = Path.Combine(root, "TestApi.slnx");
        var solutionText = File.ReadAllText(solution);

        Assert.Contains("TestApi.csproj", solutionText);
        Assert.Equal(docker, solutionText.Contains("docker-compose/docker-compose.dcproj"));
        Assert.Empty(Directory.EnumerateFiles(root, "*.sln", SearchOption.AllDirectories));
        Assert.Single(Directory.EnumerateFiles(root, "*.slnx", SearchOption.AllDirectories));

        // The compose project has its own folder: sharing the web project's bin/obj made its
        // clean step delete the web project's output (MSB3030 on F5 in Visual Studio).
        Assert.False(File.Exists(Path.Combine(root, "docker-compose.dcproj")));
        var project = File.ReadAllText(Path.Combine(root, "TestApi.csproj"));
        Assert.Equal(docker, project.Contains(@"docker-compose\docker-compose.dcproj</DockerComposeProjectPath>"));
        Assert.Equal(docker, project.Contains(@"$(DefaultItemExcludes);docker-compose\**"));
    }

    [Fact]
    public void WebApiTemplateDoesNotPullRoslynBuildHostContent()
    {
        var root = Generate("buildhost", "SqlServer", "Swagger", "None", "Identity", "OpenTelemetry", docker: false);
        fixture.RunDotnet($"restore \"{Path.Combine(root, "TestApi.csproj")}\"", root);

        // EF Core Design 10.0.0 flowed Microsoft.CodeAnalysis.Workspaces.MSBuild contentFiles
        // into the project, which showed BuildHost-net472/BuildHost-netcore in Solution Explorer.
        using var assets = System.Text.Json.JsonDocument.Parse(
            File.ReadAllText(Path.Combine(root, "obj", "project.assets.json")));

        var buildHostContent = assets.RootElement.GetProperty("targets").EnumerateObject()
            .SelectMany(target => target.Value.EnumerateObject())
            .Where(library => library.Value.TryGetProperty("contentFiles", out var files) &&
                files.EnumerateObject().Any(file => file.Name.Contains("BuildHost-")))
            .Select(library => library.Name)
            .ToList();

        Assert.Empty(buildHostContent);
    }

    [Fact]
    public void WebApiTemplateUsesProjectFileNameForHyphenatedNames()
    {
        // The namespace form of "My-Api" is "My_Api", but files and the assembly keep "My-Api".
        var root = fixture.CreateTempDirectory("webapi-hyphenated-name");
        fixture.RunDotnet(
            $"new vsa-webapi -n My-Api -D PostgreSQL --Docker true --output \"{root}\" --no-update-check",
            fixture.TemplatesRoot);

        Assert.True(File.Exists(Path.Combine(root, "My-Api.csproj")));

        var solution = File.ReadAllText(Path.Combine(root, "My-Api.slnx"));
        Assert.Contains("Path=\"My-Api.csproj\"", solution);
        Assert.Contains("Path=\"docker-compose/docker-compose.dcproj\"", solution);

        var dockerfile = File.ReadAllText(Path.Combine(root, "Dockerfile"));
        Assert.Contains("COPY [\"My-Api.csproj\", \".\"]", dockerfile);
        Assert.Contains("ENTRYPOINT [\"dotnet\", \"My-Api.dll\"]", dockerfile);
        Assert.DoesNotContain("My_Api.csproj", dockerfile);

        fixture.RunDotnet($"build \"{Path.Combine(root, "My-Api.slnx")}\"", root);
        Assert.True(File.Exists(Path.Combine(root, "bin", "Debug", "net10.0", "My-Api.dll")));
    }

    [Fact]
    public void WebApiTemplateOmitsDockerFilesByDefault()
    {
        var root = Generate("docker-off", "PostgreSQL", "Scalar", "None", "None", "OpenTelemetry", docker: false);

        foreach (var file in DockerFiles)
        {
            Assert.False(File.Exists(Path.Combine(root, file)), $"{file} should not be generated");
        }
    }

    [Theory]
    [InlineData("PostgreSQL", "None")]
    [InlineData("SqlServer", "OpenTelemetry")]
    [InlineData("Sqlite", "OpenTelemetry")]
    [InlineData("Sqlite", "None")]
    public void WebApiTemplateComposesOnlySelectedServices(string dbProvider, string observability)
    {
        var root = Generate("docker-on", dbProvider, "Scalar", "None", "None", observability, docker: true);

        foreach (var file in DockerFiles)
        {
            Assert.True(File.Exists(Path.Combine(root, file)), $"{file} should be generated");
        }

        var expectedServices = new SortedSet<string>(StringComparer.Ordinal) { "api" };
        if (dbProvider == "PostgreSQL")
        {
            expectedServices.Add("postgres");
        }
        else if (dbProvider == "SqlServer")
        {
            expectedServices.Add("sqlserver");
        }

        if (observability == "OpenTelemetry")
        {
            expectedServices.Add("aspire-dashboard");
        }

        var compose = File.ReadAllText(Path.Combine(root, "docker-compose.yml"));
        var servicesSection = ComposeServicesSection().Match(compose).Groups[1].Value;
        var declaredServices = new SortedSet<string>(
            ComposeServiceDeclaration().Matches(servicesSection).Select(match => match.Groups[1].Value),
            StringComparer.Ordinal);

        Assert.Equal(expectedServices, declaredServices);
        // .env is the single configuration source: Compose holds no connection strings,
        // OTEL settings or fallback values of its own.
        Assert.Contains("env_file:", compose);
        Assert.DoesNotContain("ConnectionStrings__", compose);
        Assert.DoesNotContain("OTEL_", compose);
        Assert.False(File.Exists(Path.Combine(root, ".env.example")));
        Assert.False(File.Exists(Path.Combine(root, "template.env")));

        var environment = File.ReadAllText(Path.Combine(root, ".env"));
        Assert.Contains("ConnectionStrings__DefaultConnection=", environment);
        Assert.Equal(dbProvider == "PostgreSQL", environment.Contains("POSTGRES_PASSWORD="));
        Assert.Equal(dbProvider == "SqlServer", environment.Contains("MSSQL_SA_PASSWORD="));
        Assert.Equal(
            observability == "OpenTelemetry",
            environment.Contains("OTEL_EXPORTER_OTLP_ENDPOINT=http://aspire-dashboard:18889"));
        Assert.Contains(".env", File.ReadAllLines(Path.Combine(root, ".gitignore")));

        // `docker compose config` validates the merged YAML without a running daemon.
        // xUnit v2 has no dynamic skip, so the check is a no-op when the CLI is absent.
        var (exitCode, output) = TryRun("docker", "compose config --services", root);
        if (exitCode is null)
        {
            return;
        }

        Assert.True(exitCode == 0, output);
        Assert.Equal(
            expectedServices,
            new SortedSet<string>(
                output.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries),
                StringComparer.Ordinal));
    }

    private static Regex DefinedConstant(string constant) =>
        new($@"<DefineConstants>[^<]*;{constant}(;|<)");

    private string Generate(
        string scenario,
        string dbProvider,
        string apiDocsTool,
        string cacheTool,
        string authProvider,
        string observability,
        bool docker)
    {
        var root = fixture.CreateTempDirectory(
            $"webapi-{scenario}-{dbProvider}-{apiDocsTool}-{cacheTool}-{authProvider}-{observability}-{docker}".ToLowerInvariant());

        fixture.RunDotnet(
            $"new vsa-webapi -n TestApi -D {dbProvider} -A {apiDocsTool} -C {cacheTool} -Au {authProvider} " +
            $"--Observability {observability} --Docker {docker.ToString().ToLowerInvariant()} " +
            $"--output \"{root}\" --no-update-check",
            fixture.TemplatesRoot);

        return root;
    }

    private static IEnumerable<string> EnumerateGeneratedFiles(string root) =>
        Directory.EnumerateFiles(root, "*", SearchOption.AllDirectories)
            .Where(path =>
            {
                var relative = Path.GetRelativePath(root, path);
                var first = relative.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)[0];
                return first is not ("obj" or "bin");
            });

    private static (int? ExitCode, string Output) TryRun(string fileName, string arguments, string workingDirectory)
    {
        try
        {
            using var process = Process.Start(new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = arguments,
                WorkingDirectory = workingDirectory,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            });

            if (process is null)
            {
                return (null, string.Empty);
            }

            var output = process.StandardOutput.ReadToEnd() + process.StandardError.ReadToEnd();
            process.WaitForExit();
            return (process.ExitCode, output);
        }
        catch (System.ComponentModel.Win32Exception)
        {
            return (null, string.Empty);
        }
    }

    // Template-engine directives in any comment flavour: #if, //#if, <!--#if.
    [GeneratedRegex(@"^\s*(//|<!--)?\s*#(if|elif|elseif|else|endif)\b")]
    private static partial Regex TemplateDirective();

    // __TOKEN__ placeholders, but not ASP.NET Core double-underscore config keys
    // such as DASHBOARD__OTLP__AUTHMODE.
    [GeneratedRegex(@"(?<![A-Za-z0-9])__[A-Z][A-Z_]*__(?![A-Za-z0-9])")]
    private static partial Regex UnreplacedToken();

    // Body of the top-level `services:` key, up to the next top-level key.
    [GeneratedRegex(@"^services:\s*\n(.*?)(?=^\S)", RegexOptions.Multiline | RegexOptions.Singleline)]
    private static partial Regex ComposeServicesSection();

    [GeneratedRegex(@"^  ([a-z][a-z0-9-]*):\s*$", RegexOptions.Multiline)]
    private static partial Regex ComposeServiceDeclaration();
}

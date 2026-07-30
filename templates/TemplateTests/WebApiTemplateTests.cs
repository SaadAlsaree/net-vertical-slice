using System.Diagnostics;
using System.Net;
using System.Net.Http.Json;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;

namespace TemplateTests;

[Collection(TemplateCollection.Name)]
public sealed class WebApiTemplateTests(TemplateFixture fixture)
{
    [Theory]
    [InlineData("None")]
    [InlineData("Identity")]
    [InlineData("Keycloak")]
    public void WebApiTemplateEmitsOnlySelectedAuthenticationProvider(string authProvider)
    {
        var root = fixture.CreateTempDirectory($"webapi-auth-shape-{authProvider.ToLowerInvariant()}");

        fixture.RunDotnet(
            $"new vsa-webapi -n TestApi -D Sqlite -A Scalar -C None -Au {authProvider} --output \"{root}\" --no-update-check",
            fixture.TemplatesRoot);

        Assert.DoesNotContain(
            Directory.EnumerateFiles(root, "*.cs", SearchOption.AllDirectories),
            path => new FileInfo(path).Length == 0);

        using var appSettings = JsonDocument.Parse(
            File.ReadAllText(Path.Combine(root, "appsettings.json")));
        using var developmentSettings = JsonDocument.Parse(
            File.ReadAllText(Path.Combine(root, "appsettings.Development.json")));

        var project = File.ReadAllText(Path.Combine(root, "TestApi.csproj"));
        var rateLimiting = File.ReadAllText(Path.Combine(
            root,
            "DependencyInjection",
            "RateLimitingServiceCollectionExtensions.cs"));
        var pipeline = File.ReadAllText(Path.Combine(
            root,
            "DependencyInjection",
            "WebApplicationExtensions.cs"));

        Assert.True(appSettings.RootElement.TryGetProperty("RateLimiting", out _));
        Assert.Contains("AddRateLimiter", rateLimiting);
        Assert.Contains("UseRateLimiter", pipeline);

        switch (authProvider)
        {
            case "Identity":
                AssertIdentityShape(root, appSettings.RootElement, developmentSettings.RootElement, project);
                break;

            case "Keycloak":
                AssertKeycloakShape(root, appSettings.RootElement, developmentSettings.RootElement, project);
                break;

            default:
                AssertNoAuthenticationShape(root, appSettings.RootElement, developmentSettings.RootElement, project);
                break;
        }
    }

    [Fact]
    public void WebApiTemplateBuilds()
    {
        var root = fixture.CreateTempDirectory("webapi");

        fixture.RunDotnet(
            $"new vsa-webapi -n TestApi -D Sqlite -A Scalar -C OutputCache -Au None --output \"{root}\" --no-update-check",
            fixture.TemplatesRoot);

        fixture.RunDotnet($"build \"{Path.Combine(root, "TestApi.csproj")}\" --no-restore", root);
    }

    [Fact]
    public void WebApiTemplateBuildsWithKeycloak()
    {
        var root = fixture.CreateTempDirectory("webapi-keycloak");

        fixture.RunDotnet(
            $"new vsa-webapi -n TestApi -D Sqlite -A Scalar -C OutputCache -Au Keycloak --output \"{root}\" --no-update-check",
            fixture.TemplatesRoot);

        fixture.RunDotnet($"build \"{Path.Combine(root, "TestApi.csproj")}\" --no-restore", root);
    }

    [Theory]
    [InlineData("PostgreSQL")]
    [InlineData("SqlServer")]
    [InlineData("Sqlite")]
    public void WebApiTemplateBuildsWithIdentity(string dbProvider)
    {
        var root = fixture.CreateTempDirectory($"webapi-identity-{dbProvider}");

        fixture.RunDotnet(
            $"new vsa-webapi -n TestApi -D {dbProvider} -A Scalar -C OutputCache -Au Identity --output \"{root}\" --no-update-check",
            fixture.TemplatesRoot);

        fixture.RunDotnet($"build \"{Path.Combine(root, "TestApi.csproj")}\" --no-restore", root);
    }

    [Fact]
    public void WebApiTemplateBuildsWithOpenTelemetry()
    {
        var root = fixture.CreateTempDirectory("webapi-opentelemetry");

        fixture.RunDotnet(
            $"new vsa-webapi -n TestApi -D Sqlite -A Scalar -C None -Au None --Observability OpenTelemetry --output \"{root}\" --no-update-check",
            fixture.TemplatesRoot);

        fixture.RunDotnet($"build \"{Path.Combine(root, "TestApi.csproj")}\" --no-restore", root);

        var project = File.ReadAllText(Path.Combine(root, "TestApi.csproj"));
        var observability = File.ReadAllText(Path.Combine(
            root,
            "DependencyInjection",
            "ObservabilityServiceCollectionExtensions.cs"));

        Assert.Contains("OpenTelemetry.Exporter.OpenTelemetryProtocol", project);
        Assert.Contains(
            "AddMeter(\"Microsoft.AspNetCore.RateLimiting\")",
            observability);
        Assert.Contains("AddMeter(\"Wolverine*\")", observability);
        Assert.Contains("AddSource(\"Wolverine\")", observability);
    }

    [Theory]
    [InlineData("None")]
    [InlineData("Identity")]
    [InlineData("Keycloak")]
    public void WebApiTemplatePublishesWithPreGeneratedWolverineCode(string authProvider)
    {
        var root = fixture.CreateTempDirectory(
            $"webapi-production-{authProvider.ToLowerInvariant()}");
        var projectPath = Path.Combine(root, "TestApi.csproj");
        var publishPath = Path.Combine(root, "publish");

        fixture.RunDotnet(
            $"new vsa-webapi -n TestApi -D Sqlite -A Scalar -C None -Au {authProvider} --output \"{root}\" --no-update-check",
            fixture.TemplatesRoot);

        fixture.RunDotnet(
            $"run --project \"{projectPath}\" -c Release -p:IncludeWolverineRuntimeCompilation=true -- codegen write",
            root,
            new Dictionary<string, string?>
            {
                ["ASPNETCORE_ENVIRONMENT"] = "Development"
            });

        Assert.NotEmpty(Directory.EnumerateFiles(
            Path.Combine(root, "Internal", "Generated"),
            "*.cs",
            SearchOption.AllDirectories));

        fixture.RunDotnet(
            $"publish \"{projectPath}\" -c Release -o \"{publishPath}\" " +
            "-p:IncludeWolverineRuntimeCompilation=false " +
            "-p:IncludeEfDesignTimePackages=false",
            root);

        var dependencies = File.ReadAllText(Path.Combine(publishPath, "TestApi.deps.json"));

        Assert.DoesNotContain("WolverineFx.RuntimeCompilation", dependencies);
        Assert.DoesNotContain("Microsoft.CodeAnalysis.Workspaces.MSBuild", dependencies);
    }

    [Fact]
    public async Task IdentityValidationReturnsProblemDetailsWithoutInvocationErrorLog()
    {
        var root = fixture.CreateTempDirectory("webapi-identity-validation");
        var projectPath = Path.Combine(root, "TestApi.csproj");
        var port = GetAvailablePort();
        var output = new StringBuilder();

        fixture.RunDotnet(
            $"new vsa-webapi -n TestApi -D Sqlite -A Scalar -C None -Au Identity --output \"{root}\" --no-update-check",
            fixture.TemplatesRoot);

        using var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments =
                    $"run --project \"{projectPath}\" --no-restore --no-launch-profile " +
                    "-p:IncludeWolverineRuntimeCompilation=true",
                WorkingDirectory = root,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        process.StartInfo.Environment["ASPNETCORE_ENVIRONMENT"] = "Development";
        process.StartInfo.Environment["DOTNET_ENVIRONMENT"] = "Development";
        process.StartInfo.Environment["ASPNETCORE_URLS"] = $"http://127.0.0.1:{port}";
        process.OutputDataReceived += (_, args) => AppendOutput(output, args.Data);
        process.ErrorDataReceived += (_, args) => AppendOutput(output, args.Data);

        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        try
        {
            using var client = new HttpClient
            {
                BaseAddress = new Uri($"http://127.0.0.1:{port}")
            };

            await WaitUntilHealthy(client, process, output);

            using var response = await client.PostAsJsonAsync(
                "/api/auth/register",
                new { });
            var responseBody = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Contains("application/problem+json", response.Content.Headers.ContentType?.MediaType);
            Assert.Contains("errors", responseBody, StringComparison.OrdinalIgnoreCase);

            for (var attempt = 1; attempt < 5; attempt++)
            {
                using var validationResponse = await client.PostAsJsonAsync(
                    "/api/auth/register",
                    new { });
                Assert.Equal(HttpStatusCode.BadRequest, validationResponse.StatusCode);
            }

            using var limitedResponse = await client.PostAsJsonAsync(
                "/api/auth/register",
                new { });
            var limitedBody = await limitedResponse.Content.ReadAsStringAsync();

            Assert.Equal(
                HttpStatusCode.TooManyRequests,
                limitedResponse.StatusCode);
            Assert.NotNull(limitedResponse.Headers.RetryAfter);
            Assert.Contains("RateLimit.Exceeded", limitedBody);

            await Task.Delay(250);

            string capturedOutput;
            lock (output)
            {
                capturedOutput = output.ToString();
            }

            Assert.DoesNotContain("Invocation of RegisterCommand", capturedOutput);
            Assert.DoesNotContain("FluentValidation.ValidationException", capturedOutput);
        }
        finally
        {
            if (!process.HasExited)
            {
                process.Kill(entireProcessTree: true);
                await process.WaitForExitAsync();
            }
        }
    }

    private static void AssertIdentityShape(
        string root,
        JsonElement appSettings,
        JsonElement developmentSettings,
        string project)
    {
        Assert.True(File.Exists(Path.Combine(
            root,
            "DependencyInjection",
            "IdentityAuthenticationExtensions.cs")));
        Assert.True(File.Exists(Path.Combine(root, "Models", "JwtSettings.cs")));
        Assert.True(File.Exists(Path.Combine(root, "Features", "AuthFeature", "Register", "RegisterEndpoint.cs")));
        Assert.False(File.Exists(Path.Combine(
            root,
            "DependencyInjection",
            "KeycloakAuthenticationExtensions.cs")));
        Assert.False(File.Exists(Path.Combine(root, "Models", "KeycloakSettings.cs")));
        Assert.True(appSettings.TryGetProperty("Jwt", out _));
        Assert.True(developmentSettings.TryGetProperty("Jwt", out _));
        Assert.False(appSettings.TryGetProperty("Keycloak", out _));
        Assert.False(developmentSettings.TryGetProperty("Keycloak", out _));
        Assert.Contains("Microsoft.AspNetCore.Identity.EntityFrameworkCore", project);
    }

    private static void AssertKeycloakShape(
        string root,
        JsonElement appSettings,
        JsonElement developmentSettings,
        string project)
    {
        Assert.True(File.Exists(Path.Combine(
            root,
            "DependencyInjection",
            "KeycloakAuthenticationExtensions.cs")));
        Assert.True(File.Exists(Path.Combine(root, "Models", "KeycloakSettings.cs")));
        Assert.False(File.Exists(Path.Combine(
            root,
            "DependencyInjection",
            "IdentityAuthenticationExtensions.cs")));
        Assert.False(Directory.Exists(Path.Combine(root, "Features", "AuthFeature")));
        Assert.False(Directory.Exists(Path.Combine(root, "Infrastructure", "Authentication")));
        Assert.False(File.Exists(Path.Combine(root, "Data", "Entities", "ApplicationUser.cs")));
        Assert.False(File.Exists(Path.Combine(root, "Data", "Entities", "RefreshToken.cs")));
        Assert.True(appSettings.TryGetProperty("Keycloak", out _));
        Assert.True(developmentSettings.TryGetProperty("Keycloak", out _));
        Assert.False(appSettings.TryGetProperty("Jwt", out _));
        Assert.False(developmentSettings.TryGetProperty("Jwt", out _));
        Assert.DoesNotContain("Microsoft.AspNetCore.Identity.EntityFrameworkCore", project);
    }

    private static void AssertNoAuthenticationShape(
        string root,
        JsonElement appSettings,
        JsonElement developmentSettings,
        string project)
    {
        Assert.False(File.Exists(Path.Combine(
            root,
            "DependencyInjection",
            "KeycloakAuthenticationExtensions.cs")));
        Assert.False(File.Exists(Path.Combine(
            root,
            "DependencyInjection",
            "IdentityAuthenticationExtensions.cs")));
        Assert.False(File.Exists(Path.Combine(root, "Extensions", "AuthClaims.cs")));
        Assert.False(File.Exists(Path.Combine(root, "Extensions", "CurrentUserExtensions.cs")));
        Assert.False(File.Exists(Path.Combine(root, "Models", "CurrentUser.cs")));
        Assert.False(File.Exists(Path.Combine(root, "Models", "KeycloakSettings.cs")));
        Assert.False(File.Exists(Path.Combine(root, "Models", "JwtSettings.cs")));
        Assert.False(Directory.Exists(Path.Combine(root, "Features", "AuthFeature")));
        Assert.False(Directory.Exists(Path.Combine(root, "Infrastructure", "Authentication")));
        Assert.False(appSettings.TryGetProperty("Keycloak", out _));
        Assert.False(appSettings.TryGetProperty("Jwt", out _));
        Assert.False(developmentSettings.TryGetProperty("Keycloak", out _));
        Assert.False(developmentSettings.TryGetProperty("Jwt", out _));
        Assert.DoesNotContain("Microsoft.AspNetCore.Authentication.JwtBearer", project);
        Assert.DoesNotContain("Microsoft.AspNetCore.Identity.EntityFrameworkCore", project);
        Assert.DoesNotContain("OpenTelemetry.Extensions.Hosting", project);
    }

    private static int GetAvailablePort()
    {
        var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        var port = ((IPEndPoint)listener.LocalEndpoint).Port;
        listener.Stop();
        return port;
    }

    private static void AppendOutput(StringBuilder output, string? line)
    {
        if (line is null)
        {
            return;
        }

        lock (output)
        {
            output.AppendLine(line);
        }
    }

    private static async Task WaitUntilHealthy(
        HttpClient client,
        Process process,
        StringBuilder output)
    {
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(30));

        while (!timeout.IsCancellationRequested)
        {
            if (process.HasExited)
            {
                string capturedOutput;
                lock (output)
                {
                    capturedOutput = output.ToString();
                }

                throw new Xunit.Sdk.XunitException(
                    $"Generated API exited before becoming healthy with code {process.ExitCode}." +
                    $"{Environment.NewLine}{capturedOutput}");
            }

            try
            {
                using var response = await client.GetAsync("/health/live", timeout.Token);
                if (response.IsSuccessStatusCode)
                {
                    return;
                }
            }
            catch (HttpRequestException)
            {
                // The API is still starting.
            }

            await Task.Delay(200, timeout.Token);
        }

        throw new Xunit.Sdk.XunitException("Generated API did not become healthy within 30 seconds.");
    }
}

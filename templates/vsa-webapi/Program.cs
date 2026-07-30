using JasperFx;
using VsaTemplate.DependencyInjection;

var isCodeGeneration = args.Contains("codegen", StringComparer.OrdinalIgnoreCase);
var builder = WebApplication.CreateBuilder(args);

builder.Host
    .AddApplicationLogging()
    .AddApplicationMessaging(builder.Environment);

builder.Services
    .AddInfrastructureServices(builder.Configuration)
    .AddApplicationServices()
    .AddApplicationAuthentication(
        builder.Configuration,
        builder.Environment,
        isCodeGeneration)
    .AddApplicationObservability()
    .AddApiServices(
        builder.Configuration,
        builder.Environment);

var app = builder.Build();

app.UseApplicationPipeline();

return await app.RunJasperFxCommands(args);

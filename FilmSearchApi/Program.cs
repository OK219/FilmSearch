using System.Collections.Generic;
using FilmSearchApi.DI;
using FilmSearchApi.Filters;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Prometheus;
using Serilog;
using Serilog.Enrichers.Sensitive;
using Serilog.Events;
using Serilog.Formatting.Compact;
using Serilog.Formatting.Json;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile(
    builder.Configuration["CONFIG_PATH"] ?? "/app/config/appsettings.json",
    optional: false,
    reloadOnChange: true
);

builder.Services.Configure<FailureSettings>(
    builder.Configuration.GetSection("FailureSettings"));

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Error)
    .MinimumLevel.Override("System", LogEventLevel.Error)
    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Error)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("service.name", "FilmSearchApi")
    .Enrich.WithMachineName()
    .Enrich.WithEnvironmentName()
    .Enrich.WithSensitiveDataMasking()
    .WriteTo.Console(new CompactJsonFormatter(new JsonValueFormatter()))
    .CreateLogger();

Metrics.DefaultRegistry.SetStaticLabels(new Dictionary<string, string>
{
    { "app", "FilmSearch" },
    { "environment", builder.Environment.EnvironmentName }
});

builder.Host.UseSerilog();

builder.Services.AddControllersWithViews();

builder.Services
    .AddAuth()
    .AddS3(builder.Configuration)
    .AddDataBase(builder.Configuration)
    .AddServices();

builder.Services.AddScoped<FailureFilter>();

var app = builder.Build();

app
    .UseHttpMetrics(options =>
    {
        options.AddCustomLabel("http_route", context =>
        {
            var endpoint = context.GetEndpoint() as RouteEndpoint;
            return endpoint?.RoutePattern.RawText ?? context.Request.Path.Value ?? "/";
        });
    })
    .UseRouting()
    .UseAuthentication()
    .UseAuthorization()
    .UseExceptionHandler(errorApp =>
    {
        errorApp.Run(async context =>
        {
            context.Response.StatusCode = 500;
            await context.Response.WriteAsJsonAsync(new
            {
                title = "Internal Server Error",
                status = 500,
            });
        });
    });
;


app.MapStaticAssets();
app.MapControllers().WithStaticAssets();
var metricServer = new KestrelMetricServer(port: int.Parse(builder.Configuration["MetricsPort"] ?? "8081"));
metricServer.Start();

app.Run();
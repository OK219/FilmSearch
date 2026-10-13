using System;
using FilmSearchApi.DI;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Enrichers.Sensitive;
using Serilog.Events;
using Serilog.Formatting.Compact;
using Serilog.Formatting.Json;

var builder = WebApplication.CreateBuilder(args);

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

builder.Host.UseSerilog();

builder.Services.AddControllersWithViews();

builder.Services
    .AddAuth()
    .AddS3(builder.Configuration)
    .AddDataBase(builder.Configuration)
    .AddServices();

var app = builder.Build();

app
    .UseRouting()
    .UseAuthentication()
    .UseAuthorization();
app.MapStaticAssets();
app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}")
    .WithStaticAssets();

app.Run();
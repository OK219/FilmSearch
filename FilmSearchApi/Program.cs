using FilmSearchApi.DI;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

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
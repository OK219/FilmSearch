using System;
using Amazon.S3;
using FilmSearchApi.Authorization;
using FilmSearchApi.DB;
using FilmSearchApi.DB.Repositories;
using FilmSearchApi.Services;
using FilmSearchApi.Services.Configs;
using FilmSearchApi.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace FilmSearchApi.DI;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAuth(this IServiceCollection services)
    {
        services
            .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(options =>
            {
                options.LoginPath = "/Account/Login";
                options.LogoutPath = "/Account/Login";
                options.AccessDeniedPath = "/Account/Login";
                options.ExpireTimeSpan = TimeSpan.FromDays(7);
                options.SlidingExpiration = true;
                options.Cookie.HttpOnly = true;
                options.Cookie.SameSite = SameSiteMode.Lax;
            });

        services.AddSingleton<IAuthorizationHandler, AdminAuthorizationHandler>();

        services.AddAuthorization(options =>
        {
            options.AddPolicy("AdminOnly", policy =>
            {
                policy.Requirements.Add(new AdminRequirement());
            });
        });

        return services;
    }

    public static IServiceCollection AddS3(this IServiceCollection services, IConfiguration configuration)
    {
        
        services.Configure<S3Configuration>(configuration.GetSection("S3"));

        services.AddSingleton<IAmazonS3>(sp =>
        {
            var opts = sp.GetRequiredService<IOptions<S3Configuration>>().Value;

            var config = new AmazonS3Config
            {
                ServiceURL = opts.ServiceUrl,
                ForcePathStyle = true,
                AuthenticationRegion = opts.AuthenticationRegion
            };

            return new AmazonS3Client(opts.AccessKey, opts.SecretKey, config);
        });

        return services;
    }

    public static IServiceCollection AddDataBase(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<FilmDbContext>(x => x.UseNpgsql(configuration["ConnectionStrings:Default"]));
        services.AddScoped<UserRepository>();
        services.AddScoped<FilmRepository>();
        services.AddScoped<ReviewRepository>();

        return services;
    }

    public static IServiceCollection AddServices(this IServiceCollection services)
    {
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IFilmService, FilmService>();
        return services;
    }
}
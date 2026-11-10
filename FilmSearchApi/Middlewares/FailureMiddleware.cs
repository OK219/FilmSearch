using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace FilmSearchApi.Middlewares;

public class FailureMiddleware
{
    private FailureSettings settings;
    private readonly RequestDelegate next;

    public FailureMiddleware(IOptionsMonitor<FailureSettings> settings, RequestDelegate next)
    {
        this.settings = settings.CurrentValue;
        settings.OnChange(x => this.settings = x);
        this.next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (settings.Enabled)
        {
            if (Random.Shared.NextDouble() > settings.ErrorChance)
                throw new Exception();
            Thread.Sleep(Random.Shared.Next(settings.Latency));
        }

        await next.Invoke(context);
    }
}
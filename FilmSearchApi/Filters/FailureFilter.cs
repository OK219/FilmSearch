using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;

namespace FilmSearchApi.Filters;

public class FailureFilter : IAsyncActionFilter
{
    private readonly IOptionsMonitor<FailureSettings> settings;

    public FailureFilter(IOptionsMonitor<FailureSettings> settings)
    {
        this.settings = settings;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var currentSettings = settings.CurrentValue;

        if (currentSettings.Enabled)
        {
            if (Random.Shared.NextDouble() > currentSettings.ErrorChance)
                throw new Exception();

            Thread.Sleep(Random.Shared.Next(currentSettings.Latency));
        }

        await next();
    }
}

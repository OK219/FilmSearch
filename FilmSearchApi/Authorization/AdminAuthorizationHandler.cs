using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;

namespace FilmSearchApi.Authorization;

public class AdminAuthorizationHandler : AuthorizationHandler<AdminRequirement>
{
    private readonly string? _adminEmail;

    public AdminAuthorizationHandler(IConfiguration configuration)
    {
        _adminEmail = configuration["AdminEmail"];
    }

    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, AdminRequirement requirement)
    {
        if (IsAdmin(context.User))
            context.Succeed(requirement);

        return Task.CompletedTask;
    }

    private bool IsAdmin(ClaimsPrincipal user)
    {
        var email = user.FindFirst(ClaimTypes.Email)?.Value;
        return IsAdminEmail(email);
    }

    private bool IsAdminEmail(string? email)
    {
        if (string.IsNullOrWhiteSpace(_adminEmail) || string.IsNullOrWhiteSpace(email))
            return false;

        return string.Equals(email, _adminEmail, StringComparison.OrdinalIgnoreCase);
    }
}
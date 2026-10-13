using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using FilmSearchApi.Models;
using FilmSearchApi.Services.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace FilmSearchApi.Controllers;

public class AccountController : Controller
{
    private readonly IUserService _userService;
    private readonly ILogger<AccountController> _logger;

    public AccountController(IUserService userService, ILogger<AccountController> logger)
    {
        _userService = userService;
        _logger = logger;
    }


    [HttpGet]
    public IActionResult Register() => View();

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Register(RegisterViewModel model, CancellationToken ct)
    {
        if (!ModelState.IsValid)
            return View(model);

        var result = await _userService.RegisterAsync(model.Email, model.Password, ct);
        if (result.IsFailed)
        {
            ModelState.AddModelError("FailReason", "На этот Email уже зарегистрирован аккаунт ");
            return View(model);
        }

        await LoginUser(model.Email, result.Value);
        _logger.LogInformation("User registered: {UserId}", result.Value);
        return RedirectToAction("Index", "Film");
    }

    [HttpPost]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl, CancellationToken ct)
    {
        if (!ModelState.IsValid)
            return View(model);

        var user = await _userService.FindByLoginAsync(model.Login, ct);

        if (user is null || !_userService.VerifyPassword(model.Password, user.PasswordHash))
        {
            _logger.LogWarning("Failed login attempt for {Email}", model.Login);
            ModelState.AddModelError("FailReason", "Неверный email или пароль");
            return View(model);
        }

        await LoginUser(user.Email, user.Id);

        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);

        return RedirectToAction("Index", "Film");
    }

    [HttpPost]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Register", "Account");
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userIdClaim, out var userId))
        {
            return RedirectToAction("Logout", "Account");
        }

        var model = await _userService.GetProfileAsync(userId);
        if (model is null)
            return NotFound();

        return View(model);
    }

    private async Task LoginUser(string userEmail, Guid userId)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId.ToString()),
            new(ClaimTypes.Email, userEmail),
        };

        var identity = new ClaimsIdentity(claims,
            CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal,
            new AuthenticationProperties { IsPersistent = true });

        _logger.LogInformation("User logged in: {UserId}", userId);
    }
}
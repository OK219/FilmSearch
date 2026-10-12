using System;
using System.Security.Claims;
using System.Threading.Tasks;
using FilmSearchApi.DB;
using FilmSearchApi.Models;
using FilmSearchApi.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FilmSearchApi.Controllers;

public class FilmController : Controller
{
    private readonly IFilmService _filmService;

    public FilmController(IFilmService filmService)
    {
        _filmService = filmService;
    }

    [HttpGet]
    [HttpPost]
    public async Task<IActionResult> Index([FromForm] FilmFilterAndOrderSettings filter, int page = 1, int pageSize = 12)
    {
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 12;
        if (pageSize > 60) pageSize = 60;

        var result = await _filmService.GetFilmsPagedAsync(filter, page, pageSize);

        if (result.TotalPages > 0 && page > result.TotalPages)
        {
            page = result.TotalPages;
            result = await _filmService.GetFilmsPagedAsync(filter, page, pageSize);
        }

        return View(new FilmListViewModel
        {
            Results = result,
            Filter = filter
        });
    }

    [Authorize]
    [HttpGet]
    public IActionResult Add() => View(new AddFilmViewModel());

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequestSizeLimit(10 * 1024 * 1024)]
    public async Task<IActionResult> Add(AddFilmViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        try
        {
            var filmId = await _filmService.AddFilmAsync(
                model.Name,
                model.Description,
                model.GenreName,
                model.ReleaseDate,
                model.Poster);

            return RedirectToAction(nameof(Details), new { id = filmId });
        }
        catch (ArgumentException ex)
        {
            ModelState.AddModelError(nameof(model.Poster), ex.Message);
            return View(model);
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", "Не удалось загрузить постер. Попробуйте позже.");
            return View(model);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Details(Guid id)
    {
        var currentUserId = GetCurrentUserId();
        var model = await _filmService.GetFilmDetailsAsync(id, currentUserId);

        if (model is null)
            return NotFound();

        return View(model);
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddReview(Guid filmId, int rating, string? reviewText)
    {
        var userId = GetCurrentUserId();
        if (userId is null)
            return RedirectToAction("Login", "Account");

        if (rating is < 1 or > 10)
        {
            TempData["ReviewError"] = "Рейтинг должен быть от 1 до 10";
            return RedirectToAction(nameof(Details), new { id = filmId });
        }

        await _filmService.AddOrUpdateReviewAsync(userId.Value, filmId, rating, reviewText);
        return RedirectToAction(nameof(Index));
    }
    
    [HttpPost]
    [Authorize(Policy = "AdminOnly")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteFilm(Guid id)
    {
        await _filmService.DeleteFilmAsync(id);
        return RedirectToAction(nameof(Index));
    }
    

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteReview(Guid filmId)
    {
        var userId = GetCurrentUserId();
        var deleted = await _filmService.DeleteReviewAsync(userId.Value, filmId);
        if (!deleted)
            return NotFound();

        return RedirectToAction(nameof(Details), new { id = filmId });
    }

    private Guid? GetCurrentUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(claim, out var id) ? id : null;
    }
}
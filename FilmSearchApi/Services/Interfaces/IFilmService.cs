using System;
using System.Threading.Tasks;
using FilmSearchApi.DB;
using FilmSearchApi.Models;
using Microsoft.AspNetCore.Http;

namespace FilmSearchApi.Services.Interfaces;

public interface IFilmService
{
    Task<PagedResult<FilmListItemDto>> GetFilmsPagedAsync(FilmFilterAndOrderSettings options, int page, int pageSize);
    Task<Guid> AddFilmAsync(string name, string description, string genreName, DateOnly releaseDate, IFormFile poster);
    Task<FilmDetailsDto?> GetFilmDetailsAsync(Guid filmId, Guid? currentUserId);
    Task AddOrUpdateReviewAsync(Guid userId, Guid filmId, int rating, string? reviewText);
    Task DeleteFilmAsync(Guid filmId);
    Task<bool> DeleteReviewAsync(Guid userId, Guid filmId);
}
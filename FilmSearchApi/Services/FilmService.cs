using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Model;
using FilmSearchApi.DB;
using FilmSearchApi.DB.Repositories;
using FilmSearchApi.Models;
using FilmSearchApi.Services.Configs;
using FilmSearchApi.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FilmSearchApi.Services;

public class FilmService : IFilmService
{
    private const int DefaultPageSize = 12;
    private const int MaxPageSize = 60;
    private const long MaxFileSizeBytes = 5 * 1024 * 1024;
    private const string S3KeyPrefix = "posters/";

    private readonly FilmRepository _filmRepository;
    private readonly ReviewRepository _reviewRepository;
    private readonly ILogger<FilmService> _logger;

    private readonly IAmazonS3 _s3;
    private readonly S3Configuration _options;

    private static readonly HashSet<string> AllowedPosterContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/jpeg",
        "image/png",
        "image/webp"
    };

    public FilmService(FilmRepository filmRepository, IAmazonS3 s3, IOptions<S3Configuration> options, ILogger<FilmService> logger, ReviewRepository reviewRepository)
    {
        _filmRepository = filmRepository;
        _reviewRepository = reviewRepository;
        _options = options.Value;
        _s3 = s3;
        _logger = logger;
    }

    public async Task<PagedResult<FilmListItemDto>> GetFilmsPagedAsync(FilmFilterAndOrderSettings settings, int page, int pageSize)
    {
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = DefaultPageSize;
        if (pageSize > MaxPageSize) pageSize = MaxPageSize;

        var (items, totalCount) = await _filmRepository.GetFilms(settings, page, pageSize);

        var dtos = items.Select(x => new FilmListItemDto()
        {
            AverageRating = x.Rating,
            Description = x.Description,
            GenreName = x.Genre?.Name,
            Id = x.Id,
            Name = x.Name,
            PosterKey = x.PosterUrl,
            ReleaseDate = x.ReleaseDate,
            ReviewCount = 0,
        }).ToList();

        _logger.LogDebug(
            "Fetched {Count} films for page {Page}/{TotalPages}",
            items.Count, page,
            totalCount > 0 ? (int)Math.Ceiling((double)totalCount / pageSize) : 0);

        return new PagedResult<FilmListItemDto>
        {
            Items = dtos,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    public async Task<Guid> AddFilmAsync(string name, string description, string genreName, DateOnly releaseDate, IFormFile poster)
    {
        var posterUrl = await UploadPosterAsync(poster);
        return await _filmRepository.RegisterFilmAsync(name, description, genreName, releaseDate, posterUrl);
    }

    public async Task<FilmDetailsDto?> GetFilmDetailsAsync(Guid filmId, Guid? currentUserId)
    {
        var film = await _filmRepository.GetFilmAsync(filmId);
        if (film is null)
            return null;

        var reviews = await _reviewRepository.GetFilmReviewsAsync(filmId);

        ReviewDto? userReview = null;
        if (currentUserId.HasValue)
        {
            var curUserReview = reviews.FirstOrDefault(r => r.UserId == currentUserId.Value);
            if (curUserReview is not null)
            {
                userReview = new ReviewDto
                {
                    UserId = curUserReview.UserId,
                    UserEmail = curUserReview.User.Email,
                    Rating = curUserReview.Rating,
                    ReviewText = curUserReview.ReviewText,
                    IsCurrentUser = true
                };
            }
        }

        return new FilmDetailsDto
        {
            Id = film.Id,
            Name = film.Name,
            Description = film.Description,
            ReleaseDate = film.ReleaseDate,
            PosterUrl = film.PosterUrl,
            GenreName = film.Genre?.Name,
            AverageRating = film.Rating,
            ReviewCount = reviews.Count,
            Reviews = reviews.Select(r => new ReviewDto
            {
                UserId = r.UserId,
                UserEmail = r.User.Email,
                Rating = r.Rating,
                ReviewText = r.ReviewText,
                IsCurrentUser = currentUserId.HasValue && r.UserId == currentUserId.Value
            }).ToList(),
            UserReview = userReview,
            CanReview = currentUserId.HasValue
        };
    }
    
    public async Task AddOrUpdateReviewAsync(Guid userId, Guid filmId, int rating, string? reviewText)
    {
        if (rating is < 1 or > 10)
            throw new ArgumentOutOfRangeException(nameof(rating), "Рейтинг должен быть от 1 до 10");

        var existing = await _reviewRepository.GetUserReviewAsync(userId, filmId);

        if (existing is null)
        {
            await _reviewRepository.AddReviewAsync(userId, filmId, rating, reviewText ?? string.Empty);
            _logger.LogInformation("User {UserId} added review for film {FilmId}", userId, filmId);
        }
        else
        {
            await _reviewRepository.UpdateReviewAsync(userId, filmId, rating, reviewText ?? string.Empty);
            _logger.LogInformation("User {UserId} updated review for film {FilmId}", userId, filmId);
        }
    }
    
    public async Task DeleteFilmAsync(Guid filmId)
    {
        var film = await _filmRepository.GetFilmAsync(filmId);
        if (film is null)
        {
            _logger.LogWarning("Attempted to delete non-existent film {FilmId}", filmId);
            return;
        }
        
        try
        {
            var posterKey = film.PosterUrl?.Substring(film.PosterUrl.IndexOf(S3KeyPrefix));
            await _s3.DeleteAsync(_options.BucketName, posterKey, null);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to delete poster {Key} for film {FilmId}", film.PosterUrl, filmId);
        }
        

        await _filmRepository.DeleteFilm(filmId);
        _logger.LogInformation("Deleted film {FilmId}: {Name}", filmId, film.Name);
    }

    public async Task<bool> DeleteReviewAsync(Guid userId, Guid filmId)
    {
        var review = await _reviewRepository.GetUserReviewAsync(userId, filmId);
        if (review is null)
        {
            _logger.LogWarning("User {UserId} tried to delete review for {FilmId} that doesn't exist", userId, filmId);
            return false;
        }

        await _reviewRepository.DeleteUserReviewAsync(userId, filmId);
        _logger.LogInformation("User {UserId} deleted their review for film {FilmId}", userId, filmId);
        return true;
    }

    private async Task<string> UploadPosterAsync(IFormFile file)
    {
        Validate(file);

        var extension = GetExtension(file.ContentType);
        var key = $"{S3KeyPrefix}{Guid.NewGuid():N}{extension}";

        await using var stream = file.OpenReadStream();

        var request = new PutObjectRequest
        {
            BucketName = _options.BucketName,
            Key = key,
            InputStream = stream,
            ContentType = file.ContentType,
            AutoCloseStream = false
        };

        await _s3.PutObjectAsync(request);

        return $"{_options.ServiceUrl.TrimEnd('/')}/{_options.BucketName}/{key}";
    }

    private static void Validate(IFormFile file)
    {
        if (file is null || file.Length == 0)
            throw new ArgumentException("Файл не выбран");

        if (file.Length > MaxFileSizeBytes)
            throw new ArgumentException("Файл слишком большой (максимум 5 МБ)");

        if (!AllowedPosterContentTypes.Contains(file.ContentType))
            throw new ArgumentException("Разрешены только JPG, PNG и WEBP");
    }

    private static string GetExtension(string contentType) => contentType.ToLowerInvariant() switch
    {
        "image/jpeg" => ".jpg",
        "image/png" => ".png",
        "image/webp" => ".webp",
        _ => ".jpg"
    };
}
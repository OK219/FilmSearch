using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FilmSearchApi.DB.Models;
using FilmSearchApi.DB.Repositories;
using FilmSearchApi.Models;
using FilmSearchApi.Services.Interfaces;
using FluentResults;
using Microsoft.Extensions.Logging;


namespace FilmSearchApi.Services;

public class UserService : IUserService
{
    private readonly UserRepository _userRepository;
    private readonly ReviewRepository _reviewRepository;
    private readonly ILogger<UserService> _logger;

    public UserService(UserRepository userRepository, ReviewRepository reviewRepository, ILogger<UserService> logger)
    {
        _userRepository = userRepository;
        _reviewRepository = reviewRepository;
        _logger = logger;
    }

    public async Task<Result<Guid>> RegisterAsync(string email, string password, CancellationToken ct = default)
    {
        email = email.Trim();

        if (await _userRepository.GetUser(email) is not null)
        {
            return Result.Fail<Guid>(new Error("Этот email уже зарегистрирован")
                .WithMetadata("Field", nameof(email)));
        }

        var passwordHash = BCrypt.Net.BCrypt.HashPassword(password);

        try
        {
            var userId = await _userRepository.RegisterUser(email, passwordHash);
            _logger.LogInformation("Registered new user {UserId} ({Email})", userId, email);
            return Result.Ok(userId);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Registration race for {Email}", email);
            return Result.Fail<Guid>("Пользователь с такими данными уже существует");
        }
    }

    public async Task<User?> FindByLoginAsync(string email, CancellationToken ct = default)
    {
        return await _userRepository.GetUser(email.Trim());
    }

    public bool VerifyPassword(string password, string passwordHash)
    {
        if (string.IsNullOrEmpty(passwordHash)) return false;

        try
        {
            return BCrypt.Net.BCrypt.Verify(password, passwordHash);
        }
        catch
        {
            return false;
        }
    }
    
    public async Task<ProfileViewModel?> GetProfileAsync(Guid userId, CancellationToken ct = default)
    {
        var user = await _userRepository.GetUser(userId);
        if (user is null)
        {
            _logger.LogWarning("Profile requested for non-existent user {UserId}", userId);
            return null;
        }

        var reviews = await _reviewRepository.GetUserReviewsWithFilm(userId);

        return new ProfileViewModel
        {
            Id = user.Id,
            Email = user.Email,
            Reviews = reviews.Select(r => new UserReviewDto
            {
                FilmId = r.FilmId,
                FilmName = r.Film.Name,
                FilmPosterUrl = r.Film.PosterUrl,
                Rating = r.Rating,
                ReviewText = r.ReviewText
            }).ToList()
        };
    }
}
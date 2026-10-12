using System;
using System.Threading;
using System.Threading.Tasks;
using FilmSearchApi.DB.Models;
using FilmSearchApi.Models;
using FluentResults;

namespace FilmSearchApi.Services.Interfaces;

public interface IUserService
{
    Task<Result<Guid>> RegisterAsync(string email, string password, CancellationToken ct = default);
    Task<User?> FindByLoginAsync(string email, CancellationToken ct = default);
    bool VerifyPassword(string password, string passwordHash);
    Task<ProfileViewModel?> GetProfileAsync(Guid userId, CancellationToken ct = default);
}
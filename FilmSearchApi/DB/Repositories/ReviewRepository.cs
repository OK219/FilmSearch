using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FilmSearchApi.DB.Models;
using Microsoft.EntityFrameworkCore;

namespace FilmSearchApi.DB.Repositories;

public class ReviewRepository(FilmDbContext dbContext)
{
    public async Task<Review?> GetUserReviewAsync(Guid userId, Guid filmId)
    {
        return await dbContext.Reviews.FirstOrDefaultAsync(x => x.UserId == userId && x.FilmId == filmId);
    }
    
    public async Task DeleteUserReviewAsync(Guid userId, Guid filmId)
    {
        await dbContext.Reviews.Where(x => x.UserId == userId && x.FilmId == filmId).ExecuteDeleteAsync();
        await UpdateFilmRating(filmId);
    }
    
    public async Task<List<Review>> GetFilmReviewsAsync(Guid filmId)
    {
        return await dbContext.Reviews
            .AsNoTracking()
            .Include(r => r.User)
            .Where(r => r.FilmId == filmId)
            .OrderByDescending(r => r.Rating)
            .ThenBy(r => r.User.Email)
            .ToListAsync();
    }
    
    public async Task<List<Review>> GetUserReviewsWithFilm(Guid userId)
    {
        return await dbContext.Reviews
            .AsNoTracking()
            .Include(r => r.Film)
            .Where(r => r.UserId == userId)
            .ToListAsync();
    }

    public async Task AddReviewAsync(Guid userId, Guid filmId, int rating, string reviewText)
    {
        dbContext.Reviews.Add(new Review { FilmId = filmId, UserId = userId, Rating = rating, ReviewText = reviewText });
        await dbContext.SaveChangesAsync();
        await UpdateFilmRating(filmId);
    }

    public async Task UpdateReviewAsync(Guid userId, Guid filmId, int rating, string reviewText)
    {
        var review = await GetUserReviewAsync(userId, filmId);
        if (review is null)
            return;

        review.ReviewText = reviewText;
        review.Rating = rating;
        dbContext.Reviews.Update(review);
        await dbContext.SaveChangesAsync();
        await UpdateFilmRating(filmId);
    }

    private async Task UpdateFilmRating(Guid filmId)
    {
        var film = await dbContext.Films.FirstAsync(x => x.Id == filmId);
        var newAvgRating = dbContext.Reviews.Where(x => x.FilmId == filmId).Average(x => x.Rating);
        film.Rating = newAvgRating;
        dbContext.Films.Update(film);
        await dbContext.SaveChangesAsync();
    }
}
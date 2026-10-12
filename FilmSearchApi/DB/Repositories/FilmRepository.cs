using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FilmSearchApi.DB.Models;
using Microsoft.EntityFrameworkCore;

namespace FilmSearchApi.DB.Repositories;

public class FilmRepository(FilmDbContext dbContext)
{
    public async Task<Film?> GetFilmAsync(Guid id)
    {
        return await dbContext
            .Films
            .Include(x => x.Genre)
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<(List<Film> Items, int TotalCount)> GetFilms(
        FilmFilterAndOrderSettings? filterAndOrdering = null, 
        int page = 1, 
        int pageSize = 50)
    {
        filterAndOrdering ??= new FilmFilterAndOrderSettings();

        var filteredFilms = dbContext.Films
            .Include(x => x.Genre)
            .FilterByName(filterAndOrdering.Name)
            .FilterByReleaseDate(filterAndOrdering.MinDate, filterAndOrdering.MaxDate);

        var totalCount = await filteredFilms.CountAsync();

        var films = filterAndOrdering.OrderBy switch
        {
            FilmOrder.ByDate => filteredFilms.OrderByDate(filterAndOrdering.IsDescending),
            FilmOrder.ByName => filteredFilms.OrderByName(filterAndOrdering.IsDescending),
            FilmOrder.ByRating => filteredFilms.OrderByRating(filterAndOrdering.IsDescending),
            _ => filteredFilms
        };

        var items = await films.GetPage(page, pageSize);
        return (items, totalCount);
    }

    public async Task<Guid> RegisterFilmAsync(string name, string description, string genreName, DateOnly releaseDate, string posterUrl)
    {
        var film = new Film
        {
            Name = name, Description = description, ReleaseDate = releaseDate, PosterUrl = posterUrl
        };

        var genre = await dbContext.Genres.FirstOrDefaultAsync(x => x.Name == genreName);
        if (genre is null)
        {
            genre = new Genre { Name = genreName };
            dbContext.Genres.Add(genre);
        }

        film.Genre = genre;
        dbContext.Films.Add(film);
        await dbContext.SaveChangesAsync();

        return film.Id;
    }

    public async Task DeleteFilm(Guid filmId)
    {
        await dbContext.Films.Where(x => x.Id == filmId).ExecuteDeleteAsync();
    }
}
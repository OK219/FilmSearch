using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FilmSearchApi.DB.Models;
using Microsoft.EntityFrameworkCore;

namespace FilmSearchApi.DB.Repositories;

public class GenreRepository(FilmDbContext dbContext)
{
    public async Task<List<Genre>> GetGenres()
    {
        return await dbContext.Genres.ToListAsync();
    }
    
    public async Task<Genre?> GetGenre(Guid id)
    {
        return await dbContext.Genres.FirstOrDefaultAsync(x => x.Id == id);
    }
}
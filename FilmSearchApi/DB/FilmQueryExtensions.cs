using System;
using System.Linq;
using FilmSearchApi.DB.Models;
using Microsoft.EntityFrameworkCore;

namespace FilmSearchApi.DB;

public static class FilmQueryExtensions
{
    public static IOrderedQueryable<Film> OrderByRating(this IQueryable<Film> query, bool isDescending = false)
    {
        return isDescending ? query.OrderByDescending(x => x.Rating) : query.OrderBy(x => x.Rating);
    }
    
    public static IOrderedQueryable<Film> OrderByDate(this IQueryable<Film> query, bool isDescending = false)
    {
        return isDescending ? query.OrderByDescending(x => x.ReleaseDate) : query.OrderBy(x => x.ReleaseDate);
    }
    
    public static IOrderedQueryable<Film> OrderByName(this IQueryable<Film> query, bool isDescending = false)
    {
        return isDescending ? query.OrderByDescending(x => x.Name.ToLower()) : query.OrderBy(x => x.Name.ToLower());
    }
    
    public static IQueryable<Film> FilterByName(this IQueryable<Film> query, string? name)
    {
        if (string.IsNullOrEmpty(name))
            return query;
        
        return query.Where(x => EF.Functions.ILike(x.Name, $"%{name}%"));
    }
    
    public static IQueryable<Film> FilterByReleaseDate(this IQueryable<Film> query, DateOnly? min = null, DateOnly? max = null)
    {
        min ??= DateOnly.MinValue;
        max ??= DateOnly.MaxValue;
        
        return query.Where(x => x.ReleaseDate >= min && x.ReleaseDate <= max);
    }
}
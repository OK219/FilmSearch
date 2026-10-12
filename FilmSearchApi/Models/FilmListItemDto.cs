using System;

namespace FilmSearchApi.Models;

public class FilmListItemDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public DateOnly ReleaseDate { get; set; }
    public string? PosterKey { get; set; }
    public string? GenreName { get; set; }
    public double? AverageRating { get; set; }
    public int ReviewCount { get; set; }
}
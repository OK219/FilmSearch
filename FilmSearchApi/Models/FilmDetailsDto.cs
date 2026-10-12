using System;
using System.Collections.Generic;

namespace FilmSearchApi.Models;

public class FilmDetailsDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public DateOnly ReleaseDate { get; set; }
    public string? PosterUrl { get; set; }
    public string? GenreName { get; set; }
    public double? AverageRating { get; set; }
    public int ReviewCount { get; set; }
    public List<ReviewDto> Reviews { get; set; } = new();
    public ReviewDto? UserReview { get; set; }
    public bool CanReview { get; set; }
}
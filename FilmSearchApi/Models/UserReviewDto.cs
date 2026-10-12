using System;

namespace FilmSearchApi.Models;

public class UserReviewDto
{
    public Guid FilmId { get; set; }
    public string FilmName { get; set; } = null!;
    public string? FilmPosterUrl { get; set; }
    public int Rating { get; set; }
    public string? ReviewText { get; set; }
}
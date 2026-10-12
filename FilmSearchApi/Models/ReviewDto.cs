using System;

namespace FilmSearchApi.Models;

public class ReviewDto
{
    public Guid UserId { get; set; }
    public string UserEmail { get; set; } = null!;
    public int Rating { get; set; }
    public string? ReviewText { get; set; }
    public bool IsCurrentUser { get; set; }
}
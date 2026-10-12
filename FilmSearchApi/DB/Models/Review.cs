using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace FilmSearchApi.DB.Models;

[Table("reviews")]
public class Review
{
    [Column("user_id")]
    public Guid UserId { get; set; }

    [ForeignKey(nameof(UserId))]
    public User User { get; set; } = null!;

    [Column("film_id")]
    public Guid FilmId { get; set; }

    [ForeignKey(nameof(FilmId))]
    public Film Film { get; set; } = null!;

    [Column("review_text")]
    public string? ReviewText { get; set; }

    [Column("rating")]
    public int Rating { get; set; }
}
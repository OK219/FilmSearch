using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FilmSearchApi.DB.Models;

[Table("film")]
public class Film
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    [Column("genre_id")]
    public Guid GenreId { get; set; }

    [ForeignKey(nameof(GenreId))]
    public Genre Genre { get; set; } = null!;

    [Required]
    [MaxLength(255)]
    [Column("name")]
    public string Name { get; set; } = null!;

    [Column("description")]
    public string? Description { get; set; }
    
    [Column("posster_url")]
    public string? PosterUrl { get; set; }
    
    [Column("release_date")]
    public DateOnly ReleaseDate { get; set; }
    
    [Column("rating")]
    public double Rating { get; set; }

    public ICollection<Review> Reviews { get; set; } = new List<Review>();
}
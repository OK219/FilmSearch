using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FilmSearchApi.DB.Models;

[Table("users")]
public class User
{
    [Key] [Column("id")] public Guid Id { get; set; }

    [Required]
    [MaxLength(100)]
    [EmailAddress]
    [Column("email")]
    public string Email { get; set; } = null!;

    [Required]
    [MaxLength(60)]
    [Column("password_encrypted", TypeName = "char(60)")]
    public string PasswordHash { get; set; } = null!;

    public ICollection<Review> Reviews { get; set; } = new List<Review>();
}
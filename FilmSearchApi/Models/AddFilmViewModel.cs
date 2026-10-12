using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace FilmSearchApi.Models;

public class AddFilmViewModel
{
    [Required(ErrorMessage = "Название обязательно")]
    [MaxLength(255)]
    [Display(Name = "Название")]
    public string Name { get; set; } = null!;

    [Required(ErrorMessage = "Описание обязательно")]
    [MaxLength(2000)]
    [Display(Name = "Описание")]
    public string Description { get; set; } = null!;

    [Required(ErrorMessage = "Жанр обязателен")]
    [MaxLength(100)]
    [Display(Name = "Жанр")]
    public string GenreName { get; set; } = null!;

    [Required(ErrorMessage = "Дата релиза обязательна")]
    [DataType(DataType.Date)]
    [Display(Name = "Дата релиза")]
    public DateOnly ReleaseDate { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow);

    [Required(ErrorMessage = "Постер обязателен")]
    [Display(Name = "Постер")]
    public IFormFile Poster { get; set; } = null!;
}
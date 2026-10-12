using System.ComponentModel.DataAnnotations;

namespace FilmSearchApi.Models;

public class LoginViewModel
{
    [Required(ErrorMessage = "Введите email или никнейм")]
    [Display(Name = "Email или никнейм")]
    public string Login { get; set; } = null!;

    [Required(ErrorMessage = "Введите пароль")]
    [DataType(DataType.Password)]
    [Display(Name = "Пароль")]
    public string Password { get; set; } = null!;

    [Display(Name = "Запомнить меня")]
    public bool RememberMe { get; set; }
}
using System;
using System.Collections.Generic;

namespace FilmSearchApi.Models;

public class ProfileViewModel
{
    public Guid Id { get; set; }
    public string Email { get; set; } = null!;
    public List<UserReviewDto> Reviews { get; set; } = new();
}
using System;

namespace FilmSearchApi.DB;

public class FilmFilterAndOrderSettings
{
    public string? Name { get; set; }
    public DateOnly MinDate { get; set; } = DateOnly.MinValue;
    public DateOnly MaxDate { get; set; } = DateOnly.MaxValue;

    public FilmOrder OrderBy { get; set; } = FilmOrder.None;

    public bool IsDescending { get; set; } = false;
}
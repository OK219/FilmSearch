using FilmSearchApi.DB;

namespace FilmSearchApi.Models;

public class FilmListViewModel
{
    public PagedResult<FilmListItemDto> Results { get; set; } = new();
    public FilmFilterAndOrderSettings Filter { get; set; } = new();
}
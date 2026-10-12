using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace FilmSearchApi.DB;

public static class QueryableExtension
{
    public static async Task<List<T>> GetPage<T>(this IQueryable<T> query, int page = 1, int pageSize = 50)
    {
        return await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }
}
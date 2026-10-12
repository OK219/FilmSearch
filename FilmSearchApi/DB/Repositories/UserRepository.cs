using System;
using System.Linq;
using System.Threading.Tasks;
using FilmSearchApi.DB.Models;
using Microsoft.EntityFrameworkCore;

namespace FilmSearchApi.DB.Repositories;

public class UserRepository(FilmDbContext dbContext)
{
    public async Task<Guid> RegisterUser(string email, string passwordHash)
    {
        var user = new User { Email = email, PasswordHash = passwordHash };
        await dbContext.Users.AddAsync(user);
        await dbContext.SaveChangesAsync();
        return user.Id;
    }
    
    public async Task<User?> GetUser(string email)
    {
        return await dbContext.Users.FirstOrDefaultAsync(x => x.Email == email);
    }
    
    public async Task<User?> GetUser(Guid id)
    {
        return await dbContext.Users.FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task DeleteUser(Guid id)
    {
        await dbContext.Users.Where(x => x.Id == id).ExecuteDeleteAsync();
        await dbContext.SaveChangesAsync();
    }
}
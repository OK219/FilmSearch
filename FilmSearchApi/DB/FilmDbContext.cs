using FilmSearchApi.DB.Models;
using Microsoft.EntityFrameworkCore;

namespace FilmSearchApi.DB;

public class FilmDbContext(DbContextOptions<FilmDbContext> options) : DbContext(options)
{
    public DbSet<User> Users { get; set; }
    public DbSet<Genre> Genres { get; set; }
    public DbSet<Film> Films { get; set; }
    public DbSet<Review> Reviews { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(u => u.Email).IsUnique();
        });

        modelBuilder.Entity<Genre>(entity => entity.HasIndex(u => u.Name).IsUnique());

        modelBuilder.Entity<Film>()
            .HasOne(f => f.Genre)
            .WithMany(g => g.Films)
            .HasForeignKey(f => f.GenreId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Film>().HasIndex(f => f.Name);
        
        modelBuilder.Entity<Review>(entity =>
        {
            entity.HasKey(r => new { r.UserId, r.FilmId });

            entity.HasOne(r => r.User)
                .WithMany(u => u.Reviews)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(r => r.Film)
                .WithMany(f => f.Reviews)
                .HasForeignKey(r => r.FilmId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
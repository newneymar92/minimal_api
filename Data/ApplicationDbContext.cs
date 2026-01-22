using Microsoft.EntityFrameworkCore;
using MinimalApi.Models;

namespace MinimalApi.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(x => x.Username).IsUnique();
            entity.Property(x => x.Username).HasMaxLength(100).IsRequired();
            entity.Property(x => x.PasswordHash).IsRequired();
        });
    }
}


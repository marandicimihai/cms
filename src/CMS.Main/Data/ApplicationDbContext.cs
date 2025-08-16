using CMS.Main.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CMS.Main.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<Project> Projects { get; set; }
    public DbSet<Schema> Schemas { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<Project>()
            .HasMany(p => p.Schemas)
            .WithOne(s => s.Project);
        
        base.OnModelCreating(builder);
    }
}
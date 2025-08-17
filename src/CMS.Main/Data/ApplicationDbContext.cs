using CMS.Main.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CMS.Main.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<Project> Projects { get; set; }
    public DbSet<Schema> Schemas { get; set; }
    public DbSet<SchemaProperty> SchemaProperties { get; set; }
    public DbSet<Entry> Entries { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<Project>()
            .HasMany(p => p.Schemas)
            .WithOne(s => s.Project);

        builder.Entity<Schema>()
            .HasMany(s => s.Properties)
            .WithOne(p => p.Schema);

        builder.Entity<Entry>()
            .HasOne(s => s.Schema);

        base.OnModelCreating(builder);
    }
}
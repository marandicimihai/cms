using CMS.Main.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

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

        // In memory db doesn't have support for json columns
        var provider = Database.ProviderName;
        if (provider != null && !provider.Contains("Npgsql"))
        {
            builder.Entity<Entry>()
                .Property(e => e.Data)
                .HasConversion(
                    v => v.RootElement.GetRawText(),
                    v => (string.IsNullOrEmpty(v) ? null : JsonDocument.Parse(v, new JsonDocumentOptions()))!
                );
        }

        base.OnModelCreating(builder);
    }
}
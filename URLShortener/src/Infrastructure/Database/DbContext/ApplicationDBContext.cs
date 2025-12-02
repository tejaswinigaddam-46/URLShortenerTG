using Microsoft.EntityFrameworkCore;
using URLShortener.Infrastructure.Database.Entities;

namespace URLShortener.Infrastructure.Database.DbContext
{
    public class ApplicationDbContext : Microsoft.EntityFrameworkCore.DbContext
    {
        public ApplicationDbContext(Microsoft.EntityFrameworkCore.DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // DbSet represents the UrlMapping table in the database.
        public DbSet<UrlMappingDataModel> UrlMappings { get; set; }

        // Configure constraints and indexes here.
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Ensure the ShortCode is unique and indexed for fast lookup.
            modelBuilder.Entity<UrlMappingDataModel>()
                .HasIndex(u => u.ShortCode)
                .IsUnique();

            // Enforce ShortCode is case-sensitive for PostgreSQL
            modelBuilder.Entity<UrlMappingDataModel>()
                .Property(u => u.ShortCode)
                .UseCollation("C");
        }
    }
}
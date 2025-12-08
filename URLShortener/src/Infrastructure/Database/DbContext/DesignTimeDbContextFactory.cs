using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System;

namespace URLShortener.Infrastructure.Database.DbContext
{
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            var builder = new DbContextOptionsBuilder<ApplicationDbContext>();

            // 1. Try explicit connection string env var (used by .NET config binding convention)
            string? connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__PostgresConnection");

            // 2. If not present, build connection string from POSTGRES_* env vars (set by docker/.env)
            if (string.IsNullOrEmpty(connectionString))
            {
                var host = Environment.GetEnvironmentVariable("POSTGRES_HOST") ?? "localhost";
                var port = Environment.GetEnvironmentVariable("POSTGRES_PORT") ?? "5432";
                var db = Environment.GetEnvironmentVariable("POSTGRES_DB") ?? "urlshortener";
                var user = Environment.GetEnvironmentVariable("POSTGRES_USER") ?? "postgres";
                var password = Environment.GetEnvironmentVariable("POSTGRES_PASSWORD") ?? "postgres";

                connectionString = $"Host={host};Port={port};Database={db};Username={user};Password={password}";
            }

            builder.UseNpgsql(connectionString);

            return new ApplicationDbContext(builder.Options);
        }
    }
}

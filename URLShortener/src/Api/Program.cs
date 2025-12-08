using Microsoft.EntityFrameworkCore;
using URLShortener.Infrastructure.Database.DbContext;
using URLShortener.Application.Interfaces;
using URLShortener.Application.Services;
using URLShortener.Infrastructure.Database.Repositories;
using URLShortener.Infrastructure.Cache.CacheRepositories;

var builder = WebApplication.CreateBuilder(args);
builder.Logging.AddConsole();

// --- 1. Data Persistence (PostgreSQL / EF Core) Configuration ---

// Get the PostgreSQL connection string from environment variables/appsettings
var postgresConnectionString = builder.Configuration.GetConnectionString("PostgresConnection")
    ?? Environment.GetEnvironmentVariable("ConnectionStrings__PostgresConnection");

if (string.IsNullOrEmpty(postgresConnectionString))
{
    throw new InvalidOperationException("PostgresConnection string is not configured.");
}

// Add DbContext with Npgsql provider. 
// Note: It uses the 'db' service name as the host, as defined in docker-compose.yml.
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(postgresConnectionString));


// --- 2. Caching Integration (Redis) Configuration ---

// Get the Redis connection string from environment variables/appsettings
var redisConnectionString = builder.Configuration.GetConnectionString("RedisConnection")
    ?? Environment.GetEnvironmentVariable("ConnectionStrings__RedisConnection");

if (string.IsNullOrEmpty(redisConnectionString))
{
    throw new InvalidOperationException("RedisConnection string is not configured.");
}

// Add Distributed Cache using StackExchange.Redis provider
// Note: It uses the 'cache' service name as the host, as defined in docker-compose.yml.
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = redisConnectionString;
    options.InstanceName = builder.Configuration["Redis:InstanceName"] ?? Environment.GetEnvironmentVariable("REDIS_INSTANCE_NAME") ?? string.Empty;
});

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddRazorPages();
builder.Services.AddScoped<IURLShortenerService, URLShortenerService>();
builder.Services.AddScoped<IURLShortenerDBRepository, URLShortenerDBRepository>();
builder.Services.AddScoped<IURLShortenerCacheRepository, URLShortenerCacheRepository>();
builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

// --- 3. Implement EF Core Migrations for Automatic Database Setup ---
// This block ensures the database is created and migrations are applied on startup.
try
{
    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        // Apply pending migrations automatically
        dbContext.Database.Migrate(); 
        // This is crucial for seamless container startup in Docker Compose
    }
}
catch (Exception ex)
{
    // Log the error if migration fails (e.g., if the DB container isn't ready)
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogError(ex, "An error occurred while migrating the database.");
    // Depending on your deployment strategy, you might want to stop the app here
}


// Configure the HTTP request pipeline.
if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("SECRET_SALT")))
{
    throw new InvalidOperationException("Secret salt string is not configured.");
}

// Removed HTTPS redirection to avoid http→https issues in container
app.UseAuthorization();
// Hello World endpoint
app.MapGet("/", () => "Hello World! URL Shortener API is running.");

app.MapControllers();
app.MapRazorPages();

app.Run();

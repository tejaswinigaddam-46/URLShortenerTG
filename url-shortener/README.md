# URL Shortener — Project Layout 

This directory contains a prepared folder structure for a monolithic 3-tier URL shortener using C#, Redis, Postgres, and Docker.

Overview
- Architecture: 3-tier monolithic
  1) Presentation / API layer (ASP.NET Core) — web API + small Razor Pages UI
  2) Cache layer — Redis cluster (cache-first reads)
  3) Persistence layer — PostgreSQL

Behavior (high-level)
- API receives incoming requests (e.g., resolve short URL / create short URL).
- On read/resolve: API checks Redis first; if key exists it returns the cached value.
- If not in Redis, API reads from PostgreSQL, writes the result into Redis, and returns it.
- If not found in DB, API returns 404.

Tech stack
- C# / .NET (API + small Razor Pages UI)
- Redis (cache)
- PostgreSQL (persistence)
- Docker + docker-compose for local development

Project structure (placeholders only)

url-shortener/
├── src/
│   ├── Api/                # Presentation Layer (API + Razor Pages UI)
│   │   ├── Controllers/
│   │   ├── Pages/
│   │   ├── wwwroot/        # CSS, JS, static files
│   │   ├── Models/         # DTOs
│   │   ├── Middleware/
│   │   ├── Validators/
│   │   ├── Extensions/
│   │   ├── Config/
│   │   └── Program.cs (placeholder)
│   |
│   ├── Application/       # Business logic
│   │   ├── Interfaces/
│   │   ├── Services/
│   │   ├── Models/
│   │   ├── Exceptions/
│   │   └── Utils/
│   |
│   ├── Infrastructure/    # Persistence + Cache
│   │   ├── Database/
│   │   │   ├── Entities/
│   │   │   ├── Repositories/
│   │   │   ├── Migrations/
│   │   │   └── DbContext/
│   │   ├── Cache/
│   │   │   ├── RedisClient/
│   │   │   ├── CacheRepositories/
│   │   │   └── Keys/
│   │   └── Logging/
│   |
│   └── Shared/
│       ├── Constants/
│       ├── Settings/
│       └── Helpers/
|
├── docker/
│   ├── api.Dockerfile
│   ├── redis/
│   ├── postgres/
│   └── docker-compose.yml
|
├── tests/
└── README.md

Notes
- These are placeholders and not runnable code. Use this layout to implement the API, application services, and infrastructure pieces.
- When implementing, follow a cache-first read pattern and ensure cache invalidation on writes as appropriate.


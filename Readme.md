URL Shortener service
What: 
URL Shortener service is a microservice that shortens long URLs into short codes.
Why:
URL Shortener service is useful when you want to share long URLs in a more concise way.
1) Easy to share in social media avoiding character limit (e.g., Twitter, SMS)
2) Track click statistics to understand audience engagement (e.g., number of clicks)
3) Link management (Change to new url)

Tech Concepts Covered:
1) Database
2) Caching
3) Unique Short Code Generation
4) Redirection mechanism
5) Scalability & DS

Tech Reasons:
1) Database:
    - DB decision always relies on type of data
    - Short url should always be unique (Atomicity) and map to a long url should be guaranteed and durable
    - Indexing can be done in 2 keys (short key and long key for retrieval)
    - Choosing Postgres sql over NO SQL DB because of the following reasons:
        - Postgres sql is a relational database that supports ACID transactions 
        - Need strong consistency over eventual consitency (ACID>>BASE)
        - Postgres sql is a well-established SQL database that has a large community and a lot of resources available

2) Caching:
    - URL shortener is a read intensive application
    - Caching can be done on short url to improve performance
    - Choosing Redis because of the following reasons:
        - Redis is a in-memory database and key-value store supports Fast read operations

3) Unique Short Code Generation: (update base62+Twitter snowflake: easy scalable and reduces db write+update)
    - Short code should be unique
    - (BEST**) Twitter snowflake can be used to generate unique short codes encoded 64 bit code (41 bits : timestamp, 10 bits: Worker id, 12: sequence number (rotates))
        - Avoid security concern of guessing short codes as there is no sequence
        - Db gets accessed only for write (to insert new url)
        - Cons: Not fixed url variable length(7-11 chars)
        - Snowflake is Timeordered (New Ids always increasing) causes Db(Write contention: always hits right Index of Btree in postgres) similarly for cache recently loaded are more used
    - Choosing Primary Key of Db and multiplying with fixed number and xor using Secret key 
        - No collision check required (as we are considering Primary key ) 
        - Multiplying with fixed number and xor using Secret key makes it not too easy to guess shortcodes even there is sequence of Primary keys of DB
        - Cons:
        - Unable to scale with sharing(because PKs repeat on shards) or by replication (after replication PK generator should be single bottleneck service)
        - Db will get accessed twice for inserting url
4) Redirection mechanism:
    - When user hits short url, it should redirect to long url 
    - 302 : Found (Temporary Redirect) 
        -  Browser does not cache the redirect response. Every request hit server and can track click statistics
    - 301 : Moved Permanently
        - Browser caches the redirect response. Subsequent requests to the same short URL will be redirected without hitting the server. Cannot track stats
    - 307 : Temporary Redirect
        - Similar to 302 but it ensure Initial method of shortcode (GET/POST) is not changed during redirection
    - Choosing default 302 Since its only GET request
    - 404 : Not Found
        - when Short url/Long url is not found in db
5) Scalability & DS: 
    - Choosing Monolithic Layered architecture over microservices because of the following reasons:
        - URL Shortener service domain is simple (URL shortening) and the overhead of managing a distributed microservice environment is unwarranted for a small, single-domain application
        - Distributed components:
            - Database: Postgres sql
            - Caching: Redis
            - Application: C# .Net Core
        - Layered architecture:
            - Presentation Layer: Handles HTTP requests, responses and business logic
            - Cache Layer: helps in caching operations for faster reads.
            - Persistence Layer: responsible for database/storage interaction.
        - Achieves Distributed Scalability by separating concerns across multiple distributed components (Database, Caching, Application)
    - Layered Architecure in detail:
        - Presentation layer receives request and checks in redis cache
            a) cache hit: returns long url from cache
            b) cache miss: proceeds to Presentation layer and checks in db 
                i) db hit: caches it in redis and returns response with redirect
                ii) db miss: returns 404 not found
    - Scalability (After deployment):
        - Presentation layer:
            - Horizontal scaling:
                - Add more instances of the application to handle increased load
                - No user data so simply a load balancer can distribute traffic among instances
        - Cache & Persistence Layer:
            - Writes: Sharding based on (alphabets of short code/long code or time of creation of urls, etc)
            - Read replicas can be used to scale read operations

Capacity planning:
For one instance
1) Avg C# server capacity: 10K RPS
2) Redis cache capacity: 100K RPS 
3) Redis db capacity: 20-100 GB
4) Postgres Volume capacity: 1-5 TB
5) Postgres write capacity: 1k TPS

a) No of users to use the system at once : 
    10k Reads per second (C#)
    1k Writes per second (DB)

b) Max no of urls db can store : 
    Postgres : 5TB
    Unit storage: 
       id : 8 bytes
       short code : 10 bytes
       long url : 100 bytes
       metadata: 150 bytes
       Total: 268 bytes per url
    No of urls : 5TB/268 bytes = 1.88 * 10^10 urls ~ 19 bilion urls

Implementation:
Tech Stack:
    - C# .Net Core
    - Postgres sql
    - Redis
    - Docker containers of all above components

Project structure: 
    - API folder: Handles API requests to and fro from user to application
    - Application folder: Contains business logic and uses interfaces to interact with persistence layer and cache layer
    - Infrastructure folder:
        - Cache folder: Implements cache layer interfaces and interacts with Redis
        - Database folder: Handles database operations and implements data access layer interfaces
    ** Since Application folder is not directly interacting with Postgres db or redis cache, but with interfaces implemented. This adds flexibility of choosing any db and any cache irrespective of changing core/business logic of applicaiton
       

Environment Setup
- Required environment variables:
  - `MACHINE_ID`: A unique identifier for the machine running the service (for Snowflake ID generation).
  - `DATACENTER_ID`: A unique identifier for the datacenter where the service is running (for Snowflake ID generation).
  - `ConnectionStrings:PostgresConnection`: PostgreSQL connection string.
  - `ConnectionStrings:RedisConnection`: Redis connection string.
- Example `.env` for Docker Compose:
  - `ASPNETCORE_URLS=http://+:8080`
  - `POSTGRES_HOST=db`
  - `POSTGRES_PORT=5432`
  - `POSTGRES_DB=DBName`
  - `POSTGRES_USER=Username`
  - `POSTGRES_PASSWORD=Password`
  - `REDIS_HOST=cache`
  - `REDIS_PORT=6379`
  - `MACHINE_ID=1`
  - `DATACENTER_ID=1`
- Example connection strings (if not using `.env` binding):
  - `ConnectionStrings:PostgresConnection=Host=db;Port=5432;Database=DBName;Username=postgres;Password=Password`
  - `ConnectionStrings:RedisConnection=cache:6379,allowAdmin=true`

Frontend Pages
- Home Page
  - Path: `/HomePage`
  - Purpose: Enter a long URL and generate a short code.
  - Validation: Requires absolute `http` or `https` URL, length ≤ 2048.
  - Output: Displays the generated short URL as a link to `/r/{shortCode}` and a navigation link to Stats.
  - Access:
    - Docker: `http://localhost:5001/HomePage`
    - Local: `http://localhost:8080/HomePage`
- Stats Page
  - Path: `/StatsPage`
  - Purpose: Enter a short code to view click statistics and original long URL.
  - Validation: Requires base62 alphanumeric short code.
  - Output: Shows `LongUrl` and total `Clicks`.
  - Access:
    - Docker: `http://localhost:5001/StatsPage`
    - Local: `http://localhost:8080/StatsPage` 

API Reference
- Create short code
  - `POST /api/urls`
  - Body: `{ "OriginalUrl": "https://example.com/very/long/path" }`
  - Responses:
    - `201 Created` `{ "shortCode": "Ab3Xy12", "originalUrl": "https://example.com/very/long/path" }`
    - `400 Bad Request` on invalid URL
    - `500 Internal Server Error` on unexpected errors
- Redirect by short code
  - `GET /r/{shortCode}`
  - Response: `302 Found` redirect to the original URL
  - Errors: `404 Not Found` if short code does not exist; `400 Bad Request` for invalid format
- Click statistics
  - `GET /api/urls/{shortCode}/stats`
  - Response: `200 OK` `{ "shortCode": "Ab3Xy12", "clicks": 42 }`
  - Errors: `404 Not Found` if short code does not exist; `400 Bad Request` for invalid format

Data Model Details
- Table: `UrlMappings`
  - `SnowflakeId`: `BIGINT`, primary key (generated by Snowflake algorithm)
  - `ShortCode`: `VARCHAR(11)` unique, case-sensitive (PostgreSQL collation `C`)
  - `LongUrl`: `VARCHAR(2048)` not null
  - `Clicks`: `INTEGER` not null (default `0`)
  - `CreatedDate`: `TIMESTAMP WITH TIME ZONE` not null (UTC)
  - `ExpirationDate`: `TIMESTAMP WITH TIME ZONE` nullable
- Constraints:
  - Unique index on `ShortCode`
  - Case-sensitive comparison on `ShortCode` via collation `C`

Run Commands
- Local (without Docker):
  - Prerequisites: .NET SDK installed
  - Set environment variables (PowerShell example):
    - `$env:SECRET_SALT="1234567890123"`
    - `$env:ConnectionStrings__PostgresConnection="Host=localhost;Port=5432;Database=URLShortener;Username=postgres;Password=postgres"`
    - `$env:ConnectionStrings__RedisConnection="localhost:6379,allowAdmin=true"`
  - Build: `dotnet build URLShortener/src/Api/Api.csproj`
  - Run: `dotnet run --project URLShortener/src/Api/Api.csproj`
  - App listens on `http://localhost:8080` if `ASPNETCORE_URLS` is set accordingly; otherwise default Kestrel port.
- Docker Compose:
  - Navigate to `URLShortener/docker`
  - Ensure `.env` file includes variables from Environment Setup
  - Start: `docker compose up -d --build`
  - API available at `http://localhost:5001` (mapped to container `8080`)
  - PostgreSQL at `localhost:5432`, Redis at `localhost:6379`
  - Stop: `docker compose down`


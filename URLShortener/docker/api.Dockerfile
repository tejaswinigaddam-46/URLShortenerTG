# docker/api.Dockerfile

# Stage 1: Build the application
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy the solution file and restore dependencies
COPY ["URLShortener.sln", "./"]
COPY ["URLShortener/src/Api/Api.csproj", "URLShortener/src/Api/"]
COPY ["URLShortener/src/Application/Application.csproj", "URLShortener/src/Application/"]
COPY ["URLShortener/src/Infrastructure/Infrastructure.csproj", "URLShortener/src/Infrastructure/"]
COPY ["URLShortener/src/Shared/Shared.csproj", "URLShortener/src/Shared/"]

RUN dotnet restore "URLShortener/src/Api/Api.csproj"

# Copy all source code and build
COPY URLShortener/src/. URLShortener/src/.
WORKDIR /src/URLShortener/src/Api
RUN dotnet build "Api.csproj" -c Release -o /app/build

# Stage 2: Publish the application
FROM build AS publish
RUN dotnet publish "Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 3: Create the final runtime image
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Api.dll"]
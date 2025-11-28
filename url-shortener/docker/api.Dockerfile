# docker/api.Dockerfile

# Stage 1: Build the application
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy the solution file and restore dependencies
COPY ["URLShortener.sln", "./"]
COPY ["url-shortener/src/Api/Api.csproj", "url-shortener/src/Api/"]
COPY ["url-shortener/src/Application/Application.csproj", "url-shortener/src/Application/"]
COPY ["url-shortener/src/Infrastructure/Infrastructure.csproj", "url-shortener/src/Infrastructure/"]
COPY ["url-shortener/src/Shared/Shared.csproj", "url-shortener/src/Shared/"]

RUN dotnet restore "url-shortener/src/Api/Api.csproj"

# Copy all source code and build
COPY url-shortener/src/. url-shortener/src/.
WORKDIR /src/url-shortener/src/Api
RUN dotnet build "Api.csproj" -c Release -o /app/build

# Stage 2: Publish the application
FROM build AS publish
RUN dotnet publish "Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 3: Create the final runtime image
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Api.dll"]
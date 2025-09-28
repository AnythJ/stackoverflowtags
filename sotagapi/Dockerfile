# Stage 1: Build the application
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy the API project file and restore dependencies
# This caches the dependencies, speeding up subsequent builds
COPY ["sotagapi.csproj", "."]
RUN dotnet restore

# Copy the rest of the application code
COPY . .

# Publish the application
RUN dotnet publish -c Release -o /app/publish

# Stage 2: Create the final runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Copy the published output from the build stage
COPY --from=build /app/publish .

# The SQLite database path is important here. 
# We'll use a Docker Volume to persist it.
# The path in the connection string is: Data Source=/app/tags.db
# The volume will be mounted to the container's /app directory.
# This assumes your startup project is named 'sotagapi' and its port is 8080 (default for ASP.NET Core 8.0).
ENTRYPOINT ["dotnet", "sotagapi.dll"]
# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy solution and project files first for better layer caching
COPY AttendancePlease.slnx .
COPY global.json .
COPY Api/Api.csproj Api/
COPY Application/Application.csproj Application/
COPY Domain/Domain.csproj Domain/
COPY Infrastructure/Infrastructure.csproj Infrastructure/

# Restore NuGet packages
RUN dotnet restore Api/Api.csproj

# Copy the rest of the source code
COPY Api/ Api/
COPY Application/ Application/
COPY Domain/ Domain/
COPY Infrastructure/ Infrastructure/

# Publish the application
RUN dotnet publish Api/Api.csproj \
    -c Release \
    -o /app/publish \
    --no-restore

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# Install SQLite (needed for the SQLite database provider)
RUN apt-get update && \
    apt-get install -y --no-install-recommends sqlite3 && \
    rm -rf /var/lib/apt/lists/*

EXPOSE 8080

COPY --from=build /app/publish .

# Create a directory for the SQLite database with appropriate permissions
RUN mkdir -p /data && chmod 777 /data
ENV ConnectionStrings__DefaultConnection="Data Source=/data/attendance.db"

ENTRYPOINT ["dotnet", "Api.dll"]

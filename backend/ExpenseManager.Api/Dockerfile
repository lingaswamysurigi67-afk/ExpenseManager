# ---- Build stage ----
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Restore first (better layer caching)
COPY ExpenseManager.Api.csproj ./
RUN dotnet restore

# Build & publish
COPY . ./
RUN dotnet publish -c Release -o /app /p:UseAppHost=false

# ---- Runtime stage ----
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /app ./

ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 8080

# Render (and most PaaS) inject a PORT env var; fall back to 8080 locally.
ENTRYPOINT ["sh", "-c", "ASPNETCORE_URLS=http://+:${PORT:-8080} dotnet ExpenseManager.Api.dll"]

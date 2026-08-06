# ----------------------------
# Build stage
# ----------------------------
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /src

# Copy project files first so Docker can cache package restore
COPY ["Ticketing.API/Ticketing.API.csproj", "Ticketing.API/"]
COPY ["Ticketing.Application/Ticketing.Application.csproj", "Ticketing.Application/"]
COPY ["Ticketing.Domain/Ticketing.Domain.csproj", "Ticketing.Domain/"]
COPY ["Ticketing.Infrastructure/Ticketing.Infrastructure.csproj", "Ticketing.Infrastructure/"]

# Restore the startup project and its referenced projects
RUN dotnet restore "Ticketing.API/Ticketing.API.csproj"

# Copy the remaining source code
COPY . .

# Publish the API
WORKDIR "/src/Ticketing.API"

RUN dotnet publish "Ticketing.API.csproj" \
    --configuration Release \
    --output /app/publish \
    --no-restore

# ----------------------------
# Runtime stage
# ----------------------------
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime

WORKDIR /app

COPY --from=build /app/publish .

# Render expects the server to bind publicly
ENV ASPNETCORE_URLS=http://0.0.0.0:10000
ENV ASPNETCORE_ENVIRONMENT=Production

EXPOSE 10000

ENTRYPOINT ["dotnet", "Ticketing.API.dll"]
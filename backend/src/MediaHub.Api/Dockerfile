# ============================================================
# Stage 1 : BUILD - on compile avec le SDK
# ============================================================
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copie les .csproj d'abord (optimisation cache Docker)
COPY ["src/MediaHub.Api/MediaHub.Api.csproj", "src/MediaHub.Api/"]
COPY ["src/MediaHub.Application/MediaHub.Application.csproj", "src/MediaHub.Application/"]
COPY ["src/MediaHub.Infrastructure/MediaHub.Infrastructure.csproj", "src/MediaHub.Infrastructure/"]
COPY ["src/MediaHub.Domain/MediaHub.Domain.csproj", "src/MediaHub.Domain/"]
COPY ["Directory.Build.props", "./"]

# Restore en cache (ne re-run que si les .csproj changent)
RUN dotnet restore "src/MediaHub.Api/MediaHub.Api.csproj"

# Copie tout le reste
COPY . .

# Build + publish
WORKDIR "/src/src/MediaHub.Api"
RUN dotnet publish "MediaHub.Api.csproj" \
    -c Release \
    -o /app/publish \
    /p:UseAppHost=false \
    /p:TreatWarningsAsErrors=false

# ============================================================
# Stage 2 : RUNTIME - image légère sans le SDK
# ============================================================
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

# Crée un user non-root (bonne pratique sécurité)
RUN groupadd -r mediahub && useradd -r -g mediahub mediahub

COPY --from=build --chown=mediahub:mediahub /app/publish .

USER mediahub

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "MediaHub.Api.dll"]
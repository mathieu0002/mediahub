<<<<<<< Updated upstream
# MediaHub API
[![CI](https://github.com/mathieu0002/mediahub/actions/workflows/ci.yml/badge.svg)](https://github.com/mathieu0002/mediahub/actions/workflows/ci.yml)
🌐 **Démo en ligne** : [Swagger](https://mediahub-api.onrender.com/swagger)

> ⚠️ L'API est hébergée sur le free tier Render et s'endort après 15 min d'inactivité. La première requête après une pause peut prendre 30-60s pour démarrer (cold start), puis l'API répond normalement.

Backend .NET 9 pour le suivi multi-média (anime, manga, films, séries).

## Architecture

Clean Architecture en 4 couches :
- **Domain** : entités, enums, value objects (aucune dépendance)
- **Application** : services, DTOs, interfaces (logique métier)
- **Infrastructure** : EF Core, providers HTTP, Redis (détails techniques)
- **Api** : controllers, middleware, configuration DI

## Stack

- .NET 9, ASP.NET Core
- PostgreSQL 16 + EF Core 9
- Redis 7 (cache distribué)
- AniList GraphQL + TMDB REST (sources de données)

## Démarrage local

```bash
docker compose up -d
dotnet run --project src/MediaHub.Api
```

API disponible sur `https://localhost:5001`, Swagger sur `/swagger`.
=======
# MediaHub API

Backend .NET 9 pour le suivi multi-média (anime, manga, films, séries).

## Architecture

Clean Architecture en 4 couches :
- **Domain** : entités, enums, value objects (aucune dépendance)
- **Application** : services, DTOs, interfaces (logique métier)
- **Infrastructure** : EF Core, providers HTTP, Redis (détails techniques)
- **Api** : controllers, middleware, configuration DI

## Stack

- .NET 9, ASP.NET Core
- PostgreSQL 16 + EF Core 9
- Redis 7 (cache distribué)
- AniList GraphQL + TMDB REST (sources de données)

## Démarrage local

```bash
docker compose up -d
dotnet run --project src/MediaHub.Api
```

API disponible sur `https://localhost:5001`, Swagger sur `/swagger`.
>>>>>>> Stashed changes

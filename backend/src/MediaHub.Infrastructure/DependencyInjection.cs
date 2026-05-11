using MediaHub.Application.Interfaces.Caching;
using MediaHub.Application.Interfaces.Persistence;
using MediaHub.Application.Interfaces.Providers;
using MediaHub.Infrastructure.Caching;
using MediaHub.Infrastructure.Persistence;
using MediaHub.Infrastructure.Providers.AniList;
using MediaHub.Infrastructure.Providers.Tmdb;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http.Resilience;
using System.Net.Http.Headers;

namespace MediaHub.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services, IConfiguration config)
    {
        // === Postgres ===
        var connString = config.GetConnectionString("Postgres")
            ?? throw new InvalidOperationException("ConnectionStrings:Postgres manquant");

        services.AddDbContext<AppDbContext>(opt =>
            opt.UseNpgsql(connString, npg =>
                npg.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName)));

        services.AddScoped<IAppDbContext>(sp => sp.GetRequiredService<AppDbContext>());

        // === Redis ===
        services.AddStackExchangeRedisCache(opt =>
        {
            opt.Configuration = config.GetConnectionString("Redis") ?? "localhost:6379";
            opt.InstanceName = "mediahub:";
        });
        services.AddSingleton<ICacheService, RedisCacheService>();

        // === Providers HTTP avec résilience ===
        services.AddHttpClient<AniListProvider>(c =>
        {
            c.BaseAddress = new Uri("https://graphql.anilist.co");
            c.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        })
        .AddStandardResilienceHandler();

        services.AddScoped<IMediaProvider, AniListProvider>();

        // TMDB
        services.Configure<TmdbOptions>(config.GetSection(TmdbOptions.SectionName));

        services.AddHttpClient<TmdbProvider>((sp, c) =>
        {
            var opt = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<TmdbOptions>>().Value;
            c.BaseAddress = new Uri(opt.BaseUrl.TrimEnd('/') + "/");
            c.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", opt.ApiKey);
            c.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        })
        .AddStandardResilienceHandler();

        services.AddScoped<IMediaProvider, TmdbProvider>();

        return services;
    }
}
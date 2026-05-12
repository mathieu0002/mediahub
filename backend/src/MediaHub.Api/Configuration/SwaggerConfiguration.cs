using Microsoft.OpenApi.Models;

namespace MediaHub.Api.Configuration;

public static class SwaggerConfiguration
{
    public static IServiceCollection AddSwaggerDocumentation(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "MediaHub API",
                Version = "v1",
                Description = "API pour le suivi multi-média (anime, manga, films, séries)",
                Contact = new OpenApiContact
                {
                    Name = "Mathieu",
                    Url = new Uri("https://github.com/mathieu0002")
                }
            });

            c.EnableAnnotations();
        });

        return services;
    }
}
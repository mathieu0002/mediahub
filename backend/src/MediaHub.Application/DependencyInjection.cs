using FluentValidation;
using MediaHub.Application.Interfaces.Services;
using MediaHub.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace MediaHub.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IMediaSearchService, MediaSearchService>();
        services.AddScoped<IUserMediaService, UserMediaService>();

        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

        return services;
    }
}
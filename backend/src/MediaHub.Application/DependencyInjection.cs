using FluentValidation;
using MediaHub.Application.Interfaces.Services;
using MediaHub.Application.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace MediaHub.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration config)
    {
        services.Configure<JwtOptions>(config.GetSection(JwtOptions.SectionName));

        services.AddScoped<IMediaSearchService, MediaSearchService>();
        services.AddScoped<IUserMediaService, UserMediaService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ICalendarService, CalendarService>();

        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

        return services;
    }
}
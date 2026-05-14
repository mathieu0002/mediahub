using MediaHub.Api.Middleware;
using MediaHub.Api.Configuration;
using MediaHub.Application;
using MediaHub.Application.Interfaces.Persistence;
using MediaHub.Domain.Entities;
using MediaHub.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Serilog;
using SharpGrip.FluentValidation.AutoValidation.Mvc.Extensions;

var builder = WebApplication.CreateBuilder(args);

// === Serilog ===
builder.Host.UseSerilog((context, config) => config
    .ReadFrom.Configuration(context.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/mediahub-.log", rollingInterval: RollingInterval.Day));

// === Services ===
builder.Services.AddControllers();
builder.Services.AddFluentValidationAutoValidation();

builder.Services.AddApplication(builder.Configuration);
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddSwaggerDocumentation();

// === CORS pour le futur front Nuxt ===
builder.Services.AddCors(opt =>
{
    opt.AddPolicy("DevFront", policy => policy
        .WithOrigins("http://localhost:3000")
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials());
});

var app = builder.Build();

// === Pipeline ===
app.UseMiddleware<ExceptionHandlingMiddleware>();


app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "MediaHub API v1");
        c.RoutePrefix = "swagger";
    });

app.UseSerilogRequestLogging();
app.UseCors("DevFront");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Applique les migrations EF Core au démarrage (utile pour Docker)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<MediaHub.Infrastructure.Persistence.AppDbContext>();
    await db.Database.MigrateAsync();
}

app.Run();

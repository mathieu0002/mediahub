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

builder.Services.AddApplication();
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

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "MediaHub API v1");
        c.RoutePrefix = "swagger";
    });
}

app.UseSerilogRequestLogging();
app.UseCors("DevFront");
app.UseAuthorization();
app.MapControllers();

// === Seed minimal : user de test pour développer sans auth ===
await SeedDevUserAsync(app);

app.Run();

// Méthode locale pour seed un user en dev
static async Task SeedDevUserAsync(WebApplication app)
{
    if (!app.Environment.IsDevelopment()) return;

    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<IAppDbContext>();
    var dbContext = (DbContext)db;

    if (!await dbContext.Set<User>().AnyAsync())
    {
        dbContext.Set<User>().Add(new User
        {
            Id = 1,
            Email = "dev@mediahub.local",
            Username = "dev",
            PasswordHash = "dev-no-auth-yet",
            CreatedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();
    }
}
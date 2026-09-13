using Microsoft.EntityFrameworkCore;
using SubastaYa.Application.Common.Interfaces;
using SubastaYa.Infrastructure.Persistence;

namespace SubastaYa.API.Extensions;

public static class DatabaseExtensions
{
    // Aplica las migraciones pendientes y carga los datos de prueba al arrancar. Solo en
    // Development: en producción las migraciones se aplican como un paso aparte del deploy.
    public static async Task PrepareDatabaseAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<SubastaYaDbContext>();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();

        await context.Database.MigrateAsync();
        await DbSeeder.SeedAsync(context, passwordHasher);
    }
}

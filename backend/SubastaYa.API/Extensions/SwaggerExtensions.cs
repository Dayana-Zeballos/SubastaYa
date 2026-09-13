using Microsoft.OpenApi.Models;

namespace SubastaYa.API.Extensions;

public static class SwaggerExtensions
{
    // El botón Authorize de Swagger es lo que permite demostrar la autenticación sin
    // herramientas externas: se pega el token del login y se prueban los endpoints.
    public static IServiceCollection AddSwaggerWithJwt(this IServiceCollection services)
    {
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "SubastaYa API",
                Version = "v1",
                Description = "Plataforma de subastas con billetera en escrow, anti-sniping y cierre automático."
            });

            var scheme = new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Pegá acá el token que devuelve POST /api/auth/login (solo el token, sin la palabra Bearer).",
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            };

            options.AddSecurityDefinition("Bearer", scheme);
            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                [scheme] = Array.Empty<string>()
            });
        });

        return services;
    }
}

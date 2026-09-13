using Microsoft.Extensions.DependencyInjection;
using SubastaYa.Application.Features.Auth;

namespace SubastaYa.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();

        return services;
    }
}

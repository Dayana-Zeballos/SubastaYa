using Microsoft.Extensions.DependencyInjection;
using SubastaYa.Application.Features.Auctions;
using SubastaYa.Application.Features.Auth;
using SubastaYa.Application.Features.Categories;

namespace SubastaYa.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<IAuctionQueryService, AuctionQueryService>();
        services.AddScoped<IAuctionCommandService, AuctionCommandService>();

        return services;
    }
}

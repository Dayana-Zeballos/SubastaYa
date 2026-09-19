using Microsoft.Extensions.DependencyInjection;
using SubastaYa.Application.Features.Auctions;
using SubastaYa.Application.Features.Audit;
using SubastaYa.Application.Features.Auth;
using SubastaYa.Application.Features.Bidding;
using SubastaYa.Application.Features.Categories;
using SubastaYa.Application.Features.Wallets;

namespace SubastaYa.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<IAuctionQueryService, AuctionQueryService>();
        services.AddScoped<IAuctionCommandService, AuctionCommandService>();
        services.AddScoped<IAuctionClosureService, AuctionClosureService>();
        services.AddScoped<IWalletService, WalletService>();
        services.AddScoped<IBiddingService, BiddingService>();
        services.AddScoped<IAuditService, AuditService>();

        return services;
    }
}

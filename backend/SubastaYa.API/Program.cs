using SubastaYa.API.Extensions;
using SubastaYa.API.Hubs;
using SubastaYa.API.Middleware;
using SubastaYa.API.Services;
using SubastaYa.Application;
using SubastaYa.Application.Common.Interfaces;
using SubastaYa.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerWithJwt();

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddSingleton<IAuctionNotifier, SignalRAuctionNotifier>();
builder.Services.AddSignalR();

builder.Services.AddCors(options =>
{
    // Localhost de cualquier puerto: cubre tanto wwwroot como Vite/React cuando lo decidamos.
    options.AddPolicy("Frontend", policy => policy
        .SetIsOriginAllowed(origin =>
            Uri.TryCreate(origin, UriKind.Absolute, out var uri)
            && (uri.Host == "localhost" || uri.Host == "127.0.0.1"))
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials());
});

builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddHostedService<SubastaYa.API.Workers.AuctionClosureWorker>();

var app = builder.Build();

// Va primero para que cualquier excepción del resto del pipeline caiga acá.
app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.MapGet("/", () => Results.Redirect("/swagger"))
        .ExcludeFromDescription();

    await app.PrepareDatabaseAsync();
}

app.UseHttpsRedirection();
app.UseCors("Frontend");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHub<AuctionHub>("/hubs/auctions");

app.Run();

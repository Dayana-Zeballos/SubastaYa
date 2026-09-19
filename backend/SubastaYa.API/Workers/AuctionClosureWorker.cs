using SubastaYa.Application.Features.Auctions;

namespace SubastaYa.API.Workers;

// Ciclo corto: el worker no toca DbContext; delega en IAuctionClosureService (decisión 12).
public class AuctionClosureWorker : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromSeconds(10);

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<AuctionClosureWorker> _logger;

    public AuctionClosureWorker(
        IServiceScopeFactory scopeFactory,
        ILogger<AuctionClosureWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("AuctionClosureWorker iniciado. Intervalo={Interval}s", Interval.TotalSeconds);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var closure = scope.ServiceProvider.GetRequiredService<IAuctionClosureService>();
                await closure.ProcessDueAuctionsAsync(stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "Error no controlado en el ciclo del AuctionClosureWorker");
            }

            try
            {
                await Task.Delay(Interval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
    }
}

using SubastaYa.Application.Common.Exceptions;
using SubastaYa.Application.Common.Interfaces;
using SubastaYa.Domain.Entities;

namespace SubastaYa.Application.Features.Auctions;

public class AuctionCommandService : IAuctionCommandService
{
    // Margen para que una fecha de inicio "ahora mismo" no se rechace por la diferencia
    // entre el reloj de quien publica y el del servidor.
    private const int StartToleranceMinutes = 2;
    private const int MinimumDurationMinutes = 5;
    private const int MaximumDurationDays = 30;
    private const int PriceDecimals = 2;

    private readonly IAuctionRepository _auctions;
    private readonly ICategoryRepository _categories;
    private readonly IAuctionQueryService _auctionQueryService;
    private readonly ICurrentUserService _currentUser;

    public AuctionCommandService(
        IAuctionRepository auctions,
        ICategoryRepository categories,
        IAuctionQueryService auctionQueryService,
        ICurrentUserService currentUser)
    {
        _auctions = auctions;
        _categories = categories;
        _auctionQueryService = auctionQueryService;
        _currentUser = currentUser;
    }

    public async Task<AuctionDetailResponse> CreateAsync(
        CreateAuctionRequest request,
        CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var startsAt = request.StartsAt.HasValue ? ToUtc(request.StartsAt.Value) : now;
        var endsAt = ToUtc(request.EndsAt);

        ValidateAmounts(request);
        ValidateDates(startsAt, endsAt, now);

        if (!await _categories.ExistsAsync(request.CategoryId, cancellationToken))
        {
            throw new BusinessRuleException("La categoría indicada no existe.");
        }

        var auction = new Auction
        {
            Title = request.Title.Trim(),
            Description = request.Description.Trim(),
            ImageUrl = NormalizeImageUrl(request.ImageUrl),
            CategoryId = request.CategoryId,
            // El vendedor sale del token: no se recibe por body para que nadie pueda publicar
            // a nombre de otro.
            SellerId = _currentUser.UserId,
            StartingPrice = request.StartingPrice,
            MinimumIncrement = request.MinimumIncrement,
            // Mientras no haya ofertas el precio vigente es el base, igual que en el seed.
            CurrentPrice = request.StartingPrice,
            StartsAt = startsAt,
            EndsAt = endsAt,
            // Si abre en el momento nace activa; si es a futuro queda como próxima.
            Status = startsAt <= now ? AuctionStatus.Active : AuctionStatus.Scheduled
        };

        await _auctions.AddAsync(auction, cancellationToken);

        // Devolvemos el detalle armado por el servicio de consulta para que el alta y el GET
        // compartan la misma representación. El vendedor está autenticado, así que resolvemos
        // IsCurrentUserWinning igual que en un GET con token.
        return await _auctionQueryService.GetByIdAsync(auction.Id, _currentUser.UserId, cancellationToken);
    }

    private static void ValidateAmounts(CreateAuctionRequest request)
    {
        if (request.StartingPrice <= 0)
        {
            throw new BusinessRuleException("El precio base tiene que ser mayor a cero.");
        }

        if (request.MinimumIncrement <= 0)
        {
            throw new BusinessRuleException("El incremento mínimo tiene que ser mayor a cero.");
        }

        // La base guarda los montos con dos decimales. Si aceptáramos más, el valor que
        // devolvemos no sería el que mandaron y la primera puja válida no cerraría.
        if (HasExtraDecimals(request.StartingPrice) || HasExtraDecimals(request.MinimumIncrement))
        {
            throw new BusinessRuleException($"Los montos admiten hasta {PriceDecimals} decimales.");
        }
    }

    private static void ValidateDates(DateTime startsAt, DateTime endsAt, DateTime now)
    {
        if (startsAt < now.AddMinutes(-StartToleranceMinutes))
        {
            throw new BusinessRuleException("La fecha de inicio no puede estar en el pasado.");
        }

        if (endsAt <= startsAt)
        {
            throw new BusinessRuleException("La fecha de cierre tiene que ser posterior a la de inicio.");
        }

        var duration = endsAt - startsAt;

        if (duration < TimeSpan.FromMinutes(MinimumDurationMinutes))
        {
            throw new BusinessRuleException($"La subasta tiene que durar al menos {MinimumDurationMinutes} minutos.");
        }

        if (duration > TimeSpan.FromDays(MaximumDurationDays))
        {
            throw new BusinessRuleException($"La subasta no puede durar más de {MaximumDurationDays} días.");
        }
    }

    private static bool HasExtraDecimals(decimal amount) => decimal.Round(amount, PriceDecimals) != amount;

    // Todo se guarda en UTC. Si la fecha viene sin zona horaria la tomamos como UTC, y si
    // trae desfasaje la convertimos, así una misma fecha no queda corrida según quién publique.
    private static DateTime ToUtc(DateTime value) => value.Kind switch
    {
        DateTimeKind.Utc => value,
        DateTimeKind.Local => value.ToUniversalTime(),
        _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
    };

    private static string NormalizeImageUrl(string? imageUrl)
    {
        if (string.IsNullOrWhiteSpace(imageUrl))
        {
            return string.Empty;
        }

        var trimmed = imageUrl.Trim();

        if (!Uri.TryCreate(trimmed, UriKind.Absolute, out var uri)
            || (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
        {
            throw new BusinessRuleException("La imagen tiene que ser una URL http o https.");
        }

        return trimmed;
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SubastaYa.Application.Common.Interfaces;
using SubastaYa.Application.Common.Models;
using SubastaYa.Application.Features.Auctions;
using SubastaYa.Application.Features.Bidding;

namespace SubastaYa.API.Controllers;

[ApiController]
[Route("api/auctions")]
public class AuctionsController : ControllerBase
{
    private readonly IAuctionQueryService _auctionQueryService;
    private readonly IAuctionCommandService _auctionCommandService;
    private readonly IBiddingService _biddingService;
    private readonly ICurrentUserService _currentUser;

    public AuctionsController(
        IAuctionQueryService auctionQueryService,
        IAuctionCommandService auctionCommandService,
        IBiddingService biddingService,
        ICurrentUserService currentUser)
    {
        _auctionQueryService = auctionQueryService;
        _auctionCommandService = auctionCommandService;
        _biddingService = biddingService;
        _currentUser = currentUser;
    }

    /// <remarks>
    /// Filtros disponibles: status (Active, Scheduled, Finished), categorySlug, minPrice,
    /// maxPrice y search. Orden: EndingSoon, HighestBid o LowestPrice.
    /// </remarks>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(PagedResult<AuctionListItemResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PagedResult<AuctionListItemResponse>>> Search(
        [FromQuery] AuctionQueryParameters parameters,
        CancellationToken cancellationToken)
    {
        return Ok(await _auctionQueryService.SearchAsync(parameters, cancellationToken));
    }

    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuctionDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AuctionDetailResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        // El detalle es público, pero si la petición trae token aprovechamos para resolver
        // en el backend si quien mira va liderando, en vez de que el frontend compare ids.
        Guid? currentUserId = User.Identity?.IsAuthenticated == true ? _currentUser.UserId : null;

        return Ok(await _auctionQueryService.GetByIdAsync(id, currentUserId, cancellationToken));
    }

    // Historial público: seudónimos, sin ids de usuario.
    [HttpGet("{id:guid}/bids")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IReadOnlyList<BidHistoryItemResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<BidHistoryItemResponse>>> GetBids(
        Guid id,
        CancellationToken cancellationToken)
    {
        return Ok(await _biddingService.GetBidsAsync(id, cancellationToken));
    }

    // El vendedor no viaja en el body: se resuelve desde el token.
    [HttpPost]
    [Authorize]
    [ProducesResponseType(typeof(AuctionDetailResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuctionDetailResponse>> Create(
        CreateAuctionRequest request,
        CancellationToken cancellationToken)
    {
        var auction = await _auctionCommandService.CreateAsync(request, cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id = auction.Id }, auction);
    }

    [HttpPost("{id:guid}/bids")]
    [Authorize]
    [ProducesResponseType(typeof(BidResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<BidResponse>> PlaceBid(
        Guid id,
        PlaceBidRequest request,
        CancellationToken cancellationToken)
    {
        var bid = await _biddingService.PlaceBidAsync(id, request, cancellationToken);

        return CreatedAtAction(nameof(GetBids), new { id }, bid);
    }
}

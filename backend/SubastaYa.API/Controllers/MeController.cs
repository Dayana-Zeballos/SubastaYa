using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SubastaYa.Application.Common.Interfaces;
using SubastaYa.Application.Common.Models;
using SubastaYa.Application.Features.Auctions;

namespace SubastaYa.API.Controllers;

[ApiController]
[Authorize]
[Route("api/me")]
public class MeController : ControllerBase
{
    private readonly IAuctionQueryService _auctions;
    private readonly ICurrentUserService _currentUser;

    public MeController(IAuctionQueryService auctions, ICurrentUserService currentUser)
    {
        _auctions = auctions;
        _currentUser = currentUser;
    }

    // Subastas que publicó el usuario del token.
    [HttpGet("auctions")]
    [ProducesResponseType(typeof(PagedResult<AuctionListItemResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<PagedResult<AuctionListItemResponse>>> GetMyAuctions(
        [FromQuery] MineQueryParameters parameters,
        CancellationToken cancellationToken)
    {
        return Ok(await _auctions.GetMineAsync(_currentUser.UserId, parameters, cancellationToken));
    }

    // Subastas que ganó (Finished y es el mejor postor).
    [HttpGet("purchases")]
    [ProducesResponseType(typeof(PagedResult<AuctionListItemResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<PagedResult<AuctionListItemResponse>>> GetMyPurchases(
        [FromQuery] MineQueryParameters parameters,
        CancellationToken cancellationToken)
    {
        return Ok(await _auctions.GetPurchasesAsync(_currentUser.UserId, parameters, cancellationToken));
    }

    // Subastas en las que pujó, abiertas o no. Sirve para "voy ganando / me superaron".
    [HttpGet("bids")]
    [ProducesResponseType(typeof(PagedResult<AuctionListItemResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<PagedResult<AuctionListItemResponse>>> GetMyBids(
        [FromQuery] MineQueryParameters parameters,
        CancellationToken cancellationToken)
    {
        return Ok(await _auctions.GetParticipatedAsync(_currentUser.UserId, parameters, cancellationToken));
    }
}

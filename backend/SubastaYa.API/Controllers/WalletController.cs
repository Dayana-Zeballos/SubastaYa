using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SubastaYa.Application.Features.Wallets;

namespace SubastaYa.API.Controllers;

[ApiController]
[Authorize]
[Route("api/wallet")]
public class WalletController : ControllerBase
{
    private readonly IWalletService _walletService;

    public WalletController(IWalletService walletService)
    {
        _walletService = walletService;
    }

    // Desglose Total / Retenido / Disponible del usuario del token.
    [HttpGet("balance")]
    [ProducesResponseType(typeof(WalletBalanceResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<WalletBalanceResponse>> GetBalance(CancellationToken cancellationToken)
    {
        return Ok(await _walletService.GetBalanceAsync(cancellationToken));
    }

    // El monto va en el body; el dueño de la billetera sale del JWT.
    [HttpPost("deposit")]
    [ProducesResponseType(typeof(WalletBalanceResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<WalletBalanceResponse>> Deposit(
        DepositRequest request,
        CancellationToken cancellationToken)
    {
        return Ok(await _walletService.DepositAsync(request, cancellationToken));
    }

    [HttpGet("transactions")]
    [ProducesResponseType(typeof(IReadOnlyList<WalletTransactionResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IReadOnlyList<WalletTransactionResponse>>> GetTransactions(
        CancellationToken cancellationToken)
    {
        return Ok(await _walletService.GetTransactionsAsync(cancellationToken));
    }
}

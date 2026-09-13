using System.ComponentModel.DataAnnotations;

namespace SubastaYa.Application.Features.Wallets;

public class DepositRequest
{
    // El usuario sale del token; acá solo viaja el monto.
    [Range(0.01, double.MaxValue, ErrorMessage = "El monto a depositar tiene que ser mayor a cero.")]
    public decimal Amount { get; set; }
}

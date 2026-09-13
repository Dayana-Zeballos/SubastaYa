using System.ComponentModel.DataAnnotations;

namespace SubastaYa.Application.Features.Bidding;

public class PlaceBidRequest
{
    // El postor sale del token; acá solo viaja el monto.
    [Range(0.01, double.MaxValue, ErrorMessage = "El monto de la puja tiene que ser mayor a cero.")]
    public decimal Amount { get; set; }
}

using System.ComponentModel.DataAnnotations;

namespace SubastaYa.Application.Features.Auctions;

public class CreateAuctionRequest
{
    [Required]
    [StringLength(200, MinimumLength = 5)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [StringLength(2000, MinimumLength = 20)]
    public string Description { get; set; } = string.Empty;

    [StringLength(512)]
    public string? ImageUrl { get; set; }

    [Required]
    public Guid CategoryId { get; set; }

    public decimal StartingPrice { get; set; }

    public decimal MinimumIncrement { get; set; }

    // Opcional: si no viene, la subasta abre en el mismo momento en que se publica.
    public DateTime? StartsAt { get; set; }

    [Required]
    public DateTime EndsAt { get; set; }
}

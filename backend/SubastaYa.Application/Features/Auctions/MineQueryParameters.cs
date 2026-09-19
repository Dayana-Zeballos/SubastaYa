namespace SubastaYa.Application.Features.Auctions;

// Paginación suelta, sin filtros de catálogo: "mis" listas son personales y cortas.
public class MineQueryParameters
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 12;
}

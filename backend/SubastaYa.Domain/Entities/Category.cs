namespace SubastaYa.Domain.Entities;

public class Category
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;

    /// <summary>Identificador legible para filtrar desde la URL (ej. "tecnologia").</summary>
    public string Slug { get; set; } = string.Empty;

    public ICollection<Auction> Auctions { get; set; } = new List<Auction>();
}

namespace SubastaYa.Application.Common.Exceptions;

// El recurso pedido no existe. El middleware la traduce a 404.
public class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message)
    {
    }

    public static NotFoundException For(string entityName, Guid id) =>
        new($"No se encontró {entityName} con id {id}.");
}

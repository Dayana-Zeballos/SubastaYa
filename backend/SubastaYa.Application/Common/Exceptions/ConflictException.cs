namespace SubastaYa.Application.Common.Exceptions;

// Conflicto de estado: la operación es válida en abstracto pero choca con el estado
// actual del recurso (pujar en una subasta ya cerrada, email ya registrado).
// El middleware la traduce a 409, igual que los conflictos de concurrencia.
public class ConflictException : Exception
{
    public ConflictException(string message) : base(message)
    {
    }
}

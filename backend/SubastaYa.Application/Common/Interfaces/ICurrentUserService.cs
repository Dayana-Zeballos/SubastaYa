namespace SubastaYa.Application.Common.Interfaces;

// Única puerta de entrada a la identidad del usuario autenticado. Ningún servicio ni
// endpoint recibe el id por parámetro: siempre sale del token.
public interface ICurrentUserService
{
    Guid UserId { get; }
}

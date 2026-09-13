using System.IdentityModel.Tokens.Jwt;
using SubastaYa.Application.Common.Interfaces;

namespace SubastaYa.API.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid UserId
    {
        get
        {
            var claim = _httpContextAccessor.HttpContext?.User
                .FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

            // Si esto explota es un bug nuestro: significa que un servicio pidió la
            // identidad desde un endpoint que no exige [Authorize].
            return Guid.TryParse(claim, out var userId)
                ? userId
                : throw new UnauthorizedAccessException("La petición no tiene un usuario autenticado.");
        }
    }
}

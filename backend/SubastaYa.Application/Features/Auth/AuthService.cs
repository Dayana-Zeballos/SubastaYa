using SubastaYa.Application.Common.Exceptions;
using SubastaYa.Application.Common.Interfaces;
using SubastaYa.Domain.Entities;

namespace SubastaYa.Application.Features.Auth;

public class AuthService : IAuthService
{
    private readonly IUserRepository _users;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _tokenGenerator;

    public AuthService(
        IUserRepository users,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator tokenGenerator)
    {
        _users = users;
        _passwordHasher = passwordHasher;
        _tokenGenerator = tokenGenerator;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var userName = request.UserName.Trim();

        if (await _users.EmailExistsAsync(email, cancellationToken))
        {
            throw new ConflictException("Ya existe una cuenta registrada con ese email.");
        }

        if (await _users.UserNameExistsAsync(userName, cancellationToken))
        {
            throw new ConflictException("Ese nombre de usuario ya está en uso.");
        }

        var user = new User
        {
            UserName = userName,
            Email = email,
            PasswordHash = _passwordHasher.Hash(request.Password)
        };

        // Todo usuario nace con billetera en cero: sin ella no podría pujar y el escrow
        // tendría que crearla sobre la marcha, en medio de la transacción de la puja.
        user.Wallet = new Wallet { UserId = user.Id };

        await _users.AddAsync(user, cancellationToken);

        return BuildResponse(user);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var user = await _users.FindByEmailAsync(email, cancellationToken);

        if (user is null || !_passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            throw new InvalidCredentialsException();
        }

        return BuildResponse(user);
    }

    private AuthResponse BuildResponse(User user)
    {
        var (token, expiresAt) = _tokenGenerator.Generate(user);

        return new AuthResponse
        {
            Token = token,
            ExpiresAt = expiresAt,
            UserId = user.Id,
            UserName = user.UserName,
            Email = user.Email
        };
    }
}

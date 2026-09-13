using Microsoft.EntityFrameworkCore;
using SubastaYa.Application.Common.Interfaces;
using SubastaYa.Domain.Entities;

namespace SubastaYa.Infrastructure.Persistence.Repositories;

public class UserRepository : IUserRepository
{
    private readonly SubastaYaDbContext _context;

    public UserRepository(SubastaYaDbContext context)
    {
        _context = context;
    }

    public Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default) =>
        _context.Users.AnyAsync(u => u.Email == email, cancellationToken);

    public Task<bool> UserNameExistsAsync(string userName, CancellationToken cancellationToken = default) =>
        _context.Users.AnyAsync(u => u.UserName == userName, cancellationToken);

    public Task<User?> FindByEmailAsync(string email, CancellationToken cancellationToken = default) =>
        _context.Users.FirstOrDefaultAsync(u => u.Email == email, cancellationToken);

    public async Task AddAsync(User user, CancellationToken cancellationToken = default)
    {
        // La billetera viaja en la navegación de User, así que ambos entran en el mismo
        // SaveChanges y por lo tanto en la misma transacción.
        _context.Users.Add(user);
        await _context.SaveChangesAsync(cancellationToken);
    }
}

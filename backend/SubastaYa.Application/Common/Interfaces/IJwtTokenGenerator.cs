using SubastaYa.Domain.Entities;

namespace SubastaYa.Application.Common.Interfaces;

public interface IJwtTokenGenerator
{
    (string Token, DateTime ExpiresAt) Generate(User user);
}

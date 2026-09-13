using SubastaYa.Domain.Entities;

namespace SubastaYa.Application.Common.Interfaces;

public interface ICategoryRepository
{
    Task<IReadOnlyList<Category>> GetAllAsync(CancellationToken cancellationToken = default);
}

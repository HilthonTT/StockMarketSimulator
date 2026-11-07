using Modules.Budgeting.Domain.Entities;
using SharedKernel;

namespace Modules.Budgeting.Domain.Repositories;

public interface IPositionRepository
{
    Task<Option<Position>> GetAsync(Guid userId, string ticker, CancellationToken cancellationToken = default);

    void Insert(Position position);
}

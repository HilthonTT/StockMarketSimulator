using Microsoft.EntityFrameworkCore;
using Modules.Budgeting.Domain.Entities;
using Modules.Budgeting.Domain.Repositories;
using Modules.Budgeting.Infrastructure.Database;
using SharedKernel;

namespace Modules.Budgeting.Infrastructure.Repositories;

internal sealed class PositionRepository(BudgetingDbContext context) : IPositionRepository
{
    public async Task<Option<Position>> GetAsync(Guid userId, string ticker, CancellationToken cancellationToken = default)
    {
        string normalizedTicker = Position.NormalizeTicker(ticker);

        Position? position = await context.Positions
            .FirstOrDefaultAsync(p => p.UserId == userId && p.Ticker == normalizedTicker, cancellationToken);

        return Option<Position>.Some(position);
    }

    public void Insert(Position position)
    {
        context.Positions.Add(position);
    }
}

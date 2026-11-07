using Application.Abstractions.Messaging;
using Modules.Budgeting.Contracts.Positions;

namespace Modules.Budgeting.Application.Positions.GetByUserId;

public sealed record GetPositionsByUserIdQuery(Guid UserId) : IQuery<List<PositionResponse>>;

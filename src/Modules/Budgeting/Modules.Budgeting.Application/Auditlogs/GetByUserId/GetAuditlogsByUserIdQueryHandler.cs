using Application.Abstractions.Authentication;
using Application.Abstractions.Messaging;
using Contracts.Common;
using Microsoft.EntityFrameworkCore;
using Modules.Budgeting.Application.Abstractions.Data;
using Modules.Budgeting.Contracts.AuditLogs;
using Modules.Budgeting.Domain.Entities;
using Modules.Budgeting.Domain.Enums;
using Modules.Budgeting.Domain.Errors;
using SharedKernel;

namespace Modules.Budgeting.Application.Auditlogs.GetByUserId;

internal sealed class GetAuditlogsByUserIdQueryHandler(
    IDbContext dbContext,
    IUserContext userContext) : IQueryHandler<GetAuditlogsByUserIdQuery, PagedList<AuditLogResponse>>
{
    public async Task<Result<PagedList<AuditLogResponse>>> Handle(
        GetAuditlogsByUserIdQuery request, 
        CancellationToken cancellationToken)
    {
        if (request.UserId != userContext.UserId)
        {
            return Result.Failure<PagedList<AuditLogResponse>>(UserErrors.Unauthorized);
        }

        IQueryable<AuditLog> auditLogsQuery = dbContext.AuditLogs.AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            string searchTerm = request.SearchTerm.ToLower();

            auditLogsQuery = auditLogsQuery.Where(a => 
                EF.Functions.ToTsVector("english", a.Action + " " + a.Description)
                    .Matches(EF.Functions.PhraseToTsQuery("english", searchTerm)));
        }

        if (request.LogType is not null && Enum.IsDefined(typeof(AuditLogType), request.LogType))
        {
            AuditLogType parsedLogType = (AuditLogType)request.LogType;

            auditLogsQuery = auditLogsQuery.Where(t => t.LogType == parsedLogType);
        }

        IQueryable<AuditLogResponse> auditLogResponsesQuery = auditLogsQuery
            .Where(t => t.UserId == request.UserId)
            .OrderByDescending(t => t.CreatedOnUtc)
            .Select(a => new AuditLogResponse(
                a.Id,
                a.UserId,
                a.LogType,
                a.Action,
                a.Description,
                a.RelatedEntityId,
                a.RelatedEntityType,
                a.CreatedOnUtc,
                a.ModifiedOnUtc));

        PagedList<AuditLogResponse> response = await PagedList<AuditLogResponse>.CreateAsync(
            auditLogResponsesQuery,
            request.Page,
            request.PageSize,
            cancellationToken);

        return response;
    }
}

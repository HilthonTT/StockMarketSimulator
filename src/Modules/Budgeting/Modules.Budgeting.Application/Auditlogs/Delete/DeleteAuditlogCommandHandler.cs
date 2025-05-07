using Application.Abstractions.Authentication;
using Application.Abstractions.Messaging;
using Modules.Budgeting.Application.Abstractions.Data;
using Modules.Budgeting.Domain.Entities;
using Modules.Budgeting.Domain.Errors;
using Modules.Budgeting.Domain.Repositories;
using SharedKernel;

namespace Modules.Budgeting.Application.Auditlogs.Delete;

internal sealed class DeleteAuditlogCommandHandler(
    IUserContext userContext,
    IAuditLogRepository auditLogRepository,
    IUnitOfWork unitOfWork) : ICommandHandler<DeleteAuditlogCommand>
{
    public async Task<Result> Handle(DeleteAuditlogCommand request, CancellationToken cancellationToken)
    {
        Option<AuditLog> optionAuditlog = await auditLogRepository.GetByIdAsync(request.AuditlogId, cancellationToken);

        if (!optionAuditlog.IsSome)
        {
            return Result.Failure(AuditLogErrors.NotFound(request.AuditlogId));
        }

        AuditLog auditLog = optionAuditlog.ValueOrThrow();

        if (auditLog.UserId != userContext.UserId)
        {
            return Result.Failure(UserErrors.Unauthorized);
        }

        auditLogRepository.Remove(auditLog);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

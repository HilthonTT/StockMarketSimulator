using Modules.Budgeting.Domain.Enums;
using SharedKernel;

namespace Modules.Budgeting.Domain.Entities;

public sealed class AuditLog : Entity, IAuditable, ISoftDeletable
{
    private AuditLog()
    {
    }

    private AuditLog(
        Guid id,
        Guid userId, 
        AuditLogType logType, 
        string action, 
        string? description = null, 
        Guid? relatedEntityId = null, 
        string? relatedEntityType = null)
    {
        Ensure.NotNullOrEmpty(id, nameof(id));
        Ensure.NotNullOrEmpty(userId, nameof(userId));
        Ensure.NotNullOrEmpty(action, nameof(action));

        Id = id;
        UserId = userId;
        LogType = logType;
        Action = action;
        Description = description;
        RelatedEntityId = relatedEntityId;
        RelatedEntityType = relatedEntityType;
    }

    public Guid Id { get; private set; }

    public Guid UserId { get; private set; }

    public string Action { get; private set; } = string.Empty;

    public string? Description { get; private set; }

    public AuditLogType LogType { get; private set; }

    public Guid? RelatedEntityId { get; private set; }

    public string? RelatedEntityType { get; private set; }

    public DateTime CreatedOnUtc { get; set; }

    public DateTime? ModifiedOnUtc { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime? DeletedOnUtc { get; set; }

    public static AuditLog Create(
        Guid userId,
        AuditLogType logType,
        string action,
        string? description = null,
        Guid? relatedEntityId = null,
        string? relatedEntityType = null)
    {
        return new AuditLog(
            Guid.CreateVersion7(),
            userId,
            logType,
            action,
            description,
            relatedEntityId,
            relatedEntityType);
    }
}

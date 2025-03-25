using SharedKernel;

namespace Modules.Users.Domain.Entities;

public sealed class EmailVerificationToken : Entity, IAuditable
{
    private EmailVerificationToken(Guid id, Guid userId, DateTime expiresOnUtc)
    {
        Ensure.NotNullOrEmpty(id, nameof(id));
        Ensure.NotNullOrEmpty(userId, nameof(userId));
        Ensure.NotNull(expiresOnUtc, nameof(expiresOnUtc));

        Id = id;
        UserId = userId;
        ExpiresOnUtc = expiresOnUtc;
    }

    private EmailVerificationToken()
    {
    }

    public Guid Id { get; private set; }

    public Guid UserId { get; private set; }

    public DateTime ExpiresOnUtc { get; private set; }

    public DateTime CreatedOnUtc { get; set; }

    public DateTime? ModifiedOnUtc { get; set; }

    public User User { get; set; } = null!;

    public static EmailVerificationToken Create(Guid userId, DateTime expiresOnUtc)
    {
        return new EmailVerificationToken(Guid.CreateVersion7(), userId, expiresOnUtc);
    }
}

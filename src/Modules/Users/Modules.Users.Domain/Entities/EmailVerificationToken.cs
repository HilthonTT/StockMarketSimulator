using SharedKernel;

namespace Modules.Users.Domain.Entities;

public sealed class EmailVerificationToken : Entity, IAuditable
{
    private const int DefaultExpirationHours = 24;

    private EmailVerificationToken(Guid id, Guid userId, DateTime expiresOnUtc)
    {
        Ensure.NotNullOrEmpty(id, nameof(id));
        Ensure.NotNullOrEmpty(userId, nameof(userId));
        Ensure.NotNull(expiresOnUtc, nameof(expiresOnUtc));

        Id = id;
        UserId = userId;
        ExpiresOnUtc = expiresOnUtc;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="EmailVerificationToken"/>
    /// </summary>
    /// <remarks>
    /// Required for EF Core
    /// </remarks>
    private EmailVerificationToken()
    {
    }

    public Guid Id { get; private set; }

    public Guid UserId { get; private set; }

    public DateTime ExpiresOnUtc { get; private set; }

    public DateTime CreatedOnUtc { get; set; }

    public DateTime? ModifiedOnUtc { get; set; }

    /// <summary>
    /// Gets or sets the user.
    /// </summary>
    /// <remarks>
    /// Navigation property for EF Core.
    /// </remarks>
    public User User { get; set; } = null!; 

    public static EmailVerificationToken Create(Guid id, Guid userId, IDateTimeProvider dateTimeProvider)
    {
        return new EmailVerificationToken(id, userId, CalculateExpirationDate(dateTimeProvider));
    }

    public void RefreshExpiration(IDateTimeProvider dateTimeProvider)
    {
        ExpiresOnUtc = CalculateExpirationDate(dateTimeProvider);
        ModifiedOnUtc = dateTimeProvider.UtcNow;
    }

    public bool IsExpired(IDateTimeProvider dateTimeProvider) => 
        dateTimeProvider.UtcNow >= ExpiresOnUtc;

    private static DateTime CalculateExpirationDate(IDateTimeProvider dateTimeProvider) =>
        dateTimeProvider.UtcNow.AddHours(DefaultExpirationHours);
}

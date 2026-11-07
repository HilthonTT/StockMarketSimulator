using SharedKernel;

namespace Modules.Users.Domain.Entities;

public sealed class RefreshToken : Entity
{
    private const int DefaultExpirationDays = 7;

    private RefreshToken(Guid id, string tokenHash, Guid userId, DateTime expiresOnUtc)
    {
        Ensure.NotNullOrEmpty(id, nameof(id));
        Ensure.NotNullOrEmpty(tokenHash, nameof(tokenHash));
        Ensure.NotNullOrEmpty(userId, nameof(userId));
        Ensure.NotNull(expiresOnUtc, nameof(expiresOnUtc));

        Id = id;
        TokenHash = tokenHash;
        UserId = userId;
        ExpiresOnUtc = expiresOnUtc;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RefreshToken"/>
    /// </summary>
    /// <remarks>
    /// Required for EF Core
    /// </remarks>
    private RefreshToken()
    {
    }

    public Guid Id { get; private set; }

    /// <summary>
    /// The SHA-256 hash of the refresh token. The plain token is only ever returned to the client,
    /// so a database leak doesn't hand out working sessions.
    /// </summary>
    public string TokenHash { get; private set; } = null!;

    public Guid UserId { get; private set; }

    public DateTime ExpiresOnUtc { get; private set; }

    /// <summary>
    /// Gets or sets the user.
    /// </summary>
    /// <remarks>
    /// Navigation property for EF Core.
    /// </remarks>
    public User User { get; set; } = null!;

    public static RefreshToken Create(string tokenHash, Guid userId)
    {
        return new(Guid.CreateVersion7(), tokenHash, userId, CalculateExpirationDate());
    }

    /// <summary>
    /// Rotates the token value. The original absolute expiry is kept on purpose: sliding it on every
    /// rotation would let a session (or a stolen token) live forever.
    /// </summary>
    public void Refresh(string newTokenHash)
    {
        Ensure.NotNullOrEmpty(newTokenHash, nameof(newTokenHash));

        TokenHash = newTokenHash;
    }

    public bool IsExpired() => DateTime.UtcNow >= ExpiresOnUtc;

    private static DateTime CalculateExpirationDate() => DateTime.UtcNow.AddDays(DefaultExpirationDays);
}

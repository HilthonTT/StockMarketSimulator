using SharedKernel;

namespace Modules.Users.Domain.Entities;

public sealed class RefreshToken : Entity
{
    private const int DefaultExpirationDays = 7;

    private RefreshToken(Guid id, string token, Guid userId, DateTime expiresOnUtc)
    {
        Ensure.NotNullOrEmpty(id, nameof(id));
        Ensure.NotNullOrEmpty(token, nameof(token));
        Ensure.NotNullOrEmpty(userId, nameof(userId));
        Ensure.NotNull(expiresOnUtc, nameof(expiresOnUtc));

        Id = id;
        Token = token;
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

    public string Token { get; private set; }

    public Guid UserId { get; private set; }

    public DateTime ExpiresOnUtc { get; private set; }

    /// <summary>
    /// Gets or sets the user.
    /// </summary>
    /// <remarks>
    /// Navigation property for EF Core.
    /// </remarks>
    public User User { get; set; } = null!;

    public static RefreshToken Create(string token, Guid userId)
    {
        return new(Guid.CreateVersion7(), token, userId, CalculateExpirationDate());
    }

    public void Refresh(string newToken)
    {
        Ensure.NotNullOrEmpty(newToken, nameof(newToken));

        Token = newToken;
        ExpiresOnUtc = CalculateExpirationDate();
    }

    public bool IsExpired() => DateTime.UtcNow >= ExpiresOnUtc;

    private static DateTime CalculateExpirationDate() => DateTime.UtcNow.AddDays(DefaultExpirationDays);
}

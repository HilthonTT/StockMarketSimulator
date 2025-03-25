using SharedKernel;

namespace Modules.Users.Domain.Entities;

public sealed class RefreshToken : Entity
{
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

    private RefreshToken()
    {
    }

    public Guid Id { get; private set; }

    public string Token { get; private set; }

    public Guid UserId { get; private set; }

    public DateTime ExpiresOnUtc { get; private set; }

    public static RefreshToken Create(string token, Guid userId)
    {
        DateTime expiresOnUtc = DateTime.UtcNow.AddDays(7);

        return new(Guid.CreateVersion7(), token, userId, expiresOnUtc);
    }

    public void Update(string token)
    {
        Token = token;
        ExpiresOnUtc = DateTime.UtcNow.AddDays(7);
    }
}

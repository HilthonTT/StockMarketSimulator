using SharedKernel;

namespace StockMarketSimulator.Api.Modules.Users.Domain;

internal sealed class User
{
    private User(Guid id, string email, string username, string passwordHash)
    {
        Ensure.NotNullOrEmpty(id, nameof(id));
        Ensure.NotNullOrEmpty(email, nameof(email));
        Ensure.NotNullOrEmpty(username, nameof(username));
        Ensure.NotNullOrEmpty(passwordHash, nameof(passwordHash));

        Id = id;
        Email = email;
        Username = username;
        PasswordHash = passwordHash;
    }

    private User()
    {
    }

    public Guid Id { get; private set; }

    public string Email { get; private set; }

    public string Username { get; private set; }

    public string PasswordHash { get; private set; }

    public static User Create(string email, string username, string passwordHash)
    {
        return new User(Guid.NewGuid(), email, username, passwordHash);
    }

    public void ChangePassword(string passwordHash)
    {
        PasswordHash = passwordHash;
    }
}

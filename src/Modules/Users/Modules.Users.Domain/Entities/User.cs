using Modules.Users.Domain.DomainEvents;
using Modules.Users.Domain.ValueObjects;
using SharedKernel;

namespace Modules.Users.Domain.Entities;

public sealed class User : Entity, IAuditable
{
    private User(Guid id, Username username, Email email, string passwordHash, bool emailVerified)
    {
        Ensure.NotNullOrEmpty(id, nameof(id));
        Ensure.NotNull(username, nameof(username));
        Ensure.NotNull(email, nameof(email));
        Ensure.NotNullOrEmpty(passwordHash, nameof(passwordHash));
        Ensure.NotNull(emailVerified, nameof(emailVerified));

        Id = id;
        Username = username;
        Email = email;
        PasswordHash = passwordHash;
        EmailVerified = emailVerified;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="User"/>
    /// </summary>
    /// <remarks>
    /// Required for EF Core
    /// </remarks>
    private User()
    {
    }

    public Guid Id { get; private set; }

    public Username Username { get; private set; }

    public Email Email { get; private set; }

    public string PasswordHash { get; private set; }

    public bool EmailVerified { get; private set; }

    public List<Role> Roles { get; set; } = [];

    public DateTime CreatedOnUtc { get; set; }

    public DateTime? ModifiedOnUtc { get; set; }

    public static User Create(Username username, Email email, string passwordHash, string verificationLink)
    {
        var user = new User(Guid.CreateVersion7(), username, email, passwordHash, false);

        user.Raise(new UserCreatedDomainEvent(user.Id, verificationLink));

        return user;
    }

    public void ChangePassword(string passwordHash)
    {
        if (PasswordHash == passwordHash)
        {
            return;
        }

        Raise(new UserPasswordChangedDomainEvent(Id));

        PasswordHash = passwordHash;
    }
}

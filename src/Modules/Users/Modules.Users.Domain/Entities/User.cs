﻿using Modules.Users.Domain.DomainEvents;
using Modules.Users.Domain.ValueObjects;
using SharedKernel;

namespace Modules.Users.Domain.Entities;

public sealed class User : Entity, IAuditable
{
    private readonly List<Role> _roles = [];

    private User(
        Guid id, 
        Username username, 
        Email email, 
        string passwordHash, 
        bool emailVerified, 
        bool hasPublicProfile)
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
        HasPublicProfile = hasPublicProfile;
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

    public bool HasPublicProfile { get; private set; }

    public Guid? ProfileImageId { get; private set; }

    public Guid? BannerImageId { get; private set; }

    public IReadOnlyList<Role> Roles => [.. _roles];

    public DateTime CreatedOnUtc { get; set; }

    public DateTime? ModifiedOnUtc { get; set; }

    public static User Create(
        Username username, 
        Email email, 
        string passwordHash, 
        string verificationLink,
        bool hasPublicProfile = false)
    {
        var user = new User(Guid.CreateVersion7(), username, email, passwordHash, false, hasPublicProfile);

        user.Raise(new UserCreatedDomainEvent(Guid.CreateVersion7(), user.Id, verificationLink));

        return user;
    }

    public void ChangePassword(string passwordHash)
    {
        if (PasswordHash == passwordHash)
        {
            return;
        }

        Raise(new UserPasswordChangedDomainEvent(Guid.CreateVersion7(), Id));

        PasswordHash = passwordHash;
    }

    public void VerifyEmail()
    {
        if (EmailVerified)
        {
            return;
        }

        EmailVerified = true;

        Raise(new UserEmailVerifiedDomainEvent(Guid.CreateVersion7(), Id));
    }

    public void AddRole(Role role)
    {
        if (Roles.FirstOrDefault(r => r.Id == role.Id) is not null)
        {
            return;
        }

        _roles.Add(role);
    }

    public void ChangeUsername(Username username)
    {
        Username = username;

        Raise(new UserNameChangedDomainEvent(Guid.CreateVersion7(), Id));
    }

    public void ChangeProfileImage(Guid imageId)
    {
        if (ProfileImageId == imageId)
        {
            return;
        }

        ProfileImageId = imageId;
    }

    public void ChangeBannerImage(Guid bannerId)
    {
        if (BannerImageId == bannerId)
        {
            return;
        }

        BannerImageId = bannerId;
    }

    public void ClearProfileImage()
    {
        ProfileImageId = null;
    }

    public void ClearBannerImage()
    {
        BannerImageId = null;
    }
}

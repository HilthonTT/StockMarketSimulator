using Modules.Users.Application.Abstractions.Authentication;
using Modules.Users.Application.Abstractions.Factories;
using Modules.Users.Application.Authentication.Register;
using Modules.Users.Domain.Entities;
using Modules.Users.Domain.Errors;
using Modules.Users.Domain.Repositories;
using Modules.Users.Domain.ValueObjects;
using SharedKernel;

namespace Modules.Users.Application.Authentication.Factories;

internal sealed class UserFactory : IUserFactory
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IEmailVerificationLinkFactory _emailVerificationLinkFactory;

    public UserFactory(
        IUserRepository userRepository, 
        IPasswordHasher passwordHasher,
        IEmailVerificationLinkFactory emailVerificationLinkFactory)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _emailVerificationLinkFactory = emailVerificationLinkFactory;
    }

    public async Task<Result<User>> CreateAsync(
        RegisterUserCommand command,
        Guid emailVerificationTokenId,
        CancellationToken cancellationToken = default)
    {
        Result<Email> emailResult = Email.Create(command.Email);
        Result<Username> usernameResult = Username.Create(command.Username);
        Result<Password> passwordResult = Password.Create(command.Password);
        Result validationResult = Result.FirstFailureOrSuccess(emailResult, usernameResult, passwordResult);

        if (validationResult.IsFailure)
        {
            return Result.Failure<User>(validationResult.Error);
        }

        Email email = emailResult.Value;
        Username username = usernameResult.Value;

        // Step 2: Ensure Email Uniqueness
        if (await _userRepository.EmailNotUniqueAsync(email, cancellationToken))
        {
            return Result.Failure<User>(UserErrors.EmailNotUnique);
        }

        string verificationLink = _emailVerificationLinkFactory.Create(emailVerificationTokenId);

        string passwordHash = _passwordHasher.Hash(command.Password);
        var user = User.Create(username, email, passwordHash, verificationLink);

        return user;
    }
}

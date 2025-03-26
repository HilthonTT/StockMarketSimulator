using Application.Abstractions.Messaging;
using Modules.Users.Application.Abstractions.Authentication;
using Modules.Users.Application.Abstractions.Data;
using Modules.Users.Domain.Entities;
using Modules.Users.Domain.Errors;
using Modules.Users.Domain.Repositories;
using Modules.Users.Domain.ValueObjects;
using SharedKernel;

namespace Modules.Users.Application.Users.Register;

internal sealed class RegisterUserCommandHandler(
    IUserRepository userRepository,
    IPasswordHasher passwordHasher,
    IEmailVerificationLinkFactory emailVerificationLinkFactory,
    IEmailVerificationTokenRepository emailVerificationTokenRepository,
    IUnitOfWork unitOfWork) : ICommandHandler<RegisterUserCommand, Guid>
{
    public async Task<Result<Guid>> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        // Step 1: Validate Email & Username
        Result<Email> emailResult = Email.Create(request.Email);
        Result<Username> usernameResult = Username.Create(request.Username);
        Result validationResult = Result.FirstFailureOrSuccess(emailResult, usernameResult);

        if (validationResult.IsFailure)
        {
            return Result.Failure<Guid>(validationResult.Error);
        }

        Email email = emailResult.Value;
        Username username = usernameResult.Value;

        // Step 2: Ensure Email Uniqueness
        if (await userRepository.EmailExistsAsync(email, cancellationToken))
        {
            return Result.Failure<Guid>(UserErrors.EmailNotUnique);
        }

        // Step 3: Generate Verification Token & Link
        Guid emailVerificationTokenId = Guid.CreateVersion7();
        string verificationLink = emailVerificationLinkFactory.Create(emailVerificationTokenId);

        // Step 4: Hash Password & Create User
        string passwordHash = passwordHasher.Hash(request.Password);
        var user = User.Create(username, email, passwordHash, verificationLink);

        userRepository.Insert(user);

        // Step 5: Create & Store Email Verification Token
        var emailVerificationToken = EmailVerificationToken.Create(emailVerificationTokenId, user.Id);
        emailVerificationTokenRepository.Insert(emailVerificationToken);

        // Step 6: Save Changes
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return user.Id;
    }
}

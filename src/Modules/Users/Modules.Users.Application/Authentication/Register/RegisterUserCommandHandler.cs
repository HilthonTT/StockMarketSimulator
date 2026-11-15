using Application.Abstractions.Messaging;
using EntityFramework.Exceptions.Common;
using Microsoft.Extensions.Options;
using Modules.Users.Application.Abstractions.Data;
using Modules.Users.Application.Abstractions.Factories;
using Modules.Users.Application.Abstractions.Options;
using Modules.Users.Domain.Entities;
using Modules.Users.Domain.Errors;
using Modules.Users.Domain.Repositories;
using SharedKernel;

namespace Modules.Users.Application.Authentication.Register;

internal sealed class RegisterUserCommandHandler(
    IUserFactory userFactory,
    IUserRepository userRepository,
    IEmailVerificationTokenRepository emailVerificationTokenRepository,
    IUnitOfWork unitOfWork,
    IDateTimeProvider dateTimeProvider,
    IOptions<UsersOptions> usersOptions) : ICommandHandler<RegisterUserCommand, Guid>
{
    public async Task<Result<Guid>> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        try
        {
            Guid emailVerificationTokenId = Guid.CreateVersion7();

            Result<User> userResult = await userFactory.CreateAsync(request, emailVerificationTokenId, cancellationToken);
            if (userResult.IsFailure)
            {
                return Result.Failure<Guid>(userResult.Error);
            }

            User user = userResult.Value;

            userRepository.Insert(user);

            var emailVerificationToken = EmailVerificationToken.Create(emailVerificationTokenId, user.Id, dateTimeProvider);
            emailVerificationTokenRepository.Insert(emailVerificationToken);

            await unitOfWork.SaveChangesAsync(cancellationToken);

            if (usersOptions.Value.AutoVerifyEmail)
            {
                // Development convenience: the same domain path as VerifyEmailCommandHandler, so the
                // email-verified event still provisions the budget and sends (or logs) the welcome email.
                // The verification email is still sent/logged; its link then reports "already verified".
                // This runs after the user is saved: while the user is still being added, EF would treat
                // the role reference as a new row and try to insert it again (pk_roles violation).
                user.VerifyEmail();
                user.AddRole(Role.Registered.CreateReference());

                await unitOfWork.SaveChangesAsync(cancellationToken);
            }

            return user.Id;
        }
        catch (UniqueConstraintException)
        {
            return Result.Failure<Guid>(UserErrors.EmailNotUnique);
        }
    }
}

using Application.Abstractions.Messaging;
using Modules.Users.Application.Abstractions.Data;
using Modules.Users.Domain.Entities;
using Modules.Users.Domain.Errors;
using Modules.Users.Domain.Repositories;
using SharedKernel;

namespace Modules.Users.Application.Authentication.VerifyEmail;

internal sealed class VerifyEmailCommandHandler(
    IEmailVerificationTokenRepository emailVerificationTokenRepository,
    IUnitOfWork unitOfWork) : ICommandHandler<VerifyEmailCommand>
{
    public async Task<Result> Handle(VerifyEmailCommand request, CancellationToken cancellationToken)
    {
        Option<EmailVerificationToken> optionToken = 
            await emailVerificationTokenRepository.GetByIdAsync(request.TokenId, cancellationToken);

        if (!optionToken.IsSome)
        {
            return Result.Failure(EmailVerificationTokenErrors.Expired);
        }

        EmailVerificationToken token = optionToken.ValueOrThrow();

        if (token.IsExpired())
        {
            return Result.Failure(EmailVerificationTokenErrors.Expired);
        }

        if (token.User.EmailVerified)
        {
            return Result.Failure(UserErrors.EmailAlreadyVerified);
        }

        token.User.VerifyEmail();

        token.User.AddRole(Role.Registered);

        emailVerificationTokenRepository.Remove(token);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

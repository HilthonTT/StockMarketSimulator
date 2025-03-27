using Application.Abstractions.Messaging;
using Modules.Users.Application.Abstractions.Data;
using Modules.Users.Domain.Entities;
using Modules.Users.Domain.Errors;
using Modules.Users.Domain.Repositories;
using SharedKernel;

namespace Modules.Users.Application.Users.VerifyEmail;

internal sealed class VerifyEmailCommandHandler(
    IEmailVerificationTokenRepository emailVerificationTokenRepository,
    IUnitOfWork unitOfWork) : ICommandHandler<VerifyEmailCommand>
{
    public async Task<Result> Handle(VerifyEmailCommand request, CancellationToken cancellationToken)
    {
        EmailVerificationToken? token = await emailVerificationTokenRepository.GetByIdAsync(request.TokenId, cancellationToken);

        if (token is null || token.IsExpired())
        {
            return Result.Failure(EmailVerificationTokenErrors.Expired);
        }

        if (token.User.EmailVerified)
        {
            return Result.Failure(UserErrors.EmailAlreadyVerified);
        }

        token.User.VerifyEmail();

        emailVerificationTokenRepository.Remove(token);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

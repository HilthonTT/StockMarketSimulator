using Application.Abstractions.Messaging;
using Modules.Users.Application.Abstractions.Authentication;
using Modules.Users.Application.Abstractions.Data;
using Modules.Users.Application.Abstractions.BackgroundJobs;
using Modules.Users.Domain.Entities;
using Modules.Users.Domain.Errors;
using Modules.Users.Domain.Repositories;
using Modules.Users.Domain.ValueObjects;
using SharedKernel;

namespace Modules.Users.Application.Authentication.ResendEmailVerification;

internal sealed class ResendEmailVerificationCommandHandler(
    IUserRepository userRepository,
    IUserJobScheduler userJobScheduler,
    IEmailVerificationLinkFactory emailVerificationLinkFactory,
    IEmailVerificationTokenRepository emailVerificationTokenRepository,
    IUnitOfWork unitOfWork,
    IDateTimeProvider dateTimeProvider) : ICommandHandler<ResendEmailVerificationCommand>
{
    public async Task<Result> Handle(ResendEmailVerificationCommand request, CancellationToken cancellationToken)
    {
        Result<Email> emailResult = Email.Create(request.Email);
        if (emailResult.IsFailure)
        {
            return emailResult;
        }

        Option<User> optionUser = await userRepository.GetByEmailAsync(emailResult.Value, cancellationToken);
        if (!optionUser.IsSome)
        {
            return Result.Failure(UserErrors.NotFoundByEmail);
        }

        User user = optionUser.ValueOrThrow();

        if (user.EmailVerified)
        {
            return Result.Failure(UserErrors.EmailAlreadyVerified);
        }

        Guid emailVerificationTokenId = Guid.CreateVersion7();
        string verificationLink = emailVerificationLinkFactory.Create(emailVerificationTokenId);

        var emailVerificationToken = EmailVerificationToken.Create(emailVerificationTokenId, user.Id, dateTimeProvider);
        emailVerificationTokenRepository.Insert(emailVerificationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        await userJobScheduler.ScheduleEmailVerificationAsync(
            user.Id,
            user.Email.Value,
            verificationLink,
            cancellationToken);

        return Result.Success();
    }
}

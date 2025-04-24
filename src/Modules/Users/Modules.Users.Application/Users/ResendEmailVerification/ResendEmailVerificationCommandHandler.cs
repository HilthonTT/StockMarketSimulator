using Application.Abstractions.Messaging;
using Modules.Users.Application.Abstractions.Authentication;
using Modules.Users.Application.Abstractions.Data;
using Modules.Users.BackgroundJobs.Users.EmailVerification;
using Modules.Users.Domain.Entities;
using Modules.Users.Domain.Errors;
using Modules.Users.Domain.Repositories;
using Modules.Users.Domain.ValueObjects;
using Quartz;
using SharedKernel;

namespace Modules.Users.Application.Users.ResendEmailVerification;

internal sealed class ResendEmailVerificationCommandHandler(
    IUserRepository userRepository,
    ISchedulerFactory schedulerFactory,
    IEmailVerificationLinkFactory emailVerificationLinkFactory,
    IEmailVerificationTokenRepository emailVerificationTokenRepository,
    IUnitOfWork unitOfWork) : ICommandHandler<ResendEmailVerificationCommand>
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

        IScheduler scheduler = await schedulerFactory.GetScheduler(cancellationToken);

        Guid emailVerificationTokenId = Guid.CreateVersion7();
        string verificationLink = emailVerificationLinkFactory.Create(emailVerificationTokenId);

        var emailVerificationToken = EmailVerificationToken.Create(emailVerificationTokenId, user.Id);
        emailVerificationTokenRepository.Insert(emailVerificationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        var jobData = new JobDataMap
        {
            { "userId", user.Id },
            { "email", user.Email.Value },
            { "verification-link", verificationLink }
        };

        IJobDetail job = JobBuilder.Create<EmailVerificationJob>()
            .WithIdentity($"reminder-{Guid.CreateVersion7()}", "email-reminders")
            .SetJobData(jobData)
            .Build();

        ITrigger trigger = TriggerBuilder.Create()
            .WithIdentity($"trigger-{Guid.CreateVersion7()}", "email-reminders")
            .StartNow()
            .Build();

        await scheduler.ScheduleJob(job, trigger, cancellationToken);

        return Result.Success();
    }
}

using Application.Abstractions.Messaging;
using EntityFramework.Exceptions.Common;
using Modules.Users.Application.Abstractions.Data;
using Modules.Users.Application.Abstractions.Factories;
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
    IDateTimeProvider dateTimeProvider) : ICommandHandler<RegisterUserCommand, Guid>
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

            return user.Id;
        }
        catch (UniqueConstraintException)
        {
            return Result.Failure<Guid>(UserErrors.EmailNotUnique);
        }
    }
}

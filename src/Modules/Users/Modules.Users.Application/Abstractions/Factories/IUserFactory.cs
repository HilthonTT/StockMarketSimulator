using Modules.Users.Application.Authentication.Register;
using Modules.Users.Domain.Entities;
using SharedKernel;

namespace Modules.Users.Application.Abstractions.Factories;

public interface IUserFactory
{
    Task<Result<User>> CreateAsync(RegisterUserCommand command, Guid emailVerificationId, CancellationToken cancellationToken = default);
}

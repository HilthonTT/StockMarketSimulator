using Application.Abstractions.Messaging;
using SharedKernel;

namespace Modules.Users.Application.Users.Register;

internal sealed class RegisterUserCommandHandler : ICommandHandler<RegisterUserCommand, Guid>
{
    public async Task<Result<Guid>> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        return Guid.Empty;
    }
}

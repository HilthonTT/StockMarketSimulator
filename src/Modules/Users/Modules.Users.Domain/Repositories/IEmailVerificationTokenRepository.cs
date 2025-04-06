using Modules.Users.Domain.Entities;
using SharedKernel;

namespace Modules.Users.Domain.Repositories;

public interface IEmailVerificationTokenRepository
{
    Task<Option<EmailVerificationToken>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    void Insert(EmailVerificationToken token);

    void Remove(EmailVerificationToken token);
}

using Modules.Users.Domain.Entities;

namespace Modules.Users.Domain.Repositories;

public interface IEmailVerificationTokenRepository
{
    Task<EmailVerificationToken?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    void Insert(EmailVerificationToken token);

    void Remove(EmailVerificationToken token);
}

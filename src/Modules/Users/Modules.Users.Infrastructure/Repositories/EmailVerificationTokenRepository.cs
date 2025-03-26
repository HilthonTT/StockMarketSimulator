using Microsoft.EntityFrameworkCore;
using Modules.Users.Domain.Entities;
using Modules.Users.Domain.Repositories;
using Modules.Users.Infrastructure.Database;

namespace Modules.Users.Infrastructure.Repositories;

internal sealed class EmailVerificationTokenRepository(UsersDbContext context) : IEmailVerificationTokenRepository
{
    public Task<EmailVerificationToken?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return context.EmailVerificationTokens
            .Include(e => e.User)
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }

    public void Insert(EmailVerificationToken token)
    {
        context.EmailVerificationTokens.Add(token);
    }

    public void Remove(EmailVerificationToken token)
    {
        context.EmailVerificationTokens.Remove(token);
    }
}

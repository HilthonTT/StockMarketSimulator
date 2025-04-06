using Microsoft.EntityFrameworkCore;
using Modules.Users.Domain.Entities;
using Modules.Users.Domain.Repositories;
using Modules.Users.Infrastructure.Database;
using SharedKernel;

namespace Modules.Users.Infrastructure.Repositories;

internal sealed class EmailVerificationTokenRepository(UsersDbContext context) : IEmailVerificationTokenRepository
{
    public async Task<Option<EmailVerificationToken>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        EmailVerificationToken? token = await context.EmailVerificationTokens
            .Include(e => e.User)
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

        return Option<EmailVerificationToken>.Some(token);
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

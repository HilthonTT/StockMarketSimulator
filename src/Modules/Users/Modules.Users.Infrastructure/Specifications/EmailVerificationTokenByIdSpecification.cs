using Infrastructure.Database.Specifications;
using Modules.Users.Domain.Entities;

namespace Modules.Users.Infrastructure.Specifications;

internal sealed class EmailVerificationTokenByIdSpecification : Specification<EmailVerificationToken>
{
    public EmailVerificationTokenByIdSpecification(Guid id)
        : base(emailVerificationToken => emailVerificationToken.Id == id)
    {
        AddInclude(emailVerificationToken => emailVerificationToken.User);
    }
}

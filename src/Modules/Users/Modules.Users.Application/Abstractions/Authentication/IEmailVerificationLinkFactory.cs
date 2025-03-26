namespace Modules.Users.Application.Abstractions.Authentication;

public interface IEmailVerificationLinkFactory
{
    string Create(Guid emailVerificationTokenId);
}

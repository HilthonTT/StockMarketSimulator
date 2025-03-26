using Modules.Users.Domain.Entities;

namespace Modules.Users.Application.Abstractions.Authentication;

public interface ITokenProvider
{
    string Create(User user);
}

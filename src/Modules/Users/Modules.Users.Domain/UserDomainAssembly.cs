using System.Reflection;

namespace Modules.Users.Domain;

public static class UserDomainAssembly
{
    public static readonly Assembly Instance = typeof(UserDomainAssembly).Assembly;
}

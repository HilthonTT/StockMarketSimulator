using System.Reflection;

namespace Modules.Users.Domain;

public static class UsersDomainAssembly
{
    public static readonly Assembly Instance = typeof(UsersDomainAssembly).Assembly;
}

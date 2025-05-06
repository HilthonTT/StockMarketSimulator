using System.Reflection;

namespace Modules.Users.Application;

public static class UsersApplicationAssembly
{
    public static readonly Assembly Instance = typeof(UsersApplicationAssembly).Assembly;
}

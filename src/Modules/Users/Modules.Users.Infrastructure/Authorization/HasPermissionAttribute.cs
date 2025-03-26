using Microsoft.AspNetCore.Authorization;

namespace Modules.Users.Infrastructure.Authorization;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public sealed class HasPermissionAttribute(string permission) : AuthorizeAttribute(permission)
{
}

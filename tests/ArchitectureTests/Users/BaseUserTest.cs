using System.Reflection;
using Modules.Users.Application.Users.GetById;
using Modules.Users.Domain.Entities;
using Modules.Users.Infrastructure.Outbox;

namespace ArchitectureTests.Users;

public abstract class BaseUserTest
{
    protected static readonly Assembly DomainAssembly = typeof(User).Assembly;
    protected static readonly Assembly ApplicationAssembly = typeof(GetUserByIdQuery).Assembly;
    protected static readonly Assembly InfrastructureAssembly = typeof(ProcessUserOutboxMessagesJob).Assembly;
    protected static readonly Assembly PresentationAssembly = typeof(Program).Assembly;
}

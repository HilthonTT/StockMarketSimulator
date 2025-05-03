using Infrastructure.Database.Specifications;
using Modules.Users.Domain.Entities;
using Modules.Users.Domain.ValueObjects;

namespace Modules.Users.Infrastructure.Specifications;

internal sealed class UserByEmailSpecification : Specification<User>
{
    public UserByEmailSpecification(Email email) 
        : base(user => user.Email == email)
    {
        AddInclude(user => user.Roles);
    }
}

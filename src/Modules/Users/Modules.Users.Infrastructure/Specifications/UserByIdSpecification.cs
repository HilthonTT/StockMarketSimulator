using Infrastructure.Database.Specifications;
using Modules.Users.Domain.Entities;

namespace Modules.Users.Infrastructure.Specifications;

internal sealed class UserByIdSpecification : Specification<User>
{
    public UserByIdSpecification(Guid userId) 
        : base(user => user.Id == userId)
    {
        AddInclude(user => user.Roles);
    }
}

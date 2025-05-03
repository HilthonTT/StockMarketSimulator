using Infrastructure.Database.Specifications;
using Microsoft.EntityFrameworkCore;
using Modules.Users.Domain.Entities;
using Modules.Users.Domain.Repositories;
using Modules.Users.Domain.ValueObjects;
using Modules.Users.Infrastructure.Database;
using Modules.Users.Infrastructure.Specifications;
using SharedKernel;

namespace Modules.Users.Infrastructure.Repositories;

internal sealed class UserRepository(UsersDbContext context) : IUserRepository
{
    public Task<bool> EmailNotUniqueAsync(Email email, CancellationToken cancellationToken = default)
    {
        return ApplySpecification(new UserByEmailSpecification(email))
            .AnyAsync(cancellationToken);
    }

    public async Task<Option<User>> GetByEmailAsync(Email email, CancellationToken cancellationToken = default)
    {
        User? user = await ApplySpecification(new UserByEmailSpecification(email))
            .FirstOrDefaultAsync(cancellationToken);

        return Option<User>.Some(user);
    }

    public async Task<Option<User>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        User? user = await ApplySpecification(new UserByIdSpecification(id))
            .FirstOrDefaultAsync(cancellationToken);

        return Option<User>.Some(user);
    }

    public void Insert(User user)
    {
        context.Users.Add(user);
    }

    public void Remove(User user)
    {
        context.Users.Remove(user);
    }

    private IQueryable<User> ApplySpecification(Specification<User> specification)
    {
        return SpecificationEvaluator.GetQuery(context.Users, specification);
    }
}

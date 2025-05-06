using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace Modules.Budgeting.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddBudgetingApplication(this IServiceCollection services)
    {
        services.AddMediatR(config => config.RegisterServicesFromAssembly(BudgetingApplicationAssembly.Instance));

        services.AddValidatorsFromAssembly(BudgetingApplicationAssembly.Instance, includeInternalTypes: true);

        return services;
    }
}

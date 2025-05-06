using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace Modules.Budgeting.BackgroundJobs;

public static class DependencyInjection
{
    public static IServiceCollection AddBudgetingBackgroundJobs(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(BudgetingBackgroundJobsAssembly.Instance, includeInternalTypes: true);

        return services;
    }
}

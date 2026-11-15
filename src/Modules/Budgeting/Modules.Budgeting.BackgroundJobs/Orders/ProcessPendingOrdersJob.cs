using Application.Abstractions.Messaging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Modules.Budgeting.Application.Orders.Fill;
using Modules.Budgeting.Application.Orders.GetTriggered;
using Quartz;
using SharedKernel;

namespace Modules.Budgeting.BackgroundJobs.Orders;

/// <summary>
/// Fills limit and stop orders whose trigger the live price has crossed. Each order runs in its own
/// scope (and unit of work), so one order failing never affects the others.
/// </summary>
[DisallowConcurrentExecution]
public sealed class ProcessPendingOrdersJob(
    IServiceScopeFactory serviceScopeFactory,
    ILogger<ProcessPendingOrdersJob> logger) : IJob
{
    private const int BatchSize = 500;

    public async ValueTask Execute(IJobExecutionContext context, CancellationToken cancellationToken = default)
    {
        List<Guid> triggered;

        using (IServiceScope scope = serviceScopeFactory.CreateScope())
        {
            ISender sender = scope.ServiceProvider.GetRequiredService<ISender>();

            Result<List<Guid>> result = await sender.Send(new GetTriggeredOrderIdsQuery(BatchSize), cancellationToken);
            if (result.IsFailure || result.Value.Count == 0)
            {
                return;
            }

            triggered = result.Value;
        }

        foreach (Guid orderId in triggered)
        {
            try
            {
                using IServiceScope scope = serviceScopeFactory.CreateScope();
                ISender sender = scope.ServiceProvider.GetRequiredService<ISender>();

                Result<bool> result = await sender.Send(new FillOrderCommand(orderId), cancellationToken);

                if (result.IsFailure)
                {
                    logger.LogInformation("Order {OrderId} not filled this tick: {Error}", orderId, result.Error.Code);
                }
            }
            catch (Exception exception) when (exception is not OperationCanceledException)
            {
                logger.LogError(exception, "Filling order {OrderId} failed", orderId);
            }
        }
    }
}

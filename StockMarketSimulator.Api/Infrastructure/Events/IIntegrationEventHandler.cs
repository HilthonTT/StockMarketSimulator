using MassTransit;

namespace StockMarketSimulator.Api.Infrastructure.Events;

public interface IIntegrationEventHandler<in TCommand> : IConsumer<TCommand>
    where TCommand : class, IIntegrationEvent
{
}

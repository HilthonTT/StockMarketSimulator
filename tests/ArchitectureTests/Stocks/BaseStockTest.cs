using Modules.Stocks.Application.Stocks.GetByTicker;
using Modules.Stocks.Domain.Entities;
using Modules.Stocks.Infrastructure.Outbox;
using System.Reflection;

namespace ArchitectureTests.Stocks;

public abstract class BaseStockTest
{
    protected static readonly Assembly DomainAssembly = typeof(Stock).Assembly;
    protected static readonly Assembly ApplicationAssembly = typeof(GetStockByTickerQuery).Assembly;
    protected static readonly Assembly InfrastructureAssembly = typeof(ProcessStockOutboxMessagesJob).Assembly;
    protected static readonly Assembly PresentationAssembly = typeof(Program).Assembly;
}

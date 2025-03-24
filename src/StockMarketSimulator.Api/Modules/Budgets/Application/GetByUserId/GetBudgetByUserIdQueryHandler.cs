using FluentValidation;
using FluentValidation.Results;
using SharedKernel;
using StockMarketSimulator.Api.Infrastructure.Database;
using StockMarketSimulator.Api.Infrastructure.Helpers;
using StockMarketSimulator.Api.Infrastructure.Messaging;
using StockMarketSimulator.Api.Modules.Budgets.Contracts;
using StockMarketSimulator.Api.Modules.Budgets.Domain;
using StockMarketSimulator.Api.Modules.Users.Api;
using StockMarketSimulator.Api.Modules.Users.Domain;

namespace StockMarketSimulator.Api.Modules.Budgets.Application.GetByUserId;

internal sealed class GetBudgetByUserIdQueryHandler : IQueryHandler<GetBudgetByUserIdQuery, BudgetResponse>
{
    private readonly IDbConnectionFactory _dbConnectionFactory;
    private readonly IBudgetRepository _budgetRepository;
    private readonly IUsersApi _usersApi;
    private readonly IValidator<GetBudgetByUserIdQuery> _validator;

    public GetBudgetByUserIdQueryHandler(
        IDbConnectionFactory dbConnectionFactory,
        IBudgetRepository budgetRepository,
        IUsersApi usersApi,
        IValidator<GetBudgetByUserIdQuery> validator)
    {
        _dbConnectionFactory = dbConnectionFactory;
        _budgetRepository = budgetRepository;
        _usersApi = usersApi;
        _validator = validator;
    }

    public async Task<Result<BudgetResponse>> Handle(
        GetBudgetByUserIdQuery query, 
        CancellationToken cancellationToken = default)
    {
        ValidationResult validationResult = await _validator.ValidateAsync(query, cancellationToken);
        if (!validationResult.IsValid)
        {
            return Result.Failure<BudgetResponse>(ValidationErrorFactory.CreateValidationError(validationResult.Errors));
        }

        if (query.UserId != _usersApi.UserId)
        {
            return Result.Failure<BudgetResponse>(UserErrors.Unauthorized);
        }

        await using var connection = await _dbConnectionFactory.GetOpenConnectionAsync(cancellationToken);

        Budget? budget = await _budgetRepository.GetByUserIdAsync(connection, query.UserId, cancellationToken: cancellationToken);
        if (budget is null)
        {
            return Result.Failure<BudgetResponse>(BudgetErrors.NotFoundByUserId(query.UserId));
        }

        return new BudgetResponse(budget.Id, budget.UserId, budget.BuyingPower);
    }
}

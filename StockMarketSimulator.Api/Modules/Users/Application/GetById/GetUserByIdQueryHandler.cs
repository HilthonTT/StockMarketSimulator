using FluentValidation;
using FluentValidation.Results;
using SharedKernel;
using StockMarketSimulator.Api.Infrastructure.Database;
using StockMarketSimulator.Api.Infrastructure.Helpers;
using StockMarketSimulator.Api.Infrastructure.Messaging;
using StockMarketSimulator.Api.Modules.Users.Contracts;
using StockMarketSimulator.Api.Modules.Users.Domain;

namespace StockMarketSimulator.Api.Modules.Users.Application.GetById;

internal sealed class GetUserByIdQueryHandler : IQueryHandler<GetUserByIdQuery, UserResponse>
{
    private readonly IDbConnectionFactory _dbConnectionFactory;
    private readonly IUserRepository _userRepository;
    private readonly IValidator<GetUserByIdQuery> _validator;

    public GetUserByIdQueryHandler(
        IDbConnectionFactory dbConnectionFactory,
        IUserRepository userRepository,
        IValidator<GetUserByIdQuery> validator)
    {
        _dbConnectionFactory = dbConnectionFactory;
        _userRepository = userRepository;
        _validator = validator;
    }

    public async Task<Result<UserResponse>> Handle(GetUserByIdQuery query, CancellationToken cancellationToken = default)
    {
        ValidationResult validationResult = await _validator.ValidateAsync(query, cancellationToken);
        if (!validationResult.IsValid)
        {
            return Result.Failure<UserResponse>(ValidationErrorFactory.CreateValidationError(validationResult.Errors));
        }

        await using var connection = await _dbConnectionFactory.GetOpenConnectionAsync(cancellationToken);

        User? user = await _userRepository.GetByIdAsync(connection, query.UserId, cancellationToken);
        if (user is null)
        {
            return Result.Failure<UserResponse>(UserErrors.NotFound(query.UserId));
        }

        var response = new UserResponse(user.Id, user.Email, user.Username);

        return response;
    }
}

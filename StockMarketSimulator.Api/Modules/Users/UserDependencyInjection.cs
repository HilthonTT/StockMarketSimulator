using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using StockMarketSimulator.Api.Infrastructure;
using StockMarketSimulator.Api.Modules.Users.Api;
using StockMarketSimulator.Api.Modules.Users.Application.ChangePassword;
using StockMarketSimulator.Api.Modules.Users.Application.DeleteAll;
using StockMarketSimulator.Api.Modules.Users.Application.GetById;
using StockMarketSimulator.Api.Modules.Users.Application.GetCurrent;
using StockMarketSimulator.Api.Modules.Users.Application.Login;
using StockMarketSimulator.Api.Modules.Users.Application.LoginWithRefreshToken;
using StockMarketSimulator.Api.Modules.Users.Application.Register;
using StockMarketSimulator.Api.Modules.Users.Application.RevokeRefreshTokens;
using StockMarketSimulator.Api.Modules.Users.Domain;
using StockMarketSimulator.Api.Modules.Users.Infrastructure;
using StockMarketSimulator.Api.Modules.Users.Persistence;
using System.Text;

namespace StockMarketSimulator.Api.Modules.Users;

public static class UserDependencyInjection
{
    public static IServiceCollection AddUserModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services
            .AddAuthenticationInternal(configuration)
            .AddPersistence()
            .AddUseCases()
            .AddPublicApis();

        services.AddValidatorsFromAssembly(typeof(UserDependencyInjection).Assembly, includeInternalTypes: true);

        return services;
    }

    private static IServiceCollection AddPublicApis(this IServiceCollection services)
    {
        services.AddScoped<IUsersApi, UsersApi>();

        return services;
    }

    private static IServiceCollection AddUseCases(this IServiceCollection services)
    {
        services.AddScoped<RegisterUserCommandHandler>();
        services.AddScoped<LoginUserCommandHandler>();
        services.AddScoped<LoginUserWithRefreshTokenCommandHandler>();
        services.AddScoped<RevokeRefreshTokensCommandHandler>();
        services.AddScoped<ChangeUserPasswordCommandHandler>();
        services.AddScoped<DeleteAllUsersCommandHandler>();

        services.AddScoped<GetCurrentUserQueryHandler>();
        services.AddScoped<GetUserByIdQueryHandler>();

        return services;
    }

    private static IServiceCollection AddPersistence(this IServiceCollection services)
    {
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
       
        return services;
    }

    private static IServiceCollection AddAuthenticationInternal(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtOptions>(configuration.GetSection("Jwt"));

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                using IServiceScope scope = services.BuildServiceProvider().CreateScope();
                var jwtOptions = scope.ServiceProvider.GetRequiredService<IOptions<JwtOptions>>().Value;

                options.RequireHttpsMetadata = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Secret)),
                    ValidIssuer = jwtOptions.Issuer,
                    ValidAudience = jwtOptions.Audience,
                    ClockSkew = TimeSpan.Zero
                };
            });

        services.AddHttpContextAccessor();
        services.AddScoped<IUserContext, UserContext>();
        services.AddScoped<ITokenProvider, TokenProvider>();

        services.AddSingleton<IPasswordHasher, PasswordHasher>();

        return services;
    }
}

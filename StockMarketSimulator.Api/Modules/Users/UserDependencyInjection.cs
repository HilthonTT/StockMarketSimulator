using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using StockMarketSimulator.Api.Modules.Users.Infrastructure;
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
            .AddAuthorizationInternal();

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
        services.AddSingleton<IPasswordHasher, PasswordHasher>();
        services.AddSingleton<ITokenProvider, TokenProvider>();

        return services;
    }

    private static IServiceCollection AddAuthorizationInternal(this IServiceCollection services)
    {
        services.AddAuthorization();

        return services;
    }
}

using Quartz;
using StockMarketSimulator.Api.Infrastructure.Email;

namespace StockMarketSimulator.Api.Modules.Users.Infrastructure.Jobs;

internal sealed class EmailVerificationJob : IJob
{
    private readonly ILogger<EmailVerificationJob> _logger;
    private readonly IEmailService _emailService;

    public EmailVerificationJob(
        ILogger<EmailVerificationJob> logger,
        IEmailService emailService)
    {
        _logger = logger;
        _emailService = emailService;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        var data = context.MergedJobDataMap;

        string? email = data.GetString("email");
        string? verificationToken = data.GetString("verification-token");
        string? verificationEndpoint = data.GetString("verification-endpoint");

        if (string.IsNullOrWhiteSpace(email) ||
             string.IsNullOrWhiteSpace(verificationToken) ||
             string.IsNullOrWhiteSpace(verificationEndpoint))
        {
            _logger.LogWarning("Email verification job missing required parameters: Email={Email}, Token={Token}, Endpoint={Endpoint}",
                email, verificationToken, verificationEndpoint);

            return;
        }

        try
        {
            // TODO: Add email verification email
            await Task.CompletedTask;

            _logger.LogInformation("Sent verification email to user with email {Email}: {Token}", email, verificationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send verification email to user with email {Email}", email);

            // Rethrow to let Quartz handle retry logic
            throw;
        }
    }
}

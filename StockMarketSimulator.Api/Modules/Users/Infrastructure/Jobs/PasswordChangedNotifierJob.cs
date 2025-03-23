using System.Threading;
using Quartz;
using StockMarketSimulator.Api.Infrastructure.Email;
using StockMarketSimulator.Api.Modules.Users.Domain;

namespace StockMarketSimulator.Api.Modules.Users.Infrastructure.Jobs;

internal sealed class PasswordChangedNotifierJob : IJob
{
    public const string Name = nameof(PasswordChangedNotifierJob);

    private readonly ILogger<PasswordChangedNotifierJob> _logger;
    private readonly IEmailService _emailService;

    public PasswordChangedNotifierJob(
        ILogger<PasswordChangedNotifierJob> logger,
        IEmailService emailService)
    {
        _logger = logger;
        _emailService = emailService;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        var data = context.MergedJobDataMap;

        string? email = data.GetString("email");
        string? username = data.GetString("username");
        DateTime dateTime = data.GetDateTime("date");

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(username))
        {
            return;
        }

        try
        {
            string subject = "Your Password Has Been Changed";

            string body = $"""
                Hello {username},

                We wanted to let you know that your password was successfully changed on {dateTime:MMMM d, yyyy 'at' HH:mm} (UTC).

                If you made this change, no further action is needed.

                If you did not request this change, please reset your password immediately and contact our support team.

                Best regards,  
                Your Support Team
                """;

            await _emailService.SendEmailAsync(email, subject, body, context.CancellationToken);

            _logger.LogInformation("Sent password changed email to user with email {Email}", email);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send password changed to user with email {Email}", email);

            // Rethrow to let Quartz handle retry logic
            throw;
        }
    }
}

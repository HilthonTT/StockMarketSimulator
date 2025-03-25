using Application.Abstractions.Emails;
using Application.Abstractions.Notifications;
using Contracts.Emails;

namespace Infrastructure.Notifications;

internal sealed class NotificationService(IEmailService emailService) : INotificationService
{
    public Task SendPasswordChangedAsync(PasswordChangedEmail request, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public Task SendPurchaseConfirmedAsync(PurchaseConfirmedEmail request, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public Task SendWelcomeAsync(WelcomeEmail request, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}

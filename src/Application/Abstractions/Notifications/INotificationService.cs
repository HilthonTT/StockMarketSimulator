using Contracts.Emails;

namespace Application.Abstractions.Notifications;

public interface INotificationService
{
    Task SendPurchaseConfirmedAsync(PurchaseConfirmedEmail request, CancellationToken cancellationToken = default);

    Task SendWelcomeAsync(WelcomeEmail request, CancellationToken cancellationToken = default);

    Task SendPasswordChangedAsync(PasswordChangedEmail request, CancellationToken cancellationToken = default);
}

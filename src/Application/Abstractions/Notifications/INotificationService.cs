using Contracts.Emails;

namespace Application.Abstractions.Notifications;

public interface INotificationService
{
    Task SendEmailVerificationAsync(EmailVerificationEmail request, CancellationToken cancellationToken = default);

    Task SendPurchaseConfirmedAsync(PurchaseConfirmedEmail request, CancellationToken cancellationToken = default);

    Task SendWelcomeAsync(WelcomeEmail request, CancellationToken cancellationToken = default);

    Task SendPasswordChangedAsync(PasswordChangedEmail request, CancellationToken cancellationToken = default);

    Task SendSellConfirmedAsync(SellConfirmedEmail request, CancellationToken cancellationToken = default);
}

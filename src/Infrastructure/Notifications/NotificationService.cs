using Application.Abstractions.Emails;
using Application.Abstractions.Notifications;
using Contracts.Emails;

namespace Infrastructure.Notifications;

internal sealed class NotificationService(IEmailService emailService) : INotificationService
{
    public Task SendEmailVerificationAsync(EmailVerificationEmail request, CancellationToken cancellationToken = default)
    {
        var mailRequest = new MailRequest(
           request.EmailTo,
           "Verify Your Email Address",
           $"""
            <p>Dear {request.EmailTo},</p>
            <p>Thank you for signing up! To complete your registration, please verify your email address by clicking the link below:</p>
            <p><a href="{request.VerificationLink}" target="_blank" style="color: #007bff; text-decoration: none;">Verify Email</a></p>
            <p>If you didn't sign up, you can safely ignore this email.</p>
            <p>Best regards,<br>The Team</p>
            """);

        return emailService.SendEmailAsync(mailRequest, true, cancellationToken);
    }

    public Task SendPasswordChangedAsync(PasswordChangedEmail request, CancellationToken cancellationToken = default)
    {
        var mailRequest = new MailRequest(
            request.EmailTo,
            "Your Password Has Been Changed",
            $"""
            <p>Dear {request.EmailTo},</p>
            <p>We wanted to let you know that your password was recently changed. If this was you, no further action is required.</p>
            <p>If you did not make this change, please reset your password immediately or contact our support team.</p>
            <p>Stay secure,<br>The Team</p>
            """);

        return emailService.SendEmailAsync(mailRequest, true, cancellationToken);
    }

    public Task SendPurchaseConfirmedAsync(PurchaseConfirmedEmail request, CancellationToken cancellationToken = default)
    {
        var mailRequest = new MailRequest(
           request.EmailTo,
           "Your Purchase Has Been Confirmed!",
           $"""
           <p>Dear {request.EmailTo},</p>
           <p>Thank you for your purchase! Your order has been successfully processed.</p>
           <p><strong>Order Details:</strong></p>
           <ul>
               <li>Order Number: {Guid.CreateVersion7()}</li>
               <li>Amount: {request.Amount:C}</li>
               <li>Date: {request.PurchaseDate:MMMM dd, yyyy}</li>
           </ul>
           <p>You can track your order status in your account.</p>
           <p>Thanks for shopping with us!<br>The Team</p>
           """);

        return emailService.SendEmailAsync(mailRequest, true, cancellationToken);
    }

    public Task SendWelcomeAsync(WelcomeEmail request, CancellationToken cancellationToken = default)
    {
        var mailRequest = new MailRequest(
            request.EmailTo,
            "Welcome to Our Service!",
            $"""
            <p>Dear {request.EmailTo},</p>
            <p>Welcome to our platform! We're excited to have you on board.</p>
            <p>To get started, log in to your account and explore everything we have to offer.</p>
            <p>If you have any questions, feel free to reach out to our support team.</p>
            <p>Happy exploring!<br>The Team</p>
            """);

        return emailService.SendEmailAsync(mailRequest, true, cancellationToken);
    }
}

namespace Contracts.Emails;

public sealed record PurchaseConfirmedEmail(string EmailTo, decimal PurchasePrice, int Quantity, DateTime Date);

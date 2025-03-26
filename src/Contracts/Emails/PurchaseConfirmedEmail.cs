namespace Contracts.Emails;

public sealed record PurchaseConfirmedEmail(string EmailTo, decimal Amount, int Quantity, DateTime PurchaseDate);

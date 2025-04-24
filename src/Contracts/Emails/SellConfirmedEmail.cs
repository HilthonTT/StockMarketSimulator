namespace Contracts.Emails;

public sealed record SellConfirmedEmail(string EmailTo, string Ticker, decimal TotalAmountEarned, DateTime SaleDate);

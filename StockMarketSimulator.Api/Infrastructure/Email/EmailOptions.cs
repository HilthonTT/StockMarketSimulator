namespace StockMarketSimulator.Api.Infrastructure.Email;

public sealed class EmailOptions
{
    public const string ConfigurationSection = "EmailOptions";

    public string SenderEmail { get; set; } = string.Empty;

    public string Sender { get; set; } = string.Empty;

    public string Host { get; set; } = string.Empty;

    public int Port { get; set; }
}

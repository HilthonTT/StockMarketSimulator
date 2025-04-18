namespace Infrastructure.Events.Options;

public sealed class MessageBrokerOptions
{
    public const string SettingsKey = "MessageBroker";

    public string QueueName { get; set; } = string.Empty;
}

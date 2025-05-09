namespace Infrastructure.Database.Options;

public sealed class DatabaseOptions
{
    public const string SettingsKey = "Database";

    public int MaxRetryCount { get; init; }

    public int CommandTimeout { get; init; }

    public bool EnableDetailedErrors { get; init; }

    public bool EnableSensitiveDataLogging { get; init; }
}

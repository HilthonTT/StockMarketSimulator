namespace Infrastructure.Storage.Options;

internal sealed class BlobOptions
{
    public const string SettingsKey = "Blobs";

    public string ContainerName { get; init; } = string.Empty;
}

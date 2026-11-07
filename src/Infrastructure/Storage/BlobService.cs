using Application.Abstractions.Storage;
using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Infrastructure.Storage.Options;
using Microsoft.Extensions.Options;
using SharedKernel;

namespace Infrastructure.Storage;

internal sealed class BlobService : IBlobService
{
    private const string OwnerMetadataKey = "owner_id";

    private readonly BlobContainerClient _containerClient;

    public BlobService(
        BlobServiceClient blobServiceClient,
        IOptions<BlobOptions> options)
    {
        _containerClient = blobServiceClient.GetBlobContainerClient(options.Value.ContainerName);
        _containerClient.CreateIfNotExists();
    }

    public async Task DeleteAsync(Guid fileId, CancellationToken cancellationToken = default)
    {
        BlobClient blobClient = _containerClient.GetBlobClient(fileId.ToString());

        await blobClient.DeleteIfExistsAsync(cancellationToken: cancellationToken);
    }

    public async Task<FileResponse> DownloadAsync(Guid fileId, CancellationToken cancellationToken = default)
    {
        BlobClient blobClient = _containerClient.GetBlobClient(fileId.ToString());

        Response<BlobDownloadResult> response = 
            await blobClient.DownloadContentAsync(cancellationToken: cancellationToken);

        return new FileResponse(response.Value.Content.ToStream(), response.Value.Details.ContentType);
    }

    public async Task<Guid?> GetOwnerAsync(Guid fileId, CancellationToken cancellationToken = default)
    {
        BlobClient blobClient = _containerClient.GetBlobClient(fileId.ToString());

        try
        {
            Response<BlobProperties> properties =
                await blobClient.GetPropertiesAsync(cancellationToken: cancellationToken);

            return properties.Value.Metadata.TryGetValue(OwnerMetadataKey, out string? owner) &&
                   Guid.TryParse(owner, out Guid ownerId)
                ? ownerId
                : null;
        }
        catch (RequestFailedException exception) when (exception.Status == 404)
        {
            return null;
        }
    }

    public async Task<Guid> UploadAsync(
        Stream stream,
        string contentType,
        Guid? ownerId = null,
        CancellationToken cancellationToken = default)
    {
        Ensure.NotNull(stream, nameof(stream));
        Ensure.NotNull(contentType, nameof(contentType));

        Guid fileId = Guid.CreateVersion7();
        BlobClient blobClient = _containerClient.GetBlobClient(fileId.ToString());

        var headers = new BlobHttpHeaders { ContentType = contentType };

        var uploadOptions = new BlobUploadOptions
        {
            HttpHeaders = headers,
            Metadata = ownerId is null
                ? null
                : new Dictionary<string, string> { [OwnerMetadataKey] = ownerId.Value.ToString() }
        };

        await blobClient.UploadAsync(stream, uploadOptions, cancellationToken);

        return fileId;
    }
}

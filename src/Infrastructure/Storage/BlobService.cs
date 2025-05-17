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

    public async Task<Guid> UploadAsync(
        Stream stream, 
        string contentType, 
        CancellationToken cancellationToken = default)
    {
        Ensure.NotNull(stream, nameof(stream));
        Ensure.NotNull(contentType, nameof(contentType));

        Guid fileId = Guid.CreateVersion7();
        BlobClient blobClient = _containerClient.GetBlobClient(fileId.ToString());

        var headers = new BlobHttpHeaders { ContentType = contentType };

        await blobClient.UploadAsync(stream, headers, cancellationToken: cancellationToken);

        return fileId;
    }
}

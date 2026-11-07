namespace Application.Abstractions.Storage;

public interface IBlobService
{
    /// <summary>Uploads a file. <paramref name="ownerId"/> is stored with the blob for authorization checks.</summary>
    Task<Guid> UploadAsync(
        Stream stream,
        string contentType,
        Guid? ownerId = null,
        CancellationToken cancellationToken = default);

    /// <summary>Returns the owner recorded at upload time, or <c>null</c> when the file is missing or has no owner.</summary>
    Task<Guid?> GetOwnerAsync(Guid fileId, CancellationToken cancellationToken = default);

    Task<FileResponse> DownloadAsync(Guid fileId, CancellationToken cancellationToken = default);

    Task DeleteAsync(Guid fileId, CancellationToken cancellationToken = default);
}

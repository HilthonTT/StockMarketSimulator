namespace Application.Abstractions.Storage;

public sealed record FileResponse(Stream Stream, string ContentType);

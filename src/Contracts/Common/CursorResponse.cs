namespace Contracts.Common;

public sealed record CursorResponse<T>(Guid? Cursor, T Data);

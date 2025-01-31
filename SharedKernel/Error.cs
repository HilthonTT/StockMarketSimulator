namespace SharedKernel;

public record Error
{
    public static readonly Error None = new(string.Empty, string.Empty, ErrorType.Failure);
    public static readonly Error NullValue = new("General.Null", "Null value was provided", ErrorType.Failure);

    public Error(string code, string description, ErrorType type)
    {
        Code = code; 
        Description = description;
        Type = type;
    }

    public string Code { get; init; }

    public string Description { get; init; }

    public ErrorType Type { get; init; }

    public static Error Failure(string code, string description) => 
        new(code, description, ErrorType.Failure);

    public static Error NotFound(string code, string description) =>
        new(code, description, ErrorType.NotFound);

    public static Error Problem(string code, string description) =>
        new(code, description, ErrorType.Problem);

    public static Error Conflict(string code, string description) =>
        new(code, description, ErrorType.Conflict);

    public static Error Forbidden(string code, string description) =>
        new(code, description, ErrorType.Forbidden);
}

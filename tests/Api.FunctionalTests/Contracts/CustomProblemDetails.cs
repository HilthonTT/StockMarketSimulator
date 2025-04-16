using SharedKernel;

namespace Api.FunctionalTests.Contracts;

internal sealed class CustomProblemDetails
{
    public string Type { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public int Status { get; set; }

    public string Detail { get; set; } = string.Empty;

    public List<Error> Errors { get; set; } = [];
}

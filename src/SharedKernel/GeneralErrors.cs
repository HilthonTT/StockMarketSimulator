namespace SharedKernel;

public static class GeneralErrors
{
    public static readonly Error UnprocessableRequest = Error.Problem(
        "General.UnprocessableRequest",
        "The server could not process the request.");
}

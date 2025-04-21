namespace SharedKernel;

public sealed class UnauthorizedException : Exception
{
    public UnauthorizedException(string error) 
        : base(error)
    {
    }

    public UnauthorizedException() 
        : base("You are not authorized to perform this action.")
    {
    }
}

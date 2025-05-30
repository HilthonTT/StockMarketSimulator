﻿namespace SharedKernel;

public class DomainException : Exception
{
    public DomainException(Error error)
        : base(error.Description)
    {
        Error = error;
    }

    public Error Error { get; }
}

﻿using Modules.Users.Domain.Errors;
using SharedKernel;

namespace Modules.Users.Domain.ValueObjects;

public sealed record Email
{
    private Email() { } // Required by EF Core

    private Email(string value) => Value = value;

    public string Value { get; }

    public static Result<Email> Create(string? email)
    {
        if (string.IsNullOrEmpty(email))
        {
            return Result.Failure<Email>(EmailErrors.Empty);
        }

        if (email.Split('@').Length != 2)
        {
            return Result.Failure<Email>(EmailErrors.InvalidFormat);
        }

        return new Email(email);
    }
}

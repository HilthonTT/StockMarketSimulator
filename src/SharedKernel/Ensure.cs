﻿using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace SharedKernel;

public static class Ensure
{
    public static void NotNullOrEmpty(
        [NotNull] string? value,
        [CallerArgumentExpression(nameof(value))] string? paramName = default)
    {
        if (string.IsNullOrEmpty(value))
        {
            throw new ArgumentNullException(paramName, $"{paramName} cannot be null or empty.");
        }
    }

    public static void NotNullOrEmpty(
        [NotNull] Guid? value,
        [CallerArgumentExpression(nameof(value))] string? paramName = default)
    {
        if (value is null || value == Guid.Empty)
        {
            throw new ArgumentNullException(paramName, $"{paramName} cannot be null or an empty GUID.");
        }
    }

    public static void NotNull(
        [NotNull] object? value,
        [CallerArgumentExpression(nameof(value))] string? paramName = default)
    {
        if (value is null)
        {
            throw new ArgumentNullException(paramName, $"{paramName} cannot be null.");
        }
    }

    public static void GreaterThanOrEqualToZero(
        int value,
        [CallerArgumentExpression(nameof(value))] string? paramName = default)
    {
        if (value < 0)
        {
            throw new ArgumentOutOfRangeException(paramName, "Value must be greater than or equal to zero.");
        }
    }

    public static void GreaterThanOrEqualToZero(
        decimal value,
        [CallerArgumentExpression(nameof(value))] string? paramName = default)
    {
        if (value < 0)
        {
            throw new ArgumentOutOfRangeException(paramName, "Value must be greater than or equal to zero.");
        }
    }

    public static void GreaterThanZero(
        int value,
        [CallerArgumentExpression(nameof(value))] string? paramName = default)
    {
        if (value <= 0)
        {
            throw new ArgumentOutOfRangeException(paramName, "Value must be greater than or equal to zero.");
        }
    }

    public static void GreaterThanZero(
        decimal value,
        [CallerArgumentExpression(nameof(value))] string? paramName = default)
    {
        if (value <= 0)
        {
            throw new ArgumentOutOfRangeException(paramName, "Value must be greater than or equal to zero.");
        }
    }
}

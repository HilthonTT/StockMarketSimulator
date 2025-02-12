using System.Diagnostics.CodeAnalysis;
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
            throw new ArgumentNullException(paramName);
        }
    }

    public static void NotNullOrEmpty(
        [NotNull] Guid? value,
        [CallerArgumentExpression(nameof(value))] string? paramName = default)
    {
        if (value is null || value == Guid.Empty)
        {
            throw new ArgumentNullException(paramName);
        }
    }

    public static void NotNull(
        [NotNull] object? value,
        [CallerArgumentExpression(nameof(value))] string? paramName = default)
    {
        if (value is null) 
        {
            throw new ArgumentNullException(paramName);
        }
    }

    public static void GreaterThanOrEqualToZero(
        int value,
        [CallerArgumentExpression(nameof(value))] string? paramName = default)
    {
        if (value < 0)
        {
            throw new ArgumentOutOfRangeException(paramName);
        }
    }

    public static void GreaterThanOrEqualToZero(
        decimal value,
        [CallerArgumentExpression(nameof(value))] string? paramName = default)
    {
        if (value < 0)
        {
            throw new ArgumentOutOfRangeException(paramName);
        }
    }
}

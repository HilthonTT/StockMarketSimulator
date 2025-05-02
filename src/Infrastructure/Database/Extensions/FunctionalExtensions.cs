using SharedKernel;

namespace Infrastructure.Database.Extensions;

/// <summary>
/// Provides functional-style extension methods for working with collections.
/// </summary>
public static class FunctionalExtensions
{
    /// <summary>
    /// Executes the specified action on each element in the enumerable sequence.
    /// Ensures that neither the source nor the action is null before iteration.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the source collection.</typeparam>
    /// <param name="source">The enumerable collection to iterate over.</param>
    /// <param name="action">The action to perform on each element.</param>
    public static void ForEach<T>(this IEnumerable<T>? source, Action<T>? action)
    {
        Ensure.NotNull(source, nameof(source));
        Ensure.NotNull(action, nameof(action));

        foreach (T obj in source)
        {
            action(obj);
        }
    }
}

namespace SharedKernel;

/// <summary>
/// Provides extension methods for working with <see cref="Result"/> and <see cref="Result{T}"/> 
/// in a functional and composable way.
/// </summary>
public static class ResultExtensions
{
    /// <summary>
    /// Asynchronously matches the result of a <see cref="Task{Result{T}}"/> to one of two outcomes,
    /// executing the appropriate delegate based on success or failure.
    /// </summary>
    /// <typeparam name="T">The type of the result value.</typeparam>
    /// <typeparam name="TOut">The return type of the matching functions.</typeparam>
    /// <param name="resultTask">The task that yields a <see cref="Result{T}"/>.</param>
    /// <param name="onSuccess">The function to execute if the result is successful.</param>
    /// <param name="onFailure">The function to execute if the result is a failure.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The result of the task is the output
    /// from either the <paramref name="onSuccess"/> or <paramref name="onFailure"/> function.
    /// </returns>
    public static async Task<TOut> Match<T, TOut>(
        this Task<Result<T>> resultTask,
        Func<T, TOut> onSuccess,
        Func<Result<T>, TOut> onFailure)
    {
        var result = await resultTask;
        return result.Match(onSuccess, onFailure);
    }

    /// <summary>
    /// Asynchronously matches the result of a <see cref="Task{Result}"/> to one of two outcomes,
    /// executing the appropriate delegate based on success or failure.
    /// </summary>
    /// <typeparam name="TOut">The return type of the matching functions.</typeparam>
    /// <param name="resultTask">The task that yields a <see cref="Result"/>.</param>
    /// <param name="onSuccess">The function to execute if the result is successful.</param>
    /// <param name="onFailure">The function to execute if the result is a failure.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The result of the task is the output
    /// from either the <paramref name="onSuccess"/> or <paramref name="onFailure"/> function.
    /// </returns>
    public static async Task<TOut> Match<TOut>(
        this Task<Result> resultTask,
        Func<TOut> onSuccess,
        Func<Result, TOut> onFailure)
    {
        var result = await resultTask;
        return result.Match(onSuccess, onFailure);
    }

    /// <summary>
    /// Matches the result and executes the appropriate function based on its success or failure.
    /// </summary>
    /// <typeparam name="TOut">The return type of the functions.</typeparam>
    /// <param name="result">The result to match on.</param>
    /// <param name="onSuccess">Function to execute if the result is successful.</param>
    /// <param name="onFailure">Function to execute if the result is a failure.</param>
    /// <returns>The result of the executed function.</returns>
    public static TOut Match<TOut>(
       this Result result,
       Func<TOut> onSuccess,
       Func<Result, TOut> onFailure)
    {
        return result.IsSuccess ? onSuccess() : onFailure(result);
    }

    /// <summary>
    /// Matches the result and executes the appropriate function based on its success or failure.
    /// </summary>
    /// <typeparam name="TIn">The type of the value in the result.</typeparam>
    /// <typeparam name="TOut">The return type of the functions.</typeparam>
    /// <param name="result">The result to match on.</param>
    /// <param name="onSuccess">Function to execute if the result is successful.</param>
    /// <param name="onFailure">Function to execute if the result is a failure.</param>
    /// <returns>The result of the executed function.</returns>
    public static TOut Match<TIn, TOut>(
        this Result<TIn> result,
        Func<TIn, TOut> onSuccess,
        Func<Result<TIn>, TOut> onFailure)
    {
        return result.IsSuccess ? onSuccess(result.Value) : onFailure(result);
    }

    /// <summary>
    /// Matches the success status of the result to the corresponding functions.
    /// </summary>
    /// <typeparam name="TIn">The result type.</typeparam>
    /// <typeparam name="TOut">The output result type.</typeparam>
    /// <param name="resultTask">The result task.</param>
    /// <param name="onSuccess">The on-success function.</param>
    /// <param name="onFailure">The on-failure function.</param>
    /// <returns>
    /// The result of the on-success function if the result is a success result, otherwise the result of the failure result.
    /// </returns>
    public static async Task<TOut> Match<TIn, TOut>(
        this Task<Result<TIn>> resultTask,
        Func<TIn, TOut> onSuccess,
        Func<Error, TOut> onFailure)
    {
        Result<TIn> result = await resultTask;

        return result.IsSuccess ? onSuccess(result.Value) : onFailure(result.Error);
    }

    /// <summary>
    /// Ensures that the specified predicate is true, otherwise returns a failure result with the specified error.
    /// </summary>
    /// <typeparam name="T">The result type.</typeparam>
    /// <param name="result">The result.</param>
    /// <param name="predicate">The predicate.</param>
    /// <param name="error">The error.</param>
    /// <returns>
    /// The success result if the predicate is true and the current result is a success result, otherwise a failure result.
    /// </returns>
    public static Result<T> Ensure<T>(
        this Result<T> result,
        Func<T, bool> predicate,
        Error error)
    {
        if (result.IsFailure)
        {
            return result;
        }

        return predicate(result.Value) ? result : Result.Failure<T>(error);
    }

    /// <summary>
    /// Maps the result value to a new value based on the specified mapping function.
    /// </summary>
    /// <typeparam name="TIn">The result type.</typeparam>
    /// <typeparam name="TOut">The output result type.</typeparam>
    /// <param name="result">The result.</param>
    /// <param name="mappingFunc">The mapping function.</param>
    /// <returns>
    /// The success result with the mapped value if the current result is a success result, otherwise a failure result.
    /// </returns>
    public static Result<TOut> Map<TIn, TOut>(
        this Result<TIn> result,
        Func<TIn, TOut> mappingFunc)
    {
        return result.IsSuccess ?
            Result.Success(mappingFunc(result.Value)) :
            Result.Failure<TOut>(result.Error);
    }

    /// <summary>
    /// Binds to the result of the function and returns it.
    /// </summary>
    /// <typeparam name="TIn">The result type.</typeparam>
    /// <param name="result">The result.</param>
    /// <param name="func">The bind function.</param>
    /// <returns>
    /// The success result with the bound value if the current result is a success result, otherwise a failure result.
    /// </returns>
    public static async Task<Result> Bind<TIn>(this Result<TIn> result, Func<TIn, Task<Result>> func)
    {
        return result.IsSuccess ?
            await func(result.Value) :
            Result.Failure(result.Error);
    }

    /// <summary>
    /// Binds to the result of the function and returns it.
    /// </summary>
    /// <typeparam name="TIn">The result type.</typeparam>
    /// <typeparam name="TOut">The output result type.</typeparam>
    /// <param name="result">The result.</param>
    /// <param name="func">The bind function.</param>
    /// <returns>
    /// The success result with the bound value if the current result is a success result, otherwise a failure result.
    /// </returns>
    public static async Task<Result<TOut>> Bind<TIn, TOut>(this Result<TIn> result, Func<TIn, Task<Result<TOut>>> func)
    {
        return result.IsSuccess ?
            await func(result.Value) :
            Result.Failure<TOut>(result.Error);
    }
}

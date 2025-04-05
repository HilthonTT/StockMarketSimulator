using Microsoft.Extensions.Logging;

namespace Infrastructure.Outbox;

public static partial class OutboxLoggers
{
    [LoggerMessage(Level = LogLevel.Information, Message = "OutboxBackgroundService starting...")]
    public static partial void LogStarting(ILogger logger);

    [LoggerMessage(Level = LogLevel.Information, Message = "Starting iteration {IterationCount}")]
    public static partial void LogStartingIteration(ILogger logger, int iterationCount);

    [LoggerMessage(Level = LogLevel.Information, Message = "Iteration {IterationCount} completed. Processed {ProcessedMessages} messages. Total processed: {TotalProcessedMessages}")]
    public static partial void LogIterationCompleted(ILogger logger, int iterationCount, int processedMessages, int totalProcessedMessages);

    [LoggerMessage(Level = LogLevel.Information, Message = "OutboxBackgroundService operation cancelled.")]
    public static partial void LogOperationCancelled(ILogger logger);

    [LoggerMessage(Level = LogLevel.Error, Message = "An error occurred in OutboxBackgroundService")]
    public static partial void LogError(ILogger logger, Exception exception);

    [LoggerMessage(Level = LogLevel.Information, Message = "OutboxBackgroundService finished. Total iterations: {IterationCount}, Total processed messages: {TotalProcessedMessages}")]
    public static partial void LogFinished(ILogger logger, int iterationCount, int totalProcessedMessages);

    [LoggerMessage(Level = LogLevel.Information, Message = "Outbox processing completed. Total time: {TotalTime}ms, Query time: {QueryTime}ms, Publish time: {PublishTime}ms, Update time: {UpdateTime}ms, Messages processed: {MessageCount}")]
    public static partial void LogProcessingPerformance(ILogger logger, long totalTime, long queryTime, long publishTime, long updateTime, int messageCount);
}

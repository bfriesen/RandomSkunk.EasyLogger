using Microsoft.Extensions.Logging;

using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace RandomSkunk.Logging;

/// <summary>
/// Provides an abstract base class for logging implementions.
/// </summary>
/// <remarks>
/// The <see cref="EasyLogger"/> class is intended for scenarios where performance is not critical, such as unit testing with
/// mocking frameworks like Moq. For production-grade or high-performance logging, consider using
/// <see cref="HighPerformanceLogger"/> instead. Both classes have identical behavior, but <see cref="EasyLogger"/> performs more
/// boxing operations and heap allocations.
/// <para/>Moq Example:
/// <code>
/// Mock&lt;EasyLogger> mockLogger = new();
/// ILogger logger = mockLogger.Object;
/// 
/// logger.Log(LogLevel.Information, 1, "Test message", null, (state, ex) => state.ToString());
/// 
/// mockLogger.Verify(logger => logger.Write(It.Is&lt;ILogEntry>(log =>
///     log.IsInformation() &amp;&amp; log.HasMessage("Test message"))));
/// </code>
/// </remarks>
public abstract class EasyLogger : LoggerBase, ILogger
{
    /// <summary>
    /// When overridden in a derived class, writes the specified log entry.
    /// </summary>
    /// <param name="logEntry">The log entry to write.</param>
    public abstract void Write(ILogEntry logEntry);

    /// <inheritdoc/>
    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        if (IsEnabled(logLevel))
        {
            CreateLogEntry(logLevel, in eventId, in state, exception, formatter, out LogEntry<TState> logEntry);
            Write(logEntry);
        }
    }
}

/// <summary>
/// Provides an abstract base class for logging implementions.
/// </summary>
/// <remarks>
/// The <see cref="EasyLogger{TCategoryName}"/> class is intended for scenarios where performance is not critical, such as unit
/// testing with mocking frameworks like Moq. For production-grade or high-performance logging, consider using
/// <see cref="HighPerformanceLogger{TCategoryName}"/> instead. Both classes have identical behavior, but
/// <see cref="EasyLogger{TCategoryName}"/> performs more boxing operations and heap allocations.
/// <para/>Moq Example:
/// <code>
/// Mock&lt;EasyLogger&lt;Example>> mockLogger = new();
/// ILogger&lt;Example> logger = mockLogger.Object;
/// 
/// logger.Log(LogLevel.Information, 1, "Test message", null, (state, ex) => state.ToString());
/// 
/// mockLogger.Verify(logger => logger.Write(It.Is&lt;ILogEntry>(log =>
///     log.IsInformation() &amp;&amp; log.HasMessage("Test message"))));
/// </code>
/// </remarks>
public abstract class EasyLogger<TCategoryName> : EasyLogger, ILogger<TCategoryName>
{
}

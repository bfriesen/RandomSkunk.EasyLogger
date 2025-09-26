using Microsoft.Extensions.Logging;

namespace RandomSkunk.Logging;

/// <summary>
/// Provides an abstract base class for logging implementations.
/// </summary>
/// <remarks>
/// The <see cref="HighPerformanceLogger"/> class is intended for production scenarios where performance is critical. For
/// scenarios where performance is not a primary concern, such as unit testing with mocking frameworks like Moq, consider using
/// <see cref="EasyLogger"/> instead. Both classes have identical behavior, but <see cref="HighPerformanceLogger"/> performs
/// fewer boxing operations and heap allocations.
/// </remarks>
public abstract class HighPerformanceLogger : LoggerBase, ILogger
{
    /// <summary>
    /// Initializes a new instance of the <see cref="HighPerformanceLogger"/> class.
    /// </summary>
    protected HighPerformanceLogger()
        : base()
    {
    }

    private protected HighPerformanceLogger(string category)
        : base(category)
    {        
    }

    /// <summary>
    /// When overridden in a derived class, writes the specified log entry.
    /// </summary>
    /// <param name="logEntry">The log entry to write.</param>
    public abstract void Write<TState>(in LogEntry<TState> logEntry);

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
            Write(in logEntry);
        }
    }
}

/// <summary>
/// Provides an abstract base class for logging implementations.
/// </summary>
/// <remarks>
/// The <see cref="HighPerformanceLogger{TCategoryName}"/> class is intended for production scenarios where performance is
/// critical. For scenarios where performance is not a primary concern, such as unit testing with mocking frameworks like Moq,
/// consider using <see cref="EasyLogger{TCategoryName}"/> instead. Both classes have identical behavior,
/// but <see cref="HighPerformanceLogger{TCategoryName}"/> performs fewer boxing operations and heap allocations.
/// </remarks>
/// <typeparam name="TCategoryName">The type whose name is used for the logger category name.</typeparam>
public abstract class HighPerformanceLogger<TCategoryName> : HighPerformanceLogger, ILogger<TCategoryName>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="HighPerformanceLogger{TCategoryName}"/> class.
    /// </summary>
    protected HighPerformanceLogger()
        : base(typeof(TCategoryName).ToString())
    {
    }
}

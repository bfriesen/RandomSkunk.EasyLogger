using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace RandomSkunk.EasyLogging;

/// <summary>
/// A simple, low-allocation logger.
/// </summary>
public class EasyLogger : ILogger
{
    private readonly AsyncLocal<Scope?> _currentScope = new();

    private LogLevel _minimumLogLevel = LogLevel.Information;

    /// <summary>
    /// Gets or sets the minimum log level that the logger should write.
    /// </summary>
    /// <remarks>Default value is <see cref="LogLevel.Information"/>.</remarks>
    public LogLevel LogLevel
    {
        get => _minimumLogLevel;
        set
        {
            if (value < LogLevel.Trace || value > LogLevel.None)
                throw new ArgumentOutOfRangeException(nameof(value));

            _minimumLogLevel = value;
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether scopes should be included in log entries.
    /// </summary>
    /// <remarks>Default value is <see langword="true"/>.</remarks>
    public bool IncludeScopes { get; init; } = true;

    /// <summary>
    /// Gets a collection that represents the logger's current scope stack.
    /// </summary>
    public IEnumerable<object> CurrentScope
    {
        get
        {
            for (var scope = _currentScope.Value; scope is not null; scope = scope.ParentScope)
                yield return scope.State;
        }
    }

    /// <summary>
    /// When overridden in a derived class, writes the specified log entry. The base virtual method
    /// does nothing.
    /// </summary>
    /// <remarks>
    /// This method is called by <see cref="EasyLogger"/>'s <see cref="Log"/> method after
    /// verifying that <see cref="IsEnabled"/> is <see langword="true"/> for the given log level.
    /// </remarks>
    /// <param name="logEntry">The log entry to write.</param>
    public virtual void WriteLogEntry(LogEntry logEntry)
    {
    }

    /// <inheritdoc/>
    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        if (!((ILogger)this).IsEnabled(logLevel))
            return;

        var getMessage = () => formatter(state, exception);
        var attributes = new LogAttributes(state, _currentScope.Value);
        var logEntry = new LogEntry(logLevel, eventId, getMessage, attributes, exception);
        WriteLogEntry(logEntry);
    }

    /// <inheritdoc/>
    public bool IsEnabled(LogLevel logLevel) =>
        logLevel >= _minimumLogLevel && logLevel < LogLevel.None;

    /// <summary>
    /// Begins a logical operation scope by pushing the specified state onto the logger's scope
    /// stack.
    /// </summary>
    /// <typeparam name="TState">The type of the state to begin scope for.</typeparam>
    /// <param name="state"></param>
    /// <returns>An object that, when disposed, pops the state off the logger's scope stack.</returns>
    public IDisposable? BeginScope<TState>(TState state)
        where TState : notnull
    {
        ArgumentNullException.ThrowIfNull(state);

        if (IncludeScopes)
            return _currentScope.Value = new Scope(state, this);

        return null;
    }

    private void EndScope(Scope scope)
    {
        Debug.Assert(IncludeScopes);

        // Gracefully handle the possibility of scopes disposing out of order.
        while (_currentScope.Value is not null)
        {
            try
            {
                if (ReferenceEquals(_currentScope.Value, scope))
                    break;
            }
            finally
            {
                _currentScope.Value = _currentScope.Value.ParentScope;
            }
        }
    }

    private sealed class Scope(object state, EasyLogger logger)
        : ILoggerScope, IDisposable
    {
        public object State { get; } = state;

        public Scope? ParentScope { get; } = logger._currentScope.Value;

        ILoggerScope? ILoggerScope.ParentScope => ParentScope;

        public void Dispose() => logger.EndScope(this);
    }
}

/// <summary>
/// A simple, low-allocation logger.
/// </summary>
/// <typeparam name="TCategoryName">The type whose name is used for the logger category name.
/// </typeparam>
public class EasyLogger<TCategoryName> : EasyLogger, ILogger<TCategoryName>
{
}

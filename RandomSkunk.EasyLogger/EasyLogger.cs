using System.Diagnostics;
using Microsoft.Extensions.Logging;

using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace RandomSkunk.Logging;

/// <summary>
/// An implementation of the <see cref="ILogger"/> interface. It is designed to fulfill the following requirements:
/// <list type="bullet">
///     <item>
///         It should make setup and verification of a mock <see cref="ILogger"/> easy, regardless of the mocking library.
///     </item>
///     <item>
///         As a base class for a custom <see cref="ILogger"/>, it should be both easy to implement and easy to understand
///         <em>how</em> to implement it.
///     </item>
///     <item>
///         It should correctly implement logging scopes and make this information easily available to a test or custom logger
///         implementation.
///     </item>
///     <item>
///         It should have minimal impact on performance.
///     </item>
/// </list>
/// </summary>
public abstract class EasyLogger : ILogger
{
    private readonly AsyncLocal<Scope?> _currentScope = new();

    private LogLevel _minimumLogLevel = LogLevel.Information;
    private bool _includeScopes = true;

    /// <summary>
    /// Gets or sets the minimum log level that the logger should write. Default value is <see cref="LogLevel.Information"/>.
    /// </summary>
    public LogLevel MinimumLogLevel
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
    /// Gets or sets a value indicating whether scopes will be included in log entries. Default value is <see langword="true"/>.
    /// </summary>
    public bool IncludeScopes
    {
        get => _includeScopes;
        set => _includeScopes = value;
    }

    /// <summary>
    /// Gets a collection that represents the logger's current scope stack.
    /// </summary>
    public IEnumerable<object> CurrentScope
    {
        get
        {
            if (!_includeScopes)
                yield break;

            for (var scope = _currentScope.Value; scope is not null; scope = scope.ParentScope)
                yield return scope.State;
        }
    }

    /// <summary>
    /// When overridden in a derived class, writes the specified log entry.
    /// </summary>
    /// <remarks>
    /// This method is called by the logger's <see cref="ILogger.Log"/> method after verifying that <see cref="IsEnabled"/> is
    /// <see langword="true"/> for the given log level.
    /// </remarks>
    /// <param name="logEntry">The log entry to write.</param>
    public abstract void Write(LogEntry logEntry);

    /// <summary>
    /// If the specified log level is enabled according to the <see cref="IsEnabled"/> method, writes a log entry according to
    /// the implementation of the abstract <see cref="Write"/> method.
    /// </summary>
    /// <param name="logLevel">Entry will be written on this level.</param>
    /// <param name="eventId">Id of the event.</param>
    /// <param name="state">The entry to be written. Can be also an object.</param>
    /// <param name="exception">The exception related to this entry.</param>
    /// <param name="formatter">Function to create a <see cref="string"/> message of the <paramref name="state"/> and
    ///     <paramref name="exception"/>.</param>
    /// <typeparam name="TState">The type of the object to be written.</typeparam>
    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        if (((ILogger)this).IsEnabled(logLevel))
        {
            var getMessage = () => formatter(state, exception);
            var currentScope = _includeScopes ? _currentScope.Value : null;
            var attributes = new LogAttributes(state, currentScope);
            var logEntry = new LogEntry(logLevel, eventId, getMessage, attributes, exception);
            Write(logEntry);
        }
    }

    /// <inheritdoc/>
    public bool IsEnabled(LogLevel logLevel) =>
        logLevel >= _minimumLogLevel && logLevel < LogLevel.None;

    /// <summary>
    /// Begins a logical operation scope by pushing the specified state onto the logger's scope stack.
    /// </summary>
    /// <typeparam name="TState">The type of the state to begin scope for.</typeparam>
    /// <param name="state"></param>
    /// <returns>An object that, when disposed, pops the state off the logger's scope stack.</returns>
    public IDisposable? BeginScope<TState>(TState state)
        where TState : notnull
    {
        if (TypeOf<TState>.IsReferenceType)
            ThrowIfNull(state);

        if (_includeScopes)
            return _currentScope.Value = new Scope(state, this);

        return null;
    }

    private void EndScope(Scope scope)
    {
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
/// An implementation of the <see cref="ILogger{TCategoryName}"/> interface. It is designed to fulfill the following
/// requirements:
/// <list type="bullet">
///     <item>
///         It should make setup and verification of a mock <see cref="ILogger{TCategoryName}"/> easy, regardless of the mocking
///         library.
///     </item>
///     <item>
///         As a base class for a custom <see cref="ILogger{TCategoryName}"/>, it should be both easy to implement and easy to
///         understand <em>how</em> to implement it.
///     </item>
///     <item>
///         It should correctly implement logging scopes and make this information easily available to a test or custom logger
///         implementation.
///     </item>
///     <item>
///         It should have minimal impact on performance.
///     </item>
/// </list>
/// </summary>
/// <typeparam name="TCategoryName">The type whose name is used for the logger category name.</typeparam>
public abstract class EasyLogger<TCategoryName> : EasyLogger, ILogger<TCategoryName>
{
}

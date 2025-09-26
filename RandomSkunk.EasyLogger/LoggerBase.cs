using Microsoft.Extensions.Logging;

namespace RandomSkunk.Logging;

/// <summary>
/// Provides an abstract base class for logger implementations that manage scopes, categories, and log levels.
/// </summary>
public abstract class LoggerBase
{
    private readonly AsyncLocal<Scope?> _currentScope = new();

    private string _category;
    private bool _includeScopes = true;
    private LogLevel _minimumLogLevel = LogLevel.Information;

    /// <summary>
    /// Initializes a new instance of the <see cref="EasyLogger"/> class.
    /// </summary>
    protected LoggerBase() => _category = GetType().ToString();

    /// <summary>
    /// Initializes a new instance of the <see cref="LoggerBase"/> class with the specified category.
    /// </summary>
    /// <param name="category">The category name associated with the logger.</param>
    protected LoggerBase(string category) => _category = category ?? throw new ArgumentNullException(nameof(category));

    /// <summary>
    /// Gets or sets the category name for the logger.
    /// </summary>
    public string Category
    {
        get => _category;
        set
        {
            ArgumentNullException.ThrowIfNull(value);
            _category = value;
        }
    }

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
    public ScopeCollection CurrentScope => new(_includeScopes ? _currentScope.Value : null);

    /// <inheritdoc/>
    public bool IsEnabled(LogLevel logLevel) => logLevel >= _minimumLogLevel && logLevel < LogLevel.None;

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
            ArgumentNullException.ThrowIfNull(state);

        if (_includeScopes)
            return _currentScope.Value = new Scope(state, this);

        return null;
    }

    /// <summary>
    /// Creates a new log entry with the specified log level, event ID, state, exception, and formatter.
    /// </summary>
    /// <typeparam name="TState">The type of the state object associated with the log entry.</typeparam>
    /// <param name="logLevel">The severity level of the log entry.</param>
    /// <param name="eventId">The identifier for the event being logged.</param>
    /// <param name="state">The state object containing contextual information for the log entry.</param>
    /// <param name="exception">The exception associated with the log entry, or <see langword="null"/> if no exception is associated.</param>
    /// <param name="formatter">A function that formats the state and exception into a log message.</param>
    /// <param name="logEntry">When this method returns, contains the newly created log entry.</param>
    /// <returns>A <see cref="LogEntry{TState}"/> representing the created log entry.</returns>
    protected void CreateLogEntry<TState>(
        LogLevel logLevel,
        in EventId eventId,
        in TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter,
        out LogEntry<TState> logEntry)
    {
        Scope? scope = _includeScopes ? _currentScope.Value : null;
        LogAttributes<TState> attributes = new(in state, scope);
        logEntry = new(_category, logLevel, in eventId, exception, formatter, in attributes);
    }

    private void EndScope(Scope disposingScope)
    {
        // Gracefully handle the possibility of scopes disposing out of order.
        while (_currentScope.Value is Scope scope)
        {
            try
            {
                if (ReferenceEquals(scope, disposingScope))
                    break;
            }
            finally
            {
                _currentScope.Value = scope.ParentScope;
            }
        }
    }

    private sealed class Scope(object state, LoggerBase logger)
        : ILoggerScope, IDisposable
    {
        public object State { get; } = state;

        public Scope? ParentScope { get; } = logger._currentScope.Value;

        ILoggerScope? ILoggerScope.ParentScope => ParentScope;

        public void Dispose() => logger.EndScope(this);
    }
}

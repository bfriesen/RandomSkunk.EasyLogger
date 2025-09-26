using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.RegularExpressions;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace RandomSkunk.Logging;

/// <summary>
/// Defines a log event.
/// </summary>
/// <typeparam name="TState">The type of the object to be written.</typeparam>
[DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
public readonly struct LogEntry<TState> : ILogEntry
{
    private readonly LogLevel _logLevel;
    private readonly EventId _eventId;
    private readonly Exception? _exception;
    private readonly Func<TState, Exception?, string>? _formatter;
    private readonly LogAttributes<TState> _attributes;

    /// <summary>
    /// Initializes a new instance of the <see cref="LogEntry{TState}"/> struct.
    /// </summary>
    /// <param name="logLevel">Entry will be written on this level.</param>
    /// <param name="eventId">Id of the event.</param>
    /// <param name="exception">The exception related to this entry.</param>
    /// <param name="formatter">Function to create a <see cref="string"/> message of the <c>state</c> and <c>exception</c>.</param>
    /// <param name="attributes">A collection of key/value pairs that describe the state and scope of the log entry.</param>
    public LogEntry(
        LogLevel logLevel,
        in EventId eventId,
        Exception? exception,
        Func<TState, Exception?, string> formatter,
        in LogAttributes<TState> attributes)
    {
        ArgumentNullException.ThrowIfNull(formatter);

        _logLevel = logLevel;
        _eventId = eventId;
        _exception = exception;
        _formatter = formatter;
        _attributes = attributes;
    }

    /// <inheritdoc/>
    public LogLevel LogLevel => _logLevel;

    /// <inheritdoc/>
    public EventId EventId => _eventId;

    /// <inheritdoc/>
    public string Message => _formatter?.Invoke(_attributes.State, _exception)!;

    /// <inheritdoc/>
    public Exception? Exception => _exception;

    /// <summary>
    /// Gets a collection of key/value pairs derived from the state and scope of the log entry.
    /// </summary>
    public LogAttributes<TState> Attributes => _attributes;

    /// <inheritdoc/>
    IEnumerable<KeyValuePair<string, object>> ILogEntry.Attributes => Attributes;

    /// <summary>
    /// Gets the log entry state.
    /// </summary>
    public TState State => _attributes.State;

    /// <inheritdoc/>
    object? ILogEntry.State => State;

    /// <inheritdoc/>
    public ScopeCollection Scope => new(_attributes.Scope);

    /// <inheritdoc/>
    public bool IsTrace() => HasLogLevel(LogLevel.Trace);

    /// <inheritdoc/>
    public bool IsDebug() => HasLogLevel(LogLevel.Debug);

    /// <inheritdoc/>
    public bool IsInformation() => HasLogLevel(LogLevel.Information);

    /// <inheritdoc/>
    public bool IsWarning() => HasLogLevel(LogLevel.Warning);

    /// <inheritdoc/>
    public bool IsError() => HasLogLevel(LogLevel.Error);

    /// <inheritdoc/>
    public bool IsCritical() => HasLogLevel(LogLevel.Critical);

    /// <inheritdoc/>
    public bool HasLogLevel(LogLevel expectedLogLevel) => _logLevel == expectedLogLevel;

    /// <inheritdoc/>
    public bool HasLogLevel(Func<LogLevel, bool> logLevelPredicate)
    {
        ArgumentNullException.ThrowIfNull(logLevelPredicate);

        return logLevelPredicate(_logLevel);
    }

    /// <inheritdoc/>
    public bool HasEventId(EventId eventId) => _eventId == eventId;

    /// <inheritdoc/>
    public bool HasEventId(Func<EventId, bool> eventIdPredicate)
    {
        ArgumentNullException.ThrowIfNull(eventIdPredicate);

        return eventIdPredicate(_eventId);
    }

    /// <inheritdoc/>
    public bool HasMessage(string expectedMessage)
    {
        ArgumentNullException.ThrowIfNull(expectedMessage);

        return Message is string message && string.Equals(message, expectedMessage);
    }

    /// <inheritdoc/>
    public bool HasMessage(string expectedMessage, StringComparison stringComparison)
    {
        ArgumentNullException.ThrowIfNull(expectedMessage);

        return Message is string message && string.Equals(message, expectedMessage, stringComparison);
    }

    /// <inheritdoc/>
    public bool HasMessage(Func<string, bool> messagePredicate)
    {
        ArgumentNullException.ThrowIfNull(messagePredicate);

        return Message is string message && messagePredicate(message);
    }

    /// <inheritdoc/>
    public bool HasMessageMatching([StringSyntax(nameof(Regex))] string regexPattern)
    {
        ArgumentNullException.ThrowIfNull(regexPattern);

        return Message is string message && Regex.IsMatch(message, regexPattern);
    }

    /// <inheritdoc/>
    public bool HasMessageMatching(
        [StringSyntax(nameof(Regex))] string regexPattern,
        RegexOptions regexOptions)
    {
        ArgumentNullException.ThrowIfNull(regexPattern);

        return Message is string message && Regex.IsMatch(message, regexPattern, regexOptions);
    }

    /// <inheritdoc/>
    public bool HasAttribute(string key)
    {
        ArgumentNullException.ThrowIfNull(key);

        foreach (var attribute in _attributes)
        {
            if (attribute.Key == key)
                return true;
        }

        return false;
    }

    /// <inheritdoc/>
    public bool HasAttribute(string key, object value)
    {
        ArgumentNullException.ThrowIfNull(key);
        ArgumentNullException.ThrowIfNull(value);

        foreach (var attribute in _attributes)
        {
            if (attribute.Key == key && Equals(attribute.Value, value))
                return true;
        }

        return false;
    }

    /// <inheritdoc/>
    public bool HasAttribute<T>(string key, T value)
        where T : notnull
    {
        ArgumentNullException.ThrowIfNull(key);
        ArgumentNullException.ThrowIfNull(value);

        foreach (var attribute in _attributes)
        {
            if (attribute.Key == key && attribute.Value is T tValue && Equals(tValue, value))
                return true;
        }

        return false;
    }

    /// <inheritdoc/>
    public bool HasAttribute(string key, Func<object, bool> valuePredicate)
    {
        ArgumentNullException.ThrowIfNull(key);
        ArgumentNullException.ThrowIfNull(valuePredicate);

        foreach (var attribute in _attributes)
        {
            if (attribute.Key == key && valuePredicate(attribute.Value))
                return true;
        }

        return false;
    }

    /// <inheritdoc/>
    public bool HasAttribute<T>(string key, Func<T, bool> valuePredicate)
        where T : notnull
    {
        ArgumentNullException.ThrowIfNull(key);
        ArgumentNullException.ThrowIfNull(valuePredicate);

        foreach (var attribute in _attributes)
        {
            if (attribute.Key == key && attribute.Value is T tValue && valuePredicate(tValue))
                return true;
        }

        return false;
    }

    /// <inheritdoc/>
    public bool HasNoState() => _attributes.State is null;

    /// <inheritdoc/>
    public bool HasState() => _attributes.State is not null;

    /// <inheritdoc/>
    public bool HasState(object? expectedState)
    {
        if (expectedState is null)
            return _attributes.State is null;

        return Equals(_attributes.State, expectedState);
    }

    /// <inheritdoc/>
    public bool HasState<T>(T? expectedState)
    {
        if (expectedState is null)
            return _attributes.State is null;

        return _attributes.State is T state && Equals(state, expectedState);
    }

    /// <inheritdoc/>
    public bool HasState(Func<object?, bool> statePredicate)
    {
        ArgumentNullException.ThrowIfNull(statePredicate);

        return statePredicate(_attributes.State);
    }

    /// <inheritdoc/>
    public bool HasState<T>(Func<T, bool> statePredicate)
    {
        ArgumentNullException.ThrowIfNull(statePredicate);

        return _attributes.State is T state && statePredicate(state);
    }

    /// <inheritdoc/>
    public bool HasNoScope() => _attributes.Scope is null;

    /// <inheritdoc/>
    public bool HasScope() => _attributes.Scope is not null;

    /// <inheritdoc/>
    public bool HasScope(object? expectedScope)
    {
        if (expectedScope is null)
            return _attributes.Scope is null;

        for (var scope = _attributes.Scope; scope is not null; scope = scope.ParentScope)
        {
            if (Equals(scope.State, expectedScope))
                return true;
        }

        return false;
    }

    /// <inheritdoc/>
    public bool HasScope<T>(T? expectedScope)
    {
        if (expectedScope is null)
            return _attributes.Scope is null;

        for (var scope = _attributes.Scope; scope is not null; scope = scope.ParentScope)
        {
            if (scope.State is T state && Equals(state, expectedScope))
                return true;
        }

        return false;
    }

    /// <inheritdoc/>
    public bool HasScope(Func<object, bool> scopePredicate)
    {
        ArgumentNullException.ThrowIfNull(scopePredicate);

        for (var scope = _attributes.Scope; scope is not null; scope = scope.ParentScope)
        {
            if (scopePredicate(scope.State))
                return true;
        }

        return false;
    }

    /// <inheritdoc/>
    public bool HasScope<T>(Func<T, bool> scopePredicate)
        where T : notnull
    {
        ArgumentNullException.ThrowIfNull(scopePredicate);

        for (var scope = _attributes.Scope; scope is not null; scope = scope.ParentScope)
        {
            if (scope.State is T state && scopePredicate(state))
                return true;
        }

        return false;
    }

    /// <inheritdoc/>
    public bool HasNoException() => _exception is null;

    /// <inheritdoc/>
    public bool HasException() => _exception is not null;

    /// <inheritdoc/>
    public bool HasException<TException>()
        where TException : Exception => _exception is TException;

    /// <inheritdoc/>
    public bool HasException(Exception? expectedException) =>
        ReferenceEquals(_exception, expectedException);

    /// <inheritdoc/>
    public bool HasException(Func<Exception?, bool> exceptionPredicate)
    {
        ArgumentNullException.ThrowIfNull(exceptionPredicate);

        return exceptionPredicate(_exception);
    }

    /// <inheritdoc/>
    public bool HasException<TException>(Func<TException, bool> exceptionPredicate)
        where TException : Exception
    {
        ArgumentNullException.ThrowIfNull(exceptionPredicate);

        return _exception is TException tException && exceptionPredicate(tException);
    }

    /// <summary>
    /// Returns a string that represents the current log entry.
    /// </summary>
    /// <returns>A string that represents the current log entry.</returns>
    public override string ToString()
    {
        var sb = StringBuilderPool.Get();
        Append(sb, _logLevel, _eventId, Message, _attributes.State, _attributes.Scope, _exception);
        return sb.ReturnToPool();
    }

    private string GetDebuggerDisplay()
    {
        var sb = new StringBuilder();
        Append(sb, _logLevel, _eventId, Message, _attributes.State, _attributes.Scope, _exception);
        return sb.ToString();
    }

    private static void Append(
        StringBuilder sb,
        LogLevel logLevel,
        EventId eventId,
        string? message,
        object? state,
        ILoggerScope? loggerScope,
        Exception? exception)
    {
        sb.Append('{').AppendLine().Append(" LogLevel = ").Append(logLevel);

        if (eventId.Id != 0)
        {
            sb.Append(',').AppendLine().Append(" EventId = ").Append(eventId.Id);
            if (!string.IsNullOrWhiteSpace(eventId.Name))
                sb.Append(" (").Append(eventId.Name).Append(')');
        }

        if (!string.IsNullOrWhiteSpace(message))
            sb.Append(',').AppendLine().Append(" Message = ").Append(message);
        else
            message = null; // Normalize empty and whitespace message strings to null.

        if (state is not null)
            sb.Append(',').AppendLine().Append(" State = ").AppendState(state, skipStateStringIfEqualTo: message);

        if (loggerScope is not null)
        {
            for (var scope = loggerScope; scope is not null; scope = scope.ParentScope)
            {
                if (ReferenceEquals(scope, loggerScope))
                    sb.Append(',').AppendLine().Append(" Scope = ").AppendState(scope.State);
                else
                    sb.Append(',').AppendLine().Append(" ParentScope = ").AppendState(scope.State);
            }
        }

        if (exception is not null)
            sb.Append(',').AppendLine().Append(" Exception = ").Append(exception);

        sb.Append(' ').AppendLine().Append('}');
    }
}

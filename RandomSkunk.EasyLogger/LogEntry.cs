using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;

using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace RandomSkunk.Logging;

/// <summary>
/// Defines a log event.
/// </summary>
[DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
public readonly struct LogEntry
{
    private readonly LogLevel _logLevel;
    private readonly EventId _eventId;
    private readonly Func<string> _getMessage;
    private readonly LogAttributes _attributes;
    private readonly Exception? _exception;

    /// <summary>
    /// Initializes a new instance of the <see cref="LogEntry"/> struct.
    /// </summary>
    /// <param name="logLevel">The log level.</param>
    /// <param name="eventId">The Id of the log entry.</param>
    /// <param name="getMessage">The function that gets the message of the log entry.</param>
    /// <param name="attributes">A collection of key/value pairs that describe the state and scope of the log entry.</param>
    /// <param name="exception">The exception related to the log entry.</param>
    /// <exception cref="ArgumentNullException">If <paramref name="getMessage"/> is <see langword="null"/>.</exception>
    public LogEntry(
        LogLevel logLevel,
        EventId eventId,
        Func<string> getMessage,
        LogAttributes attributes,
        Exception? exception)
    {
        ThrowIfNull(getMessage);

        _logLevel = logLevel;
        _eventId = eventId;
        _getMessage = getMessage;
        _attributes = attributes;
        _exception = exception;
    }

    /// <summary>
    /// Gets the log level.
    /// </summary>
    public LogLevel LogLevel => _logLevel;

    /// <summary>
    /// Gets the log event ID.
    /// </summary>
    public EventId EventId => _eventId;

    /// <summary>
    /// Gets the function that gets the log message.
    /// </summary>
    public Func<string> GetMessage => _getMessage;

    /// <summary>
    /// Gets the collection of key/value pairs derived from the state and scope of the log.
    /// </summary>
    public LogAttributes Attributes => _attributes;

    /// <summary>
    /// Gets the log exception.
    /// </summary>
    public Exception? Exception => _exception;

    /// <summary>
    /// Gets the state.
    /// </summary>
    public object? State => _attributes.State;

    /// <summary>
    /// A collection of objects that represent a logger's current scope at the time of the log. The first object in the
    /// collection represents the logger's current scope, the second object represents its parent scope, the third represents its
    /// grandparent scope, and so on.
    /// </summary>
    public IEnumerable<object> Scope
    {
        get
        {
            for (var scope = _attributes.Scope; scope is not null; scope = scope.ParentScope)
                yield return scope.State;
        }
    }

    /// <summary>
    /// Whether the log entry was made at <see cref="LogLevel.Trace"/>.
    /// </summary>
    public bool IsTrace() => HasLogLevel(LogLevel.Trace);

    /// <summary>
    /// Whether the log entry was made at <see cref="LogLevel.Debug"/>.
    /// </summary>
    public bool IsDebug() => HasLogLevel(LogLevel.Debug);

    /// <summary>
    /// Whether the log entry was made at <see cref="LogLevel.Information"/>.
    /// </summary>
    public bool IsInformation() => HasLogLevel(LogLevel.Information);

    /// <summary>
    /// Whether the log entry was made at <see cref="LogLevel.Warning"/>.
    /// </summary>
    public bool IsWarning() => HasLogLevel(LogLevel.Warning);

    /// <summary>
    /// Whether the log entry was made at <see cref="LogLevel.Error"/>.
    /// </summary>
    public bool IsError() => HasLogLevel(LogLevel.Error);

    /// <summary>
    /// Whether the log entry was made at <see cref="LogLevel.Trace"/>.
    /// </summary>
    public bool IsCritical() => HasLogLevel(LogLevel.Critical);

    /// <summary>
    /// Whether the log entry has the specified level.
    /// </summary>
    /// <param name="expectedLogLevel">The level to check.</param>
    public bool HasLogLevel(LogLevel expectedLogLevel) => _logLevel == expectedLogLevel;

    /// <summary>
    /// Whether the log entry has a level that matches the specified predicate.
    /// </summary>
    /// <param name="logLevelPredicate">A function the returns whether the log entry's level is a match.</param>
    public bool HasLogLevel(Func<LogLevel, bool> logLevelPredicate)
    {
        ThrowIfNull(logLevelPredicate);

        return logLevelPredicate(_logLevel);
    }

    /// <summary>
    /// Whether the log entry has the specified Id.
    /// </summary>
    /// <param name="eventId">The Id to check.</param>
    public bool HasEventId(EventId eventId) => _eventId == eventId;

    /// <summary>
    /// Whether the log entry has an Id that matches the specified predicate.
    /// </summary>
    /// <param name="eventIdPredicate">A function that determines whether the log entry's Id is a match.</param>
    public bool HasEventId(Func<EventId, bool> eventIdPredicate)
    {
        ThrowIfNull(eventIdPredicate);

        return eventIdPredicate(_eventId);
    }

    /// <summary>
    /// Whether the log entry has the specified message.
    /// </summary>
    /// <param name="expectedMessage">The message to check.</param>
    public bool HasMessage(string expectedMessage)
    {
        ThrowIfNull(expectedMessage);

        return _getMessage is not null && string.Equals(_getMessage(), expectedMessage);
    }

    /// <summary>
    /// Whether the log entry has the specified message.
    /// </summary>
    /// <param name="expectedMessage">The message to check.</param>
    /// <param name="stringComparison">One of the enumeration values that specifies the rules for the comparison.</param>
    public bool HasMessage(string expectedMessage, StringComparison stringComparison)
    {
        ThrowIfNull(expectedMessage);

        return _getMessage is not null && string.Equals(_getMessage(), expectedMessage, stringComparison);
    }

    /// <summary>
    /// Whether the log entry has a message that matches the specified predicate.
    /// </summary>
    /// <param name="messagePredicate">A function that determines whether the log entry's message is a match.</param>
    public bool HasMessage(Func<string, bool> messagePredicate)
    {
        ThrowIfNull(messagePredicate);

        return _getMessage is not null && messagePredicate(_getMessage());
    }

    /// <summary>
    /// Whether the log entry has a message that matches the specified regular expression.
    /// </summary>
    /// <param name="regexPattern">The regular expression pattern to match.</param>
    public bool HasMessageMatching([StringSyntax(nameof(Regex))] string regexPattern)
    {
        ThrowIfNull(regexPattern);

        return _getMessage is not null && Regex.IsMatch(_getMessage(), regexPattern);
    }

    /// <summary>
    /// Whether the log entry has a message that matches the specified regular expression.
    /// </summary>
    /// <param name="regexPattern">The regular expression pattern to match.</param>
    /// <param name="regexOptions">A bitwise combination of the enumeration values that provide options for matching.</param>
    public bool HasMessageMatching(
        [StringSyntax(nameof(Regex))] string regexPattern,
        RegexOptions regexOptions)
    {
        ThrowIfNull(regexPattern);

        return _getMessage is not null && Regex.IsMatch(_getMessage(), regexPattern, regexOptions);
    }

    /// <summary>
    /// Whether the log entry has an attribute with the specified key.
    /// </summary>
    /// <param name="key">The key to match.</param>
    public bool HasAttribute(string key)
    {
        ThrowIfNull(key);

        foreach (var attribute in _attributes)
        {
            if (attribute.Key == key)
                return true;
        }

        return false;
    }

    /// <summary>
    /// Whether the log entry has an attribute with the specified key and value.
    /// </summary>
    /// <param name="key">The key to match.</param>
    /// <param name="value">The value to match.</param>
    public bool HasAttribute(string key, object value)
    {
        ThrowIfNull(key);
        ThrowIfNull(value);

        foreach (var attribute in _attributes)
        {
            if (attribute.Key == key && Equals(attribute.Value, value))
                return true;
        }

        return false;
    }

    /// <summary>
    /// Whether the log entry has an attribute with the specified key and value.
    /// </summary>
    /// <typeparam name="T">The expected type of the value.</typeparam>
    /// <param name="key">The key to match.</param>
    /// <param name="value">The value to match.</param>
    public bool HasAttribute<T>(string key, T value)
        where T : notnull
    {
        ThrowIfNull(key);
        ThrowIfNull(value);

        foreach (var attribute in _attributes)
        {
            if (attribute.Key == key && attribute.Value is T tValue && Equals(tValue, value))
                return true;
        }

        return false;
    }

    /// <summary>
    /// Whether the log entry has an attribute with the specified key and a value that matches the specified predicate.
    /// </summary>
    /// <param name="key">The key to match.</param>
    /// <param name="valuePredicate">A function the returns whether the value retrieved by the key is a match.</param>
    public bool HasAttribute(string key, Func<object, bool> valuePredicate)
    {
        ThrowIfNull(key);
        ThrowIfNull(valuePredicate);

        foreach (var attribute in _attributes)
        {
            if (attribute.Key == key && valuePredicate(attribute.Value))
                return true;
        }

        return false;
    }

    /// <summary>
    /// Whether the log entry has an attribute with the specified key and a value that matches the specified predicate.
    /// </summary>
    /// <typeparam name="T">The expected type of the value.</typeparam>
    /// <param name="key">The key to match.</param>
    /// <param name="valuePredicate">A function the returns whether the value retrieved by the key is a match.</param>
    public bool HasAttribute<T>(string key, Func<T, bool> valuePredicate)
        where T : notnull
    {
        ThrowIfNull(key);
        ThrowIfNull(valuePredicate);

        foreach (var attribute in _attributes)
        {
            if (attribute.Key == key && attribute.Value is T tValue && valuePredicate(tValue))
                return true;
        }

        return false;
    }

    /// <summary>
    /// Whether the log entry was made without a state.
    /// </summary>
    public bool HasNoState() => _attributes.State is null;

    /// <summary>
    /// Whether the log entry was made with any state.
    /// </summary>
    public bool HasState() => _attributes.State is not null;

    /// <summary>
    /// Whether the log entry was made with the specified state.
    /// </summary>
    /// <param name="expectedState">The expected state.</param>
    public bool HasState(object? expectedState)
    {
        if (expectedState is null)
            return _attributes.State is null;

        return Equals(_attributes.State, expectedState);
    }

    /// <summary>
    /// Whether the log entry was made with the specified state.
    /// </summary>
    /// <typeparam name="TState">The type of the expected state.</typeparam>
    /// <param name="expectedState">The expected state.</param>
    public bool HasState<TState>(TState? expectedState)
    {
        if (expectedState is null)
            return _attributes.State is null;

        return _attributes.State is TState state && Equals(state, expectedState);
    }

    /// <summary>
    /// Whether the log entry was made with a state that matches the specified predicate.
    /// </summary>
    /// <param name="statePredicate">A function that determines whether the log entry's state is a match.</param>
    public bool HasState(Func<object?, bool> statePredicate)
    {
        ThrowIfNull(statePredicate);

        return statePredicate(_attributes.State);
    }

    /// <summary>
    /// Whether the log entry was made with a state that matches the specified predicate.
    /// </summary>
    /// <typeparam name="TState">The expectetd type of the state.</typeparam>
    /// <param name="statePredicate">A function that determines whether the log entry's state is a match.</param>
    public bool HasState<TState>(Func<TState, bool> statePredicate)
    {
        ThrowIfNull(statePredicate);

        return _attributes.State is TState state && statePredicate(state);
    }

    /// <summary>
    /// Whether the log entry was made without a logger scope.
    /// </summary>
    public bool HasNoScope() => _attributes.Scope is null;

    /// <summary>
    /// Whether the log entry was made with a logger scope.
    /// </summary>
    public bool HasScope() => _attributes.Scope is not null;

    /// <summary>
    /// Whether the log entry was made with the specified logger scope.
    /// </summary>
    /// <param name="expectedScope">The expected logger scope.</param>
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

    /// <summary>
    /// Whether the log entry was made with the specified logger scope.
    /// </summary>
    /// <typeparam name="TState">The type of the expected logger scope.</typeparam>
    /// <param name="expectedScope">The expected logger scope.</param>
    public bool HasScope<TState>(TState? expectedScope)
    {
        if (expectedScope is null)
            return _attributes.Scope is null;

        for (var scope = _attributes.Scope; scope is not null; scope = scope.ParentScope)
        {
            if (scope.State is TState state && Equals(state, expectedScope))
                return true;
        }

        return false;
    }

    /// <summary>
    /// Whether the log entry was made with a logger scope that matches the specified predicate.
    /// </summary>
    /// <param name="scopePredicate">A function that determines whether the logger scope is a match.</param>
    public bool HasScope(Func<object, bool> scopePredicate)
    {
        ThrowIfNull(scopePredicate);

        for (var scope = _attributes.Scope; scope is not null; scope = scope.ParentScope)
        {
            if (scopePredicate(scope.State))
                return true;
        }

        return false;
    }

    /// <summary>
    /// Whether the log entry was made with a logger scope that matches the specified predicate.
    /// </summary>
    /// <typeparam name="TState">The expected type of the logger scope.</typeparam>
    /// <param name="scopePredicate">A function that determines whether the logger scope is a match.</param>
    public bool HasScope<TState>(Func<TState, bool> scopePredicate)
        where TState : notnull
    {
        ThrowIfNull(scopePredicate);

        for (var scope = _attributes.Scope; scope is not null; scope = scope.ParentScope)
        {
            if (scope.State is TState state && scopePredicate(state))
                return true;
        }

        return false;
    }

    /// <summary>
    /// Whether the log entry was made without an exception.
    /// </summary>
    public bool HasNoException() => _exception is null;

    /// <summary>
    /// Whether the log entry was made with any exception.
    /// </summary>
    public bool HasException() => _exception is not null;

    /// <summary>
    /// Whether the log entry was made with an exception of type <typeparamref name="TException"/>.
    /// </summary>
    /// <typeparam name="TException">The expected type of exception.</typeparam>
    public bool HasException<TException>()
        where TException : Exception => _exception is TException;

    /// <summary>
    /// Whether the log entry was made with the specified exception.
    /// </summary>
    /// <param name="expectedException">The exception to check (by reference).</param>
    public bool HasException(Exception? expectedException) =>
        ReferenceEquals(_exception, expectedException);

    /// <summary>
    /// Whether the log entry has an exception that matches the specified predicate.
    /// </summary>
    /// <param name="exceptionPredicate">A function that determines whether the log entry's exception is a match.</param>
    public bool HasException(Func<Exception?, bool> exceptionPredicate)
    {
        ThrowIfNull(exceptionPredicate);

        return exceptionPredicate(_exception);
    }

    /// <summary>
    /// Whether the log entry has an exception of type <typeparamref name="TException"/> that matches the specified predicate.
    /// </summary>
    /// <typeparam name="TException">The expected type of exception.</typeparam>
    /// <param name="exceptionPredicate">A function that determines whether the log entry's exception is a match.</param>
    public bool HasException<TException>(Func<TException, bool> exceptionPredicate)
        where TException : Exception
    {
        ThrowIfNull(exceptionPredicate);

        return _exception is TException tException && exceptionPredicate(tException);
    }

    /// <summary>
    /// Returns a string that represents the current log entry.
    /// </summary>
    /// <returns>A string that represents the current log entry.</returns>
    public override string ToString()
    {
        var sb = StringBuilderPool.Get();
        Append(sb, _logLevel, _eventId, _getMessage(), _attributes.State, _attributes.Scope, _exception);
        return sb.ReturnToPool();
    }

    private string GetDebuggerDisplay()
    {
        var sb = new StringBuilder();
        Append(sb, _logLevel, _eventId, _getMessage(), _attributes.State, _attributes.Scope, _exception);
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

using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;

namespace RandomSkunk.EasyLogging;

/// <summary>
/// Defines a log event.
/// </summary>
public readonly struct LogEntry
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LogEntry"/> struct.
    /// </summary>
    /// <param name="logLevel">The log level of the entry.</param>
    /// <param name="eventId">The Id of the entry.</param>
    /// <param name="getMessage">A function that returns the message of the entry.</param>
    /// <param name="attributes">A collection of key/value pairs that describe the state and scope
    /// of the entry.</param>
    /// <param name="exception">The exception related to the entry.</param>
    /// <exception cref="ArgumentNullException">If <paramref name="getMessage"/> is
    /// <see langword="null"/>.</exception>
    public LogEntry(
        LogLevel logLevel,
        EventId eventId,
        Func<string> getMessage,
        LogAttributes attributes,
        Exception? exception)
    {
        ArgumentNullException.ThrowIfNull(getMessage);

        LogLevel = logLevel;
        EventId = eventId;
        GetMessage = getMessage;
        Attributes = attributes;
        Exception = exception;
    }

    /// <summary>
    /// The log level of the entry.
    /// </summary>
    public LogLevel LogLevel { get; }

    /// <summary>
    /// The Id of the entry.
    /// </summary>
    public EventId EventId { get; }

    /// <summary>
    /// A function that gets the message of the entry.
    /// </summary>
    public Func<string> GetMessage { get; }

    /// <summary>
    /// A collection of key/value pairs that describe the state and scope of the entry.
    /// </summary>
    public LogAttributes Attributes { get; }

    /// <summary>
    /// The exception related to the entry.
    /// </summary>
    public Exception? Exception { get; }

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
    public bool HasLogLevel(LogLevel expectedLogLevel) => LogLevel == expectedLogLevel;

    /// <summary>
    /// Whether the log entry has a level that matches the specified predicate.
    /// </summary>
    /// <param name="logLevelPredicate">A function the returns whether the log entry's level is a
    /// match.</param>
    public bool HasLogLevel(Func<LogLevel, bool> logLevelPredicate)
    {
        ArgumentNullException.ThrowIfNull(logLevelPredicate);

        return logLevelPredicate(LogLevel);
    }

    /// <summary>
    /// Whether the log entry has the specified Id.
    /// </summary>
    /// <param name="eventId">The Id to check.</param>
    public bool HasEventId(EventId eventId) => EventId == eventId;

    /// <summary>
    /// Whether the log entry has an Id that matches the specified predicate.
    /// </summary>
    /// <param name="eventIdPredicate">A function that returns whether the log entry's Id is a
    /// match.</param>
    public bool HasEventId(Func<EventId, bool> eventIdPredicate)
    {
        ArgumentNullException.ThrowIfNull(eventIdPredicate);

        return eventIdPredicate(EventId);
    }

    /// <summary>
    /// Whether the log entry has the specified message.
    /// </summary>
    /// <param name="expectedMessage">The message to check.</param>
    public bool HasMessage(string expectedMessage)
    {
        ArgumentNullException.ThrowIfNull(expectedMessage);

        return string.Equals(GetMessage(), expectedMessage);
    }

    public bool HasMessage(string expectedMessage, StringComparison stringComparison)
    {
        ArgumentNullException.ThrowIfNull(expectedMessage);

        return string.Equals(GetMessage(), expectedMessage, stringComparison);
    }

    /// <summary>
    /// Whether the log entry has a message that matches the specified predicate.
    /// </summary>
    /// <param name="messagePredicate">A function that returns whether the log entry's message is a
    /// match.</param>
    public bool HasMessage(Func<string, bool> messagePredicate)
    {
        ArgumentNullException.ThrowIfNull(messagePredicate);

        return messagePredicate(GetMessage());
    }

    /// <summary>
    /// Whether the log entry has a message that matches the specified regular expression.
    /// </summary>
    /// <param name="regexPattern">The regular expression pattern to match.</param>
    public bool HasMessageMatching([StringSyntax(nameof(Regex))] string regexPattern)
    {
        ArgumentNullException.ThrowIfNull(regexPattern);

        return Regex.IsMatch(GetMessage(), regexPattern);
    }

    /// <summary>
    /// Whether the log entry has a message that matches the specified regular expression.
    /// </summary>
    /// <param name="regexPattern">The regular expression pattern to match.</param>
    /// <param name="regexOptions">A bitwise combination of the enumeration values that provide
    /// options for matching.</param>
    public bool HasMessageMatching(
        [StringSyntax(nameof(Regex))] string regexPattern,
        RegexOptions regexOptions)
    {
        ArgumentNullException.ThrowIfNull(regexPattern);

        return Regex.IsMatch(GetMessage(), regexPattern, regexOptions);
    }

    /// <summary>
    /// Whether the log entry has an attribute with the specified key.
    /// </summary>
    /// <param name="key">The key to match.</param>
    public bool HasAttribute(string key)
    {
        ArgumentNullException.ThrowIfNull(key);

        foreach (var attribute in Attributes)
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
        ArgumentNullException.ThrowIfNull(key);
        ArgumentNullException.ThrowIfNull(value);

        foreach (var attribute in Attributes)
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
        ArgumentNullException.ThrowIfNull(key);
        ArgumentNullException.ThrowIfNull(value);

        foreach (var attribute in Attributes)
        {
            if (attribute.Key == key && attribute.Value is T tValue && Equals(tValue, value))
                return true;
        }

        return false;
    }

    /// <summary>
    /// Whether the log entry has an attribute with the specified key and a value that matches the
    /// specified predicate.
    /// </summary>
    /// <param name="key">The key to match.</param>
    /// <param name="valuePredicate">A function the returns whether the value retrieved by the key
    /// is a match.</param>
    public bool HasAttribute(string key, Func<object, bool> valuePredicate)
    {
        ArgumentNullException.ThrowIfNull(key);
        ArgumentNullException.ThrowIfNull(valuePredicate);

        foreach (var attribute in Attributes)
        {
            if (attribute.Key == key && valuePredicate(attribute.Value))
                return true;
        }

        return false;
    }

    /// <summary>
    /// Whether the log entry has an attribute with the specified key and a value that matches the
    /// specified predicate.
    /// </summary>
    /// <typeparam name="T">The expected type of the value.</typeparam>
    /// <param name="key">The key to match.</param>
    /// <param name="valuePredicate">A function the returns whether the value retrieved by the key
    /// is a match.</param>
    public bool HasAttribute<T>(string key, Func<T, bool> valuePredicate)
        where T : notnull
    {
        ArgumentNullException.ThrowIfNull(key);
        ArgumentNullException.ThrowIfNull(valuePredicate);

        foreach (var attribute in Attributes)
        {
            if (attribute.Key == key && attribute.Value is T tValue && valuePredicate(tValue))
                return true;
        }

        return false;
    }

    /// <summary>
    /// Whether the log entry was made without a state object.
    /// </summary>
    public bool HasNoState() => Attributes.State is null;

    /// <summary>
    /// Whether the log entry was made with any state object.
    /// </summary>
    public bool HasState() => Attributes.State is not null;

    /// <summary>
    /// Whether the log entry was made with a state object that matches the specified predicate.
    /// </summary>
    /// <param name="statePredicate">A function that returns whether the log entry's state object
    /// is a match.</param>
    public bool HasState(Func<object?, bool> statePredicate)
    {
        ArgumentNullException.ThrowIfNull(statePredicate);

        return statePredicate(Attributes.State);
    }

    /// <summary>
    /// Whether the log entry was made with a state object that matches the specified predicate.
    /// </summary>
    /// <typeparam name="TState">The expectetd type of the state object.</typeparam>
    /// <param name="statePredicate">A function that returns whether the log entry's state object
    /// is a match.</param>
    public bool HasState<TState>(Func<TState, bool> statePredicate)
    {
        ArgumentNullException.ThrowIfNull(statePredicate);

        return Attributes.State is TState state && statePredicate(state);
    }

    /// <summary>
    /// Whether the log entry was made without a logger scope.
    /// </summary>
    public bool HasNoScope() => Attributes.Scope is null;

    /// <summary>
    /// Whether the log entry was made with a logger scope.
    /// </summary>
    public bool HasScope() => Attributes.Scope is not null;

    /// <summary>
    /// Whether the log entry was made with a logger scope that matches the specified predicate.
    /// </summary>
    /// <param name="scopePredicate">A function that returns whether the log entry's scope state
    /// object is a match.</param>
    public bool HasScope(Func<object, bool> scopePredicate)
    {
        ArgumentNullException.ThrowIfNull(scopePredicate);

        for (var scope = Attributes.Scope; scope is not null; scope = scope.ParentScope)
        {
            if (scopePredicate(scope.State))
                return true;
        }

        return false;
    }

    /// <summary>
    /// Whether the log entry was made with a logger scope that matches the specified predicate.
    /// </summary>
    /// <typeparam name="TState">The expected type of the scope's state object.</typeparam>
    /// <param name="scopePredicate">A function that returns whether the log entry's scope state
    /// object is a match.</param>
    public bool HasScope<TState>(Func<TState, bool> scopePredicate)
        where TState : notnull
    {
        ArgumentNullException.ThrowIfNull(scopePredicate);

        for (var scope = Attributes.Scope; scope is not null; scope = scope.ParentScope)
        {
            if (scope.State is TState state && scopePredicate(state))
                return true;
        }

        return false;
    }

    /// <summary>
    /// Whether the log entry was made without an exception.
    /// </summary>
    public bool HasNoException() => Exception is null;

    /// <summary>
    /// Whether the log entry was made with any exception.
    /// </summary>
    public bool HasException() => Exception is not null;

    /// <summary>
    /// Whether the log entry was made with an exception of type <typeparamref name="TException"/>.
    /// </summary>
    /// <typeparam name="TException">The expected type of exception.</typeparam>
    public bool HasException<TException>()
        where TException : Exception => Exception is TException;

    /// <summary>
    /// Whether the log entry was made with the specified exception.
    /// </summary>
    /// <param name="expectedException">The exception to check (by reference).</param>
    public bool HasException(Exception? expectedException) =>
        ReferenceEquals(Exception, expectedException);

    /// <summary>
    /// Whether the log entry has an exception that matches the specified predicate.
    /// </summary>
    /// <param name="exceptionPredicate">A function that returns whether the log entry's exception
    /// is a match.</param>
    public bool HasException(Func<Exception?, bool> exceptionPredicate)
    {
        ArgumentNullException.ThrowIfNull(exceptionPredicate);

        return exceptionPredicate(Exception);
    }

    /// <summary>
    /// Whether the log entry has an exception of type <typeparamref name="TException"/> that
    /// matches the specified predicate.
    /// </summary>
    /// <typeparam name="TException">The expected type of exception.</typeparam>
    /// <param name="exceptionPredicate">A function that returns whether the log entry's exception
    /// is a match.</param>
    public bool HasException<TException>(Func<TException, bool> exceptionPredicate)
        where TException : Exception
    {
        ArgumentNullException.ThrowIfNull(exceptionPredicate);

        return Exception is TException tException && exceptionPredicate(tException);
    }

    public override string ToString()
    {
        var sb = StringBuilderPool.Get();
        sb.Append("{\n LogLevel = ").Append(LogLevel);

        if (EventId.Id != 0)
            sb.Append(",\n EventId = ").Append(EventId);

        var message = GetMessage();
        if (!string.IsNullOrWhiteSpace(message))
            sb.Append(",\n Message = ").Append(message);
        else
            message = null; // Normalize empty and whitespace message strings to null.

        if (Attributes.State is not null)
            sb.Append(",\n State = ").AppendState(Attributes.State, skipStateStringIfEqualTo: message);

        if (Attributes.Scope is not null)
        {
            for (var scope = Attributes.Scope; scope is not null; scope = scope.ParentScope)
            {
                if (ReferenceEquals(scope, Attributes.Scope))
                    sb.Append(",\n Scope = ").AppendState(scope.State);
                else
                    sb.Append(",\n ParentScope = ").AppendState(scope.State);
            }
        }

        if (Exception is not null)
            sb.Append(",\n Exception = ").Append(Exception);

        sb.Append("\n}");
        return sb.ReturnToPool();
    }
}

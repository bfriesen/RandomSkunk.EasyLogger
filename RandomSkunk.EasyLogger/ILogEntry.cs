using Microsoft.Extensions.Logging;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace RandomSkunk.Logging;

/// <summary>
/// Defines a log event.
/// </summary>
public interface ILogEntry
{
    /// <summary>
    /// Gets the log category.
    /// </summary>
    string Category { get; }

    /// <summary>
    /// Gets the log level.
    /// </summary>
    LogLevel LogLevel { get; }

    /// <summary>
    /// Gets the log event ID.
    /// </summary>
    EventId EventId { get; }

    /// <summary>
    /// Gets the formatted message.
    /// </summary>
    string Message { get; }

    /// <summary>
    /// Gets the exception related to this entry.
    /// </summary>
    Exception? Exception { get; }

    /// <summary>
    /// Gets a collection of key/value pairs derived from the state and scope of the log entry.
    /// </summary>
    IEnumerable<KeyValuePair<string, object>> Attributes { get; }

    /// <summary>
    /// Gets the log entry state.
    /// </summary>
    object? State { get; }

    /// <summary>
    /// Gets the collection of objects that represent a logger's current scope at the time of the log. The first object in the
    /// collection represents the logger's current scope, the second object represents its parent scope, the third represents its
    /// grandparent scope, and so on.
    /// </summary>
    ScopeCollection Scope { get; }

    /// <summary>
    /// Whether the log entry has the specified category.
    /// </summary>
    /// <param name="category">The category to check.</param>
    bool HasCategory(string category);

    /// <summary>
    /// Whether the log entry has a category specified by <typeparamref name="TCategoryName"/>.
    /// </summary>
    /// <typeparam name="TCategoryName">The type whose name is used for the expected logger category name.</typeparam>
    bool HasCategory<TCategoryName>();

    /// <summary>
    /// Whether the log entry has a category that matches the specified predicate.
    /// </summary>
    /// <param name="categoryPredicate">A function the returns whether the log entry's category is a match.</param>
    bool HasCategory(Func<string, bool> categoryPredicate);

    /// <summary>
    /// Whether the log entry was made at <see cref="LogLevel.Trace"/>.
    /// </summary>
    bool IsTrace();

    /// <summary>
    /// Whether the log entry was made at <see cref="LogLevel.Debug"/>.
    /// </summary>
    bool IsDebug();

    /// <summary>
    /// Whether the log entry was made at <see cref="LogLevel.Information"/>.
    /// </summary>
    bool IsInformation();

    /// <summary>
    /// Whether the log entry was made at <see cref="LogLevel.Warning"/>.
    /// </summary>
    bool IsWarning();

    /// <summary>
    /// Whether the log entry was made at <see cref="LogLevel.Error"/>.
    /// </summary>
    bool IsError();

    /// <summary>
    /// Whether the log entry was made at <see cref="LogLevel.Trace"/>.
    /// </summary>
    bool IsCritical();

    /// <summary>
    /// Whether the log entry has the specified level.
    /// </summary>
    /// <param name="expectedLogLevel">The level to check.</param>
    bool HasLogLevel(LogLevel expectedLogLevel);

    /// <summary>
    /// Whether the log entry has a level that matches the specified predicate.
    /// </summary>
    /// <param name="logLevelPredicate">A function the returns whether the log entry's level is a match.</param>
    bool HasLogLevel(Func<LogLevel, bool> logLevelPredicate);

    /// <summary>
    /// Whether the log entry has the specified Id.
    /// </summary>
    /// <param name="eventId">The Id to check.</param>
    bool HasEventId(EventId eventId);

    /// <summary>
    /// Whether the log entry has an Id that matches the specified predicate.
    /// </summary>
    /// <param name="eventIdPredicate">A function that determines whether the log entry's Id is a match.</param>
    bool HasEventId(Func<EventId, bool> eventIdPredicate);

    /// <summary>
    /// Whether the log entry has the specified message.
    /// </summary>
    /// <param name="expectedMessage">The message to check.</param>
    bool HasMessage(string expectedMessage);

    /// <summary>
    /// Whether the log entry has the specified message.
    /// </summary>
    /// <param name="expectedMessage">The message to check.</param>
    /// <param name="stringComparison">One of the enumeration values that specifies the rules for the comparison.</param>
    bool HasMessage(string expectedMessage, StringComparison stringComparison);

    /// <summary>
    /// Whether the log entry has a message that matches the specified predicate.
    /// </summary>
    /// <param name="messagePredicate">A function that determines whether the log entry's message is a match.</param>
    bool HasMessage(Func<string, bool> messagePredicate);

    /// <summary>
    /// Whether the log entry has a message that matches the specified regular expression.
    /// </summary>
    /// <param name="regexPattern">The regular expression pattern to match.</param>
    bool HasMessageMatching([StringSyntax(nameof(Regex))] string regexPattern);

    /// <summary>
    /// Whether the log entry has a message that matches the specified regular expression.
    /// </summary>
    /// <param name="regexPattern">The regular expression pattern to match.</param>
    /// <param name="regexOptions">A bitwise combination of the enumeration values that provide options for matching.</param>
    bool HasMessageMatching([StringSyntax(nameof(Regex))] string regexPattern, RegexOptions regexOptions);

    /// <summary>
    /// Whether the log entry has an attribute with the specified key.
    /// </summary>
    /// <param name="key">The key to match.</param>
    bool HasAttribute(string key);

    /// <summary>
    /// Whether the log entry has an attribute with the specified key and value.
    /// </summary>
    /// <param name="key">The key to match.</param>
    /// <param name="value">The value to match.</param>
    bool HasAttribute(string key, object value);

    /// <summary>
    /// Whether the log entry has an attribute with the specified key and value.
    /// </summary>
    /// <typeparam name="T">The expected type of the value.</typeparam>
    /// <param name="key">The key to match.</param>
    /// <param name="value">The value to match.</param>
    bool HasAttribute<T>(string key, T value) where T : notnull;

    /// <summary>
    /// Whether the log entry has an attribute with the specified key and a value that matches the specified predicate.
    /// </summary>
    /// <param name="key">The key to match.</param>
    /// <param name="valuePredicate">A function the returns whether the value retrieved by the key is a match.</param>
    bool HasAttribute(string key, Func<object, bool> valuePredicate);

    /// <summary>
    /// Whether the log entry has an attribute with the specified key and a value that matches the specified predicate.
    /// </summary>
    /// <typeparam name="T">The expected type of the value.</typeparam>
    /// <param name="key">The key to match.</param>
    /// <param name="valuePredicate">A function the returns whether the value retrieved by the key is a match.</param>
    bool HasAttribute<T>(string key, Func<T, bool> valuePredicate) where T : notnull;

    /// <summary>
    /// Whether the log entry was made without a state.
    /// </summary>
    bool HasNoState();

    /// <summary>
    /// Whether the log entry was made with any state.
    /// </summary>
    bool HasState();

    /// <summary>
    /// Whether the log entry was made with the specified state.
    /// </summary>
    /// <param name="expectedState">The expected state.</param>
    bool HasState(object? expectedState);

    /// <summary>
    /// Whether the log entry was made with the specified state.
    /// </summary>
    /// <typeparam name="T">The type of the expected state.</typeparam>
    /// <param name="expectedState">The expected state.</param>
    bool HasState<T>(T? expectedState);

    /// <summary>
    /// Whether the log entry was made with a state that matches the specified predicate.
    /// </summary>
    /// <param name="statePredicate">A function that determines whether the log entry's state is a match.</param>
    bool HasState(Func<object?, bool> statePredicate);

    /// <summary>
    /// Whether the log entry was made with a state that matches the specified predicate.
    /// </summary>
    /// <typeparam name="T">The expectetd type of the state.</typeparam>
    /// <param name="statePredicate">A function that determines whether the log entry's state is a match.</param>
    bool HasState<T>(Func<T, bool> statePredicate);

    /// <summary>
    /// Whether the log entry was made without a logger scope.
    /// </summary>
    bool HasNoScope();

    /// <summary>
    /// Whether the log entry was made with a logger scope.
    /// </summary>
    bool HasScope();

    /// <summary>
    /// Whether the log entry was made with the specified logger scope.
    /// </summary>
    /// <param name="expectedScope">The expected logger scope.</param>
    bool HasScope(object? expectedScope);

    /// <summary>
    /// Whether the log entry was made with the specified logger scope.
    /// </summary>
    /// <typeparam name="T">The type of the expected logger scope.</typeparam>
    /// <param name="expectedScope">The expected logger scope.</param>
    bool HasScope<T>(T? expectedScope);

    /// <summary>
    /// Whether the log entry was made with a logger scope that matches the specified predicate.
    /// </summary>
    /// <param name="scopePredicate">A function that determines whether the logger scope is a match.</param>
    bool HasScope(Func<object, bool> scopePredicate);

    /// <summary>
    /// Whether the log entry was made with a logger scope that matches the specified predicate.
    /// </summary>
    /// <typeparam name="T">The expected type of the logger scope.</typeparam>
    /// <param name="scopePredicate">A function that determines whether the logger scope is a match.</param>
    bool HasScope<T>(Func<T, bool> scopePredicate) where T : notnull;

    /// <summary>
    /// Whether the log entry was made without an exception.
    /// </summary>
    bool HasNoException();

    /// <summary>
    /// Whether the log entry was made with any exception.
    /// </summary>
    bool HasException();

    /// <summary>
    /// Whether the log entry was made with an exception of type <typeparamref name="TException"/>.
    /// </summary>
    /// <typeparam name="TException">The expected type of exception.</typeparam>
    bool HasException<TException>() where TException : Exception;

    /// <summary>
    /// Whether the log entry was made with the specified exception.
    /// </summary>
    /// <param name="expectedException">The exception to check (by reference).</param>
    bool HasException(Exception? expectedException);

    /// <summary>
    /// Whether the log entry has an exception that matches the specified predicate.
    /// </summary>
    /// <param name="exceptionPredicate">A function that determines whether the log entry's exception is a match.</param>
    bool HasException(Func<Exception?, bool> exceptionPredicate);

    /// <summary>
    /// Whether the log entry has an exception of type <typeparamref name="TException"/> that matches the specified predicate.
    /// </summary>
    /// <typeparam name="TException">The expected type of exception.</typeparam>
    /// <param name="exceptionPredicate">A function that determines whether the log entry's exception is a match.</param>
    bool HasException<TException>(Func<TException, bool> exceptionPredicate) where TException : Exception;
}

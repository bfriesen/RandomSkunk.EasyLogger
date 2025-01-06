using System.Collections;
using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;

namespace RandomSkunk.EasyLogging.Experimental;

public static class LoggerExtensions
{
    private static readonly Func<FormattedLogAttributes, Exception?, string> _messageFormatter = MessageFormatter;

    public static void Trace(
        this ILogger logger,
        EventId eventId,
        Exception? exception,
        string messageFormat,
        params (string Key, object Value)[] attributes) =>
        logger.LogWithAttributes(LogLevel.Trace, eventId, exception, messageFormat, attributes);

    public static void Trace(
        this ILogger logger,
        EventId eventId,
        string messageFormat,
        params (string Key, object Value)[] attributes) =>
        logger.LogWithAttributes(LogLevel.Trace, eventId, null, messageFormat, attributes);

    public static void Trace(
        this ILogger logger,
        Exception? exception,
        string messageFormat,
        params (string Key, object Value)[] attributes) =>
        logger.LogWithAttributes(LogLevel.Trace, 0, exception, messageFormat, attributes);

    public static void Trace(
        this ILogger logger,
        string messageFormat,
        params (string Key, object Value)[] attributes) =>
        logger.LogWithAttributes(LogLevel.Trace, 0, null, messageFormat, attributes);

    public static void Debug(
        this ILogger logger,
        EventId eventId,
        Exception? exception,
        string messageFormat,
        params (string Key, object Value)[] attributes) =>
        logger.LogWithAttributes(LogLevel.Debug, eventId, exception, messageFormat, attributes);

    public static void Debug(
        this ILogger logger,
        EventId eventId,
        string messageFormat,
        params (string Key, object Value)[] attributes) =>
        logger.LogWithAttributes(LogLevel.Debug, eventId, null, messageFormat, attributes);

    public static void Debug(
        this ILogger logger,
        Exception? exception,
        string messageFormat,
        params (string Key, object Value)[] attributes) =>
        logger.LogWithAttributes(LogLevel.Debug, 0, exception, messageFormat, attributes);

    public static void Debug(
        this ILogger logger,
        string messageFormat,
        params (string Key, object Value)[] attributes) =>
        logger.LogWithAttributes(LogLevel.Debug, 0, null, messageFormat, attributes);

    public static void Information(
        this ILogger logger,
        EventId eventId,
        Exception? exception,
        string messageFormat,
        params (string Key, object Value)[] attributes) =>
        logger.LogWithAttributes(LogLevel.Information, eventId, exception, messageFormat, attributes);

    public static void Information(
        this ILogger logger,
        EventId eventId,
        string messageFormat,
        params (string Key, object Value)[] attributes) =>
        logger.LogWithAttributes(LogLevel.Information, eventId, null, messageFormat, attributes);

    public static void Information(
        this ILogger logger,
        Exception? exception,
        string messageFormat,
        params (string Key, object Value)[] attributes) =>
        logger.LogWithAttributes(LogLevel.Information, 0, exception, messageFormat, attributes);

    public static void Information(
        this ILogger logger,
        string messageFormat,
        params (string Key, object Value)[] attributes) =>
        logger.LogWithAttributes(LogLevel.Information, 0, null, messageFormat, attributes);

    public static void Warning(
        this ILogger logger,
        EventId eventId,
        Exception? exception,
        string messageFormat,
        params (string Key, object Value)[] attributes) =>
        logger.LogWithAttributes(LogLevel.Warning, eventId, exception, messageFormat, attributes);

    public static void Warning(
        this ILogger logger,
        EventId eventId,
        string messageFormat,
        params (string Key, object Value)[] attributes) =>
        logger.LogWithAttributes(LogLevel.Warning, eventId, null, messageFormat, attributes);

    public static void Warning(
        this ILogger logger,
        Exception? exception,
        string messageFormat,
        params (string Key, object Value)[] attributes) =>
        logger.LogWithAttributes(LogLevel.Warning, 0, exception, messageFormat, attributes);

    public static void Warning(
        this ILogger logger,
        string messageFormat,
        params (string Key, object Value)[] attributes) =>
        logger.LogWithAttributes(LogLevel.Warning, 0, null, messageFormat, attributes);

    public static void Error(
        this ILogger logger,
        EventId eventId,
        Exception? exception,
        string messageFormat,
        params (string Key, object Value)[] attributes) =>
        logger.LogWithAttributes(LogLevel.Error, eventId, exception, messageFormat, attributes);

    public static void Error(
        this ILogger logger,
        EventId eventId,
        string messageFormat,
        params (string Key, object Value)[] attributes) =>
        logger.LogWithAttributes(LogLevel.Error, eventId, null, messageFormat, attributes);

    public static void Error(
        this ILogger logger,
        Exception? exception,
        string messageFormat,
        params (string Key, object Value)[] attributes) =>
        logger.LogWithAttributes(LogLevel.Error, 0, exception, messageFormat, attributes);

    public static void Error(
        this ILogger logger,
        string messageFormat,
        params (string Key, object Value)[] attributes) =>
        logger.LogWithAttributes(LogLevel.Error, 0, null, messageFormat, attributes);

    public static void Critical(
        this ILogger logger,
        EventId eventId,
        Exception? exception,
        string messageFormat,
        params (string Key, object Value)[] attributes) =>
        logger.LogWithAttributes(LogLevel.Critical, eventId, exception, messageFormat, attributes);

    public static void Critical(
        this ILogger logger,
        EventId eventId,
        string messageFormat,
        params (string Key, object Value)[] attributes) =>
        logger.LogWithAttributes(LogLevel.Critical, eventId, null, messageFormat, attributes);

    public static void Critical(
        this ILogger logger,
        Exception? exception,
        string messageFormat,
        params (string Key, object Value)[] attributes) =>
        logger.LogWithAttributes(LogLevel.Critical, 0, exception, messageFormat, attributes);

    public static void Critical(
        this ILogger logger,
        string messageFormat,
        params (string Key, object Value)[] attributes) =>
        logger.LogWithAttributes(LogLevel.Critical, 0, null, messageFormat, attributes);

    private static void LogWithAttributes(
        this ILogger logger,
        LogLevel logLevel,
        EventId eventId,
        Exception? exception,
        string messageFormat,
        (string, object)[] attributes)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentException.ThrowIfNullOrWhiteSpace(messageFormat);

        if (logLevel is < LogLevel.Trace or > LogLevel.Critical)
            throw new ArgumentOutOfRangeException(nameof(logLevel));

        var state = new FormattedLogAttributes(messageFormat, attributes ?? []);
        logger.Log(logLevel, eventId, state, exception, _messageFormatter);
    }

    public static IDisposable? BeginScope(
        this ILogger logger,
        params (string Key, object Value)[] attributes)
    {
        ArgumentNullException.ThrowIfNull(logger);

        if (attributes is null or { Length: 0 })
            return null;

        return logger.BeginScope(attributes.ToKeyValuePairs());
    }

    public static IDisposable? BeginScope(
        this ILogger logger,
        string messageFormat,
        params (string Key, object Value)[] attributes)
    {
        ArgumentNullException.ThrowIfNull(logger);

        var state = new FormattedLogAttributes(messageFormat, attributes ?? []);
        return logger.BeginScope(state);
    }

    private readonly struct FormattedLogAttributes : IReadOnlyCollection<KeyValuePair<string, object>>
    {
        private static readonly ConcurrentDictionary<string[], Regex> _regexByAttributeKeys = new(AttributeKeysComparer.Instance);

        private readonly string _messageFormat;
        private readonly (string Key, object Value)[] _attributes;

        public FormattedLogAttributes(string messageFormat, (string, object)[] attributes)
        {
            _messageFormat = messageFormat;
            _attributes = attributes ?? [];
        }

        public int Count => _attributes.Length;

        public string FormatMessage(Exception? exception)
        {
            var localAttributes = _attributes;
            var attributesLength = localAttributes.Length;

            var attributeKeys = new string[attributesLength + 1];
            for (var i = 0; i < attributesLength; i++)
                attributeKeys[i] = Regex.Escape(localAttributes[i].Key);
            attributeKeys[^1] = nameof(Exception);
            Array.Sort(attributeKeys);

            var regex = _regexByAttributeKeys.GetOrAdd(
                attributeKeys,
                keys =>
                {
                    var pattern = "(?<={)(?>" + string.Join('|', keys) + ")(?=})";
                    return new Regex(pattern); // Is it worth it for a compiled regex here?
                });

            return regex.Replace(
                _messageFormat,
                match =>
                {
                    var matchedKey = match.Value;

                    foreach (var (key, value) in localAttributes)
                    {
                        if (matchedKey == key)
                            return value?.ToString() ?? "";
                    }

                    if (matchedKey == nameof(Exception))
                        return exception?.ToString() ?? "";

                    return "";
                });
        }

        public override string ToString() => FormatMessage(null);

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator() =>
            _attributes.ToKeyValuePairs().GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        private class AttributeKeysComparer : IEqualityComparer<string[]>
        {
            public static readonly AttributeKeysComparer Instance = new();

            private AttributeKeysComparer()
            {
            }

            public bool Equals(string[]? lhs, string[]? rhs)
            {
                if (ReferenceEquals(lhs, rhs))
                    return true;

                if (lhs == null || rhs == null)
                    return false;

                var length = lhs.Length;
                if (length != rhs.Length)
                    return false;

                for (int i = 0; i < length; i++)
                {
                    if (!string.Equals(lhs[i], rhs[i]))
                        return false;
                }

                return true;
            }

            public int GetHashCode(string[] array)
            {
                unchecked
                {
                    int result = 0;
                    var length = array.Length;
                    for (int i = 0; i < length; i++)
                        result = (result * 397) ^ (array[i]?.GetHashCode() ?? 0);
                    return result;
                }
            }
        }
    }

    private static string MessageFormatter(FormattedLogAttributes state, Exception? exception) =>
        state.FormatMessage(exception);
}

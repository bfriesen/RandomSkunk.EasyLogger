using FluentAssertions;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;

namespace RandomSkunk.Logging.Tests;

public class LogEntryTests
{
    public class Constructor
    {
        [Fact]
        public void SetsAllProperties()
        {
            const string message = "Hello world!";
            var logLevel = LogLevel.Warning;
            var eventId = new EventId(123);
            var getMessage = () => message;
            var state = "abc";
            var scope = "xyz";
            var logAttributes = new LogAttributes(state, scope);
            var exception = new Exception();

            var logEntry = new LogEntry(logLevel, eventId, getMessage, logAttributes, exception);

            logEntry.LogLevel.Should().Be(logLevel);
            logEntry.EventId.Should().Be(eventId);
            logEntry.GetMessage.Should().BeSameAs(getMessage);
            logEntry.Attributes.State.Should().BeSameAs(state);
            logEntry.Attributes.Scope.Should().NotBeNull();
            logEntry.Attributes.Scope?.State.Should().BeSameAs(scope);
            logEntry.Attributes.Scope!.ParentScope.Should().BeNull();
            logEntry.Exception.Should().BeSameAs(exception);
        }
    }

    public class IsTraceMethod
    {
        [Fact]
        public void ReturnsTrueForTraceLogs()
        {
            var logEntry = new LogEntry(LogLevel.Trace, 0, () => "", default, null);

            logEntry.IsTrace().Should().BeTrue();
        }

        [Theory]
        [InlineData(LogLevel.Debug)]
        [InlineData(LogLevel.Information)]
        [InlineData(LogLevel.Warning)]
        [InlineData(LogLevel.Error)]
        [InlineData(LogLevel.Critical)]
        public void ReturnsFalseForNonTraceLogs(LogLevel logLevel)
        {
            var logEntry = new LogEntry(logLevel, 0, () => "", default, null);

            logEntry.IsTrace().Should().BeFalse();
        }
    }

    public class IsDebugMethod
    {
        [Fact]
        public void ReturnsTrueForDebugLogs()
        {
            var logEntry = new LogEntry(LogLevel.Debug, 0, () => "", default, null);

            logEntry.IsDebug().Should().BeTrue();
        }

        [Theory]
        [InlineData(LogLevel.Trace)]
        [InlineData(LogLevel.Information)]
        [InlineData(LogLevel.Warning)]
        [InlineData(LogLevel.Error)]
        [InlineData(LogLevel.Critical)]
        public void ReturnsFalseForNonDebugLogs(LogLevel logLevel)
        {
            var logEntry = new LogEntry(logLevel, 0, () => "", default, null);

            logEntry.IsDebug().Should().BeFalse();
        }
    }

    public class IsInformationMethod
    {
        [Fact]
        public void ReturnsTrueForInformationLogs()
        {
            var logEntry = new LogEntry(LogLevel.Information, 0, () => "", default, null);

            logEntry.IsInformation().Should().BeTrue();
        }

        [Theory]
        [InlineData(LogLevel.Trace)]
        [InlineData(LogLevel.Debug)]
        [InlineData(LogLevel.Warning)]
        [InlineData(LogLevel.Error)]
        [InlineData(LogLevel.Critical)]
        public void ReturnsFalseForNonInformationLogs(LogLevel logLevel)
        {
            var logEntry = new LogEntry(logLevel, 0, () => "", default, null);

            logEntry.IsInformation().Should().BeFalse();
        }
    }

    public class IsWarningMethod
    {
        [Fact]
        public void ReturnsTrueForWarningLogs()
        {
            var logEntry = new LogEntry(LogLevel.Warning, 0, () => "", default, null);

            logEntry.IsWarning().Should().BeTrue();
        }

        [Theory]
        [InlineData(LogLevel.Trace)]
        [InlineData(LogLevel.Debug)]
        [InlineData(LogLevel.Information)]
        [InlineData(LogLevel.Error)]
        [InlineData(LogLevel.Critical)]
        public void ReturnsFalseForNonWarningLogs(LogLevel logLevel)
        {
            var logEntry = new LogEntry(logLevel, 0, () => "", default, null);

            logEntry.IsWarning().Should().BeFalse();
        }
    }

    public class IsErrorMethod
    {
        [Fact]
        public void ReturnsTrueForErrorLogs()
        {
            var logEntry = new LogEntry(LogLevel.Error, 0, () => "", default, null);

            logEntry.IsError().Should().BeTrue();
        }

        [Theory]
        [InlineData(LogLevel.Trace)]
        [InlineData(LogLevel.Debug)]
        [InlineData(LogLevel.Information)]
        [InlineData(LogLevel.Warning)]
        [InlineData(LogLevel.Critical)]
        public void ReturnsFalseForNonErrorLogs(LogLevel logLevel)
        {
            var logEntry = new LogEntry(logLevel, 0, () => "", default, null);

            logEntry.IsError().Should().BeFalse();
        }
    }

    public class IsCriticalMethod
    {
        [Fact]
        public void ReturnsTrueForCriticalLogs()
        {
            var logEntry = new LogEntry(LogLevel.Critical, 0, () => "", default, null);

            logEntry.IsCritical().Should().BeTrue();
        }

        [Theory]
        [InlineData(LogLevel.Trace)]
        [InlineData(LogLevel.Debug)]
        [InlineData(LogLevel.Information)]
        [InlineData(LogLevel.Warning)]
        [InlineData(LogLevel.Error)]
        public void ReturnsFalseForNonCriticalLogs(LogLevel logLevel)
        {
            var logEntry = new LogEntry(logLevel, 0, () => "", default, null);

            logEntry.IsCritical().Should().BeFalse();
        }
    }

    public class HasLogLevelMethod
    {
        public class GivenLogLevelParameter
        {
            [Fact]
            public void WhenLogLevelMatchesReturnsTrue()
            {
                LogLevel actualLogLevel = LogLevel.Error;
                LogLevel expectedLogLevel = actualLogLevel;

                var logEntry = new LogEntry(actualLogLevel, 0, () => "", default, null);

                logEntry.HasLogLevel(expectedLogLevel).Should().BeTrue();
            }

            [Fact]
            public void WhenLogLevelDoesNotMatchReturnsFalse()
            {
                LogLevel actualLogLevel = LogLevel.Error;
                LogLevel expectedLogLevel = LogLevel.Warning;

                var logEntry = new LogEntry(actualLogLevel, 0, () => "", default, null);

                logEntry.HasLogLevel(expectedLogLevel).Should().BeFalse();
            }
        }

        public class GivenFunctionParameter
        {
            [Theory]
            [InlineData(true)]
            [InlineData(false)]
            public void ReturnsResultOfFunction(bool valueReturnedByFunction)
            {
                LogLevel actualLogLevel = LogLevel.Information;
                LogLevel? capturedLogLevel = null;

                var logLevelPredicate = (LogLevel logLevel) =>
                {
                    capturedLogLevel = logLevel;
                    return valueReturnedByFunction;
                };

                var logEntry = new LogEntry(actualLogLevel, 0, () => "", default, null);

                logEntry.HasLogLevel(logLevelPredicate).Should().Be(valueReturnedByFunction);

                capturedLogLevel.Should().Be(actualLogLevel);
            }

            [Fact]
            public void ThrowsWhenParameterIsNull()
            {
                string actualMessage = "abc";
                Func<string, bool> messagePredicate = null!;

                var logEntry = new LogEntry(default, 0, () => actualMessage, default, null);

                logEntry.Invoking(x => x.HasMessage(messagePredicate))
                    .Should().ThrowExactly<ArgumentNullException>();
            }
        }
    }

    public class HasEventIdMethod
    {
        public class GivenEventIdParameter
        {
            [Fact]
            public void WhenEventIdMatchesReturnsTrue()
            {
                EventId actualEventId = 123;
                EventId expectedEventId = actualEventId;

                var logEntry = new LogEntry(default, actualEventId, () => "", default, null);

                logEntry.HasEventId(expectedEventId).Should().BeTrue();
            }

            [Fact]
            public void WhenEventIdDoesNotMatchReturnsFalse()
            {
                EventId actualEventId = 123;
                EventId expectedEventId = 456;

                var logEntry = new LogEntry(default, actualEventId, () => "", default, null);

                logEntry.HasEventId(expectedEventId).Should().BeFalse();
            }
        }

        public class GivenFunctionParameter
        {
            [Theory]
            [InlineData(true)]
            [InlineData(false)]
            public void ReturnsResultOfFunction(bool valueReturnedByFunction)
            {
                EventId actualEventId = 123;
                EventId? capturedEventId = null;

                var eventIdPredicate = (EventId eventId) =>
                {
                    capturedEventId = eventId;
                    return valueReturnedByFunction;
                };

                var logEntry = new LogEntry(default, actualEventId, () => "", default, null);

                logEntry.HasEventId(eventIdPredicate).Should().Be(valueReturnedByFunction);

                capturedEventId.Should().Be(actualEventId);
            }

            //[Fact]
            //public void ThrowsWhenParameterIsNull()
            //{
            //    string actualMessage = "abc";
            //    Func<string, bool> messagePredicate = null!;

            //    var logEntry = new LogEntry(default, 0, () => actualMessage, default, null);

            //    logEntry.Invoking(x => x.HasMessage(messagePredicate))
            //        .Should().ThrowExactly<ArgumentNullException>();
            //}
        }
    }

    public class HasMessageMethod
    {
        public class GivenMessageParameter
        {
            [Fact]
            public void WhenMessageMatchesReturnsTrue()
            {
                string actualMessage = "abc";
                string expectedMessage = actualMessage;

                var logEntry = new LogEntry(default, 0, () => actualMessage, default, null);

                logEntry.HasMessage(expectedMessage).Should().BeTrue();
            }

            [Fact]
            public void WhenMessageDoesNotMatchReturnsFalse()
            {
                string actualMessage = "abc";
                string expectedMessage = "xyz";

                var logEntry = new LogEntry(default, 0, () => actualMessage, default, null);

                logEntry.HasMessage(expectedMessage).Should().BeFalse();
            }

            [Fact]
            public void WhenLogEntryIsDefaultReturnsFalse()
            {
                string expectedMessage = "";

                default(LogEntry).HasMessage(expectedMessage).Should().BeFalse();
            }

            [Fact]
            public void ThrowsWhenParameterIsNull()
            {
                string actualMessage = "abc";
                string expectedMessage = null!;

                var logEntry = new LogEntry(default, 0, () => actualMessage, default, null);

                logEntry.Invoking(x => x.HasMessage(expectedMessage))
                    .Should().ThrowExactly<ArgumentNullException>();
            }
        }

        public class GivenMessageAndComparisonParameters
        {
            [Fact]
            public void WhenMessageMatchesReturnsTrue()
            {
                string actualMessage = "abc";
                string expectedMessage = "ABC";
                StringComparison comparison = StringComparison.OrdinalIgnoreCase;

                var logEntry = new LogEntry(default, 0, () => actualMessage, default, null);

                logEntry.HasMessage(expectedMessage, comparison).Should().BeTrue();
            }

            [Fact]
            public void WhenMessageDoesNotMatchReturnsFalse()
            {
                string actualMessage = "abc";
                string expectedMessage = "XYZ";
                StringComparison comparison = StringComparison.OrdinalIgnoreCase;

                var logEntry = new LogEntry(default, 0, () => actualMessage, default, null);

                logEntry.HasMessage(expectedMessage, comparison).Should().BeFalse();
            }

            [Fact]
            public void WhenLogEntryIsDefaultReturnsFalse()
            {
                string expectedMessage = "";
                StringComparison comparison = StringComparison.OrdinalIgnoreCase;

                default(LogEntry).HasMessage(expectedMessage, comparison).Should().BeFalse();
            }

            [Fact]
            public void ThrowsWhenMessageParameterIsNull()
            {
                string actualMessage = "abc";
                string expectedMessage = null!;
                StringComparison comparison = StringComparison.OrdinalIgnoreCase;

                var logEntry = new LogEntry(default, 0, () => actualMessage, default, null);

                logEntry.Invoking(x => x.HasMessage(expectedMessage, comparison))
                    .Should().ThrowExactly<ArgumentNullException>();
            }
        }

        public class GivenFunctionParameter
        {
            [Theory]
            [InlineData(true)]
            [InlineData(false)]
            public void ReturnsResultOfFunction(bool valueReturnedByFunction)
            {
                string actualMessage = "abc";
                string? capturedMessage = null;
                var messagePredicate = (string message) =>
                {
                    capturedMessage = message;
                    return valueReturnedByFunction;
                };

                var logEntry = new LogEntry(default, default, () => actualMessage, default, null);

                logEntry.HasMessage(messagePredicate).Should().Be(valueReturnedByFunction);

                capturedMessage.Should().Be(actualMessage);
            }

            [Fact]
            public void WhenLogEntryIsDefaultReturnsFalse()
            {
                var messagePredicate = (string message) => true;

                default(LogEntry).HasMessage(messagePredicate).Should().BeFalse();
            }

            [Fact]
            public void ThrowsWhenParameterIsNull()
            {
                string actualMessage = "abc";
                Func<string, bool> messagePredicate = null!;

                var logEntry = new LogEntry(default, 0, () => actualMessage, default, null);

                logEntry.Invoking(x => x.HasMessage(messagePredicate))
                    .Should().ThrowExactly<ArgumentNullException>();
            }
        }
    }

    public class HasMessageMatchingMethod
    {
        public class GivenPatternParameter
        {
            [Fact]
            public void WhenMessageMatchesReturnsTrue()
            {
                string actualMessage = "abc";
                string regexPattern = "^[abc]{3}$";

                var logEntry = new LogEntry(default, 0, () => actualMessage, default, null);

                logEntry.HasMessageMatching(regexPattern).Should().BeTrue();
            }

            [Fact]
            public void WhenMessageDoesNotMatchReturnsFalse()
            {
                string actualMessage = "abcc";
                string regexPattern = "^[abc]{3}$";

                var logEntry = new LogEntry(default, 0, () => actualMessage, default, null);

                logEntry.HasMessageMatching(regexPattern).Should().BeFalse();
            }

            [Fact]
            public void WhenLogEntryIsDefaultReturnsFalse()
            {
                string regexPattern = "^[abc]{3}$";

                default(LogEntry).HasMessageMatching(regexPattern).Should().BeFalse();
            }

            [Fact]
            public void ThrowsWhenParameterIsNull()
            {
                string actualMessage = "abc";
                string regexPattern = null!;

                var logEntry = new LogEntry(default, 0, () => actualMessage, default, null);

                logEntry.Invoking(x => x.HasMessageMatching(regexPattern))
                    .Should().ThrowExactly<ArgumentNullException>();
            }
        }

        public class GivenPatternAndOptionsParameters
        {
            [Fact]
            public void WhenMessageMatchesReturnsTrue()
            {
                string actualMessage = "ABC";
                string regexPattern = "^[abc]{3}$";
                RegexOptions regexOptions = RegexOptions.IgnoreCase;

                var logEntry = new LogEntry(default, 0, () => actualMessage, default, null);

                logEntry.HasMessageMatching(regexPattern, regexOptions).Should().BeTrue();
            }

            [Fact]
            public void WhenMessageDoesNotMatchReturnsFalse()
            {
                string actualMessage = "ABCC";
                string regexPattern = "^[abc]{3}$";
                RegexOptions regexOptions = RegexOptions.IgnoreCase;

                var logEntry = new LogEntry(default, 0, () => actualMessage, default, null);

                logEntry.HasMessageMatching(regexPattern, regexOptions).Should().BeFalse();
            }

            [Fact]
            public void WhenLogEntryIsDefaultReturnsFalse()
            {
                string regexPattern = "^[abc]{3}$";
                RegexOptions regexOptions = RegexOptions.IgnoreCase;

                default(LogEntry).HasMessageMatching(regexPattern, regexOptions).Should().BeFalse();
            }

            [Fact]
            public void ThrowsWhenRegexPatternParameterIsNull()
            {
                string actualMessage = "abc";
                string regexPattern = null!;
                RegexOptions regexOptions = RegexOptions.IgnoreCase;

                var logEntry = new LogEntry(default, 0, () => actualMessage, default, null);

                logEntry.Invoking(x => x.HasMessageMatching(regexPattern, regexOptions))
                    .Should().ThrowExactly<ArgumentNullException>();
            }
        }
    }

    public class HasAttributeMethod
    {
        public class GivenKeyParameter
        {
            [Fact]
            public void WhenKeyFoundReturnsTrue()
            {
                string myKey = "My Key";
                KeyValuePair<string, object> item = new(myKey, 123);

                var logEntry = new LogEntry(default, 0, () => "", new LogAttributes(new[] { item }), null);

                logEntry.HasAttribute(myKey).Should().BeTrue();
            }

            [Fact]
            public void WhenKeyNotFoundReturnsFalse()
            {
                string myKey = "My Key";
                KeyValuePair<string, object> item = new(myKey, 123);

                var logEntry = new LogEntry(default, 0, () => "", new LogAttributes(new[] { item }), null);

                logEntry.HasAttribute("Unknown Key").Should().BeFalse();
            }

            [Fact]
            public void WhenLogEntryIsDefaultReturnsFalse()
            {
                default(LogEntry).HasAttribute("My Key").Should().BeFalse();
            }

            [Fact]
            public void ThrowsWhenParameterIsNull()
            {
                string myKey = "My Key";
                KeyValuePair<string, object> item = new(myKey, 123);

                var logEntry = new LogEntry(default, 0, () => "", new LogAttributes(new[] { item }), null);

                logEntry.Invoking(x => x.HasAttribute(null!))
                    .Should().ThrowExactly<ArgumentNullException>();
            }
        }

        public class GivenKeyAndValueParameters
        {
            [Fact]
            public void WhenKeyFoundAndValueMatchesReturnsTrue()
            {
                string myKey = "My Key";
                object myValue = 123;
                KeyValuePair<string, object> item = new(myKey, myValue);

                var logEntry = new LogEntry(default, 0, () => "", new LogAttributes(new[] { item }), null);

                logEntry.HasAttribute(myKey, myValue).Should().BeTrue();
            }

            [Fact]
            public void WhenKeyNotFoundReturnsFalse()
            {
                string myKey = "My Key";
                object myValue = 123;
                KeyValuePair<string, object> item = new(myKey, myValue);

                var logEntry = new LogEntry(default, 0, () => "", new LogAttributes(new[] { item }), null);

                logEntry.HasAttribute("Unknown Key", myValue).Should().BeFalse();
            }

            [Fact]
            public void WhenKeyFoundButValueDoesNotMatchReturnsFalse()
            {
                string myKey = "My Key";
                object myValue = 123;
                string otherKey = "Other Key";
                object otherValue = 456.789;
                KeyValuePair<string, object> item = new(myKey, myValue);
                KeyValuePair<string, object> otherItem = new(otherKey, otherValue);

                var logEntry = new LogEntry(default, 0, () => "", new LogAttributes(new[] { item, otherItem }), null);

                logEntry.HasAttribute(myKey, otherValue).Should().BeFalse();
            }

            [Fact]
            public void WhenLogEntryIsDefaultReturnsFalse()
            {
                string myKey = "My Key";
                object myValue = 123;

                default(LogEntry).HasAttribute(myKey, myValue).Should().BeFalse();
            }

            [Fact]
            public void ThrowsWhenKeyParameterIsNull()
            {
                string myKey = "My Key";
                object myValue = 123;
                KeyValuePair<string, object> item = new(myKey, myValue);

                var logEntry = new LogEntry(default, 0, () => "", new LogAttributes(new[] { item }), null);

                logEntry.Invoking(x => x.HasAttribute(null!, myValue))
                    .Should().ThrowExactly<ArgumentNullException>();
            }

            [Fact]
            public void ThrowsWhenValueParameterIsNull()
            {
                string myKey = "My Key";
                object myValue = 123;
                KeyValuePair<string, object> item = new(myKey, myValue);

                var logEntry = new LogEntry(default, 0, () => "", new LogAttributes(new[] { item }), null);

                logEntry.Invoking(x => x.HasAttribute(myKey, null!))
                    .Should().ThrowExactly<ArgumentNullException>();
            }
        }

        public class GivenKeyAndFunctionParameters
        {
            [Fact]
            public void WhenKeyFoundAndValueMatchesReturnsTrue()
            {
                string myKey = "My Key";
                object myValue = 123;
                KeyValuePair<string, object> item = new(myKey, myValue);

                object? capturedValue = null;
                var valuePredicate = (object value) =>
                {
                    capturedValue = value;
                    return true;
                };

                var logEntry = new LogEntry(default, 0, () => "", new LogAttributes(new[] { item }), null);

                logEntry.HasAttribute(myKey, valuePredicate).Should().BeTrue();

                capturedValue.Should().Be(myValue);
            }

            [Fact]
            public void WhenKeyNotFoundReturnsFalse()
            {
                string myKey = "My Key";
                object myValue = 123;
                KeyValuePair<string, object> item = new(myKey, myValue);

                object? capturedValue = null;
                var valuePredicate = (object value) =>
                {
                    capturedValue = value;
                    return true;
                };

                var logEntry = new LogEntry(default, 0, () => "", new LogAttributes(new[] { item }), null);

                logEntry.HasAttribute("Unknown Key", valuePredicate).Should().BeFalse();

                capturedValue.Should().BeNull();
            }

            [Fact]
            public void WhenValueDoesNotMatchPredicateReturnsFalse()
            {
                string myKey = "My Key";
                object myValue = 123;
                string otherKey = "Other Key";
                object otherValue = 456.789;
                KeyValuePair<string, object> item = new(myKey, myValue);
                KeyValuePair<string, object> otherItem = new(otherKey, otherValue);

                object? capturedValue = null;
                var valuePredicate = (object value) =>
                {
                    capturedValue = value;
                    return false;
                };

                var logEntry = new LogEntry(default, 0, () => "", new LogAttributes(new[] { item, otherItem }), null);

                logEntry.HasAttribute(myKey, valuePredicate).Should().BeFalse();

                capturedValue.Should().Be(myValue);
            }

            [Fact]
            public void WhenLogEntryIsDefaultReturnsFalse()
            {
                string myKey = "My Key";

                object? capturedValue = null;
                var valuePredicate = (object value) =>
                {
                    capturedValue = value;
                    return false;
                };

                default(LogEntry).HasAttribute(myKey, valuePredicate).Should().BeFalse();

                capturedValue.Should().BeNull();
            }

            [Fact]
            public void ThrowsWhenKeyParameterIsNull()
            {
                string myKey = "My Key";
                object myValue = 123;
                KeyValuePair<string, object> item = new(myKey, myValue);

                object? capturedValue = null;
                var valuePredicate = (object value) =>
                {
                    capturedValue = value;
                    return false;
                };

                var logEntry = new LogEntry(default, 0, () => "", new LogAttributes(new[] { item }), null);

                logEntry.Invoking(x => x.HasAttribute(null!, valuePredicate))
                    .Should().ThrowExactly<ArgumentNullException>();

                capturedValue.Should().BeNull();
            }

            [Fact]
            public void ThrowsWhenFunctionParameterIsNull()
            {
                string myKey = "My Key";
                object myValue = 123;
                KeyValuePair<string, object> item = new(myKey, myValue);

                Func<object, bool> valuePredicate = null!;

                var logEntry = new LogEntry(default, 0, () => "", new LogAttributes(new[] { item }), null);

                logEntry.Invoking(x => x.HasAttribute(myKey, valuePredicate))
                    .Should().ThrowExactly<ArgumentNullException>();
            }
        }

        public class OfTypeT
        {
            public class GivenKeyAndValueParameters
            {
                [Fact]
                public void WhenKeyFoundAndValueMatchesReturnsTrue()
                {
                    string myKey = "My Key";
                    int myValue = 123;
                    KeyValuePair<string, object> item = new(myKey, myValue);

                    var logEntry = new LogEntry(default, 0, () => "", new LogAttributes(new[] { item }), null);

                    logEntry.HasAttribute(myKey, myValue).Should().BeTrue();
                }

                [Fact]
                public void WhenKeyNotFoundReturnsFalse()
                {
                    string myKey = "My Key";
                    int myValue = 123;
                    KeyValuePair<string, object> item = new(myKey, myValue);

                    var logEntry = new LogEntry(default, 0, () => "", new LogAttributes(new[] { item }), null);

                    logEntry.HasAttribute("Unknown Key", myValue).Should().BeFalse();
                }

                [Fact]
                public void WhenKeyFoundButValueDoesNotMatchReturnsFalse()
                {
                    string myKey = "My Key";
                    int myValue = 123;
                    string otherKey = "Other Key";
                    double otherValue = 456.789;
                    KeyValuePair<string, object> item = new(myKey, myValue);
                    KeyValuePair<string, object> otherItem = new(otherKey, otherValue);

                    var logEntry = new LogEntry(default, 0, () => "", new LogAttributes(new[] { item, otherItem }), null);

                    logEntry.HasAttribute(myKey, otherValue).Should().BeFalse();
                }

                [Fact]
                public void WhenLogEntryIsDefaultReturnsFalse()
                {
                    string myKey = "My Key";
                    int myValue = 123;

                    default(LogEntry).HasAttribute(myKey, myValue).Should().BeFalse();
                }

                [Fact]
                public void ThrowsWhenKeyParameterIsNull()
                {
                    string myKey = "My Key";
                    int myValue = 123;
                    KeyValuePair<string, object> item = new(myKey, myValue);

                    var logEntry = new LogEntry(default, 0, () => "", new LogAttributes(new[] { item }), null);

                    logEntry.Invoking(x => x.HasAttribute(null!, myValue))
                        .Should().ThrowExactly<ArgumentNullException>();
                }

                [Fact]
                public void ThrowsWhenValueParameterIsNull()
                {
                    string myKey = "My Key";
                    object myValue = "My Value";
                    KeyValuePair<string, object> item = new(myKey, myValue);

                    var logEntry = new LogEntry(default, 0, () => "", new LogAttributes(new[] { item }), null);

                    logEntry.Invoking(x => x.HasAttribute(myKey, (string)null!))
                        .Should().ThrowExactly<ArgumentNullException>();
                }
            }

            public class GivenKeyAndFunctionParameters
            {
                [Fact]
                public void WhenKeyFoundAndValueMatchesReturnsTrue()
                {
                    string myKey = "My Key";
                    int myValue = 123;
                    KeyValuePair<string, object> item = new(myKey, myValue);

                    int? capturedValue = null;
                    var valuePredicate = (int value) =>
                    {
                        capturedValue = value;
                        return true;
                    };

                    var logEntry = new LogEntry(default, 0, () => "", new LogAttributes(new[] { item }), null);

                    logEntry.HasAttribute(myKey, valuePredicate).Should().BeTrue();

                    capturedValue.Should().Be(myValue);
                }

                [Fact]
                public void WhenKeyNotFoundReturnsFalse()
                {
                    string myKey = "My Key";
                    int myValue = 123;
                    KeyValuePair<string, object> item = new(myKey, myValue);

                    int? capturedValue = null;
                    var valuePredicate = (int value) =>
                    {
                        capturedValue = value;
                        return true;
                    };

                    var logEntry = new LogEntry(default, 0, () => "", new LogAttributes(new[] { item }), null);

                    logEntry.HasAttribute("Unknown Key", valuePredicate).Should().BeFalse();

                    capturedValue.Should().BeNull();
                }

                [Fact]
                public void WhenValueDoesNotMatchPredicateReturnsFalse()
                {
                    string myKey = "My Key";
                    int myValue = 123;
                    string otherKey = "Other Key";
                    double otherValue = 456.789;
                    KeyValuePair<string, object> item = new(myKey, myValue);
                    KeyValuePair<string, object> otherItem = new(otherKey, otherValue);

                    int? capturedValue = null;
                    var valuePredicate = (int value) =>
                    {
                        capturedValue = value;
                        return false;
                    };

                    var logEntry = new LogEntry(default, 0, () => "", new LogAttributes(new[] { item, otherItem }), null);

                    logEntry.HasAttribute(myKey, valuePredicate).Should().BeFalse();

                    capturedValue.Should().Be(myValue);
                }

                [Fact]
                public void WhenLogEntryIsDefaultReturnsFalse()
                {
                    string myKey = "My Key";

                    int? capturedValue = null;
                    var valuePredicate = (int value) =>
                    {
                        capturedValue = value;
                        return false;
                    };

                    default(LogEntry).HasAttribute(myKey, valuePredicate).Should().BeFalse();

                    capturedValue.Should().BeNull();
                }

                [Fact]
                public void ThrowsWhenKeyParameterIsNull()
                {
                    string myKey = "My Key";
                    int myValue = 123;
                    KeyValuePair<string, object> item = new(myKey, myValue);

                    int? capturedValue = null;
                    var valuePredicate = (int value) =>
                    {
                        capturedValue = value;
                        return false;
                    };

                    var logEntry = new LogEntry(default, 0, () => "", new LogAttributes(new[] { item }), null);

                    logEntry.Invoking(x => x.HasAttribute(null!, valuePredicate))
                        .Should().ThrowExactly<ArgumentNullException>();

                    capturedValue.Should().BeNull();
                }

                [Fact]
                public void ThrowsWhenFunctionParameterIsNull()
                {
                    string myKey = "My Key";
                    int myValue = 123;
                    KeyValuePair<string, object> item = new(myKey, myValue);

                    Func<int, bool> valuePredicate = null!;

                    var logEntry = new LogEntry(default, 0, () => "", new LogAttributes(new[] { item }), null);

                    logEntry.Invoking(x => x.HasAttribute(myKey, valuePredicate))
                        .Should().ThrowExactly<ArgumentNullException>();
                }
            }
        }
    }

    public class HasNoStateMethod
    {
        [Fact]
        public void WhenStateIsNullReturnsTrue()
        {
            var logEntry = new LogEntry(default, 0, () => "", new LogAttributes(null), null);

            logEntry.HasNoState().Should().BeTrue();
        }

        [Fact]
        public void WhenStateIsNotNullReturnsFalse()
        {
            var logEntry = new LogEntry(default, 0, () => "", new LogAttributes(123), null);

            logEntry.HasNoState().Should().BeFalse();
        }

        [Fact]
        public void WhenLogEntryIsDefaultReturnsTrue()
        {
            default(LogEntry).HasNoState().Should().BeTrue();
        }
    }

    public class HasStateMethod
    {
        public class GivenNoParameters
        {
            [Fact]
            public void WhenStateIsNotNullReturnsTrue()
            {
                var logEntry = new LogEntry(default, 0, () => "", new LogAttributes(123), null);

                logEntry.HasState().Should().BeTrue();
            }

            [Fact]
            public void WhenStateIsNullReturnsFalse()
            {
                var logEntry = new LogEntry(default, 0, () => "", new LogAttributes(null), null);

                logEntry.HasState().Should().BeFalse();
            }

            [Fact]
            public void WhenLogEntryIsDefaultReturnsFalse()
            {
                default(LogEntry).HasState().Should().BeFalse();
            }
        }

        public class GivenStateParameter
        {
            [Theory]
            [InlineData("abc", "abc")]
            [InlineData(null, null)]
            public void WhenStateMatchesReturnsTrue(object? actualState, object? expectedState)
            {
                var logEntry = new LogEntry(default, 0, () => "", new LogAttributes(actualState), null);

                logEntry.HasState(expectedState).Should().BeTrue();
            }

            [Theory]
            [InlineData("abc", "xyz")]
            [InlineData("abc", null)]
            [InlineData(null, "abc")]
            public void WhenStateDoesNotMatchReturnsFalse(object? actualState, object? expectedState)
            {
                var logEntry = new LogEntry(default, 0, () => "", new LogAttributes(actualState), null);

                logEntry.HasState(expectedState).Should().BeFalse();
            }

            [Theory]
            [InlineData("abc")]
            [InlineData(null)]
            public void WhenLogEntryIsDefaultReturnsWhetherExpectedStateIsNull(object? expectedState)
            {
                default(LogEntry).HasState(expectedState).Should().Be(expectedState is null);
            }
        }

        public class GivenFunctionParameter
        {
            [Theory]
            [InlineData(true)]
            [InlineData(false)]
            public void ReturnsResultOfFunction(bool valueReturnedByFunction)
            {
                object? actualState = "abc";
                object? capturedState = null;

                var statePredicate = (object? state) =>
                {
                    capturedState = state;
                    return valueReturnedByFunction;
                };

                var logEntry = new LogEntry(default, 0, () => "", new LogAttributes(actualState), null);

                logEntry.HasState(statePredicate).Should().Be(valueReturnedByFunction);

                capturedState.Should().BeSameAs(actualState);
            }

            [Fact]
            public void WhenLogEntryIsDefaultReturnsResultOfFunction()
            {
                object? capturedState = "abc";
                bool valueReturnedByFunction = true;

                var statePredicate = (object? state) =>
                {
                    capturedState = state;
                    return valueReturnedByFunction;
                };

                default(LogEntry).HasState(statePredicate).Should().Be(valueReturnedByFunction);

                capturedState.Should().BeNull();
            }

            [Fact]
            public void ThrowsWhenParameterIsNull()
            {
                object? actualState = "abc";
                Func<object?, bool> statePredicate = null!;

                var logEntry = new LogEntry(default, 0, () => "", new LogAttributes(actualState), null);

                logEntry.Invoking(x => x.HasState(statePredicate))
                    .Should().ThrowExactly<ArgumentNullException>();
            }
        }

        public class OfTypeTState
        {
            public class GivenStateParameter
            {
                [Theory]
                [InlineData("abc", "abc")]
                [InlineData(null, null)]
                public void WhenStateMatchesReturnsTrue(string? actualState, string? expectedState)
                {
                    var logEntry = new LogEntry(default, 0, () => "", new LogAttributes(actualState), null);

                    logEntry.HasState(expectedState).Should().BeTrue();
                }

                [Theory]
                [InlineData("abc", "xyz")]
                [InlineData("abc", null)]
                [InlineData(null, "abc")]
                public void WhenStateDoesNotMatchReturnsFalse(string? actualState, string? expectedState)
                {
                    var logEntry = new LogEntry(default, 0, () => "", new LogAttributes(actualState), null);

                    logEntry.HasState(expectedState).Should().BeFalse();
                }

                [Theory]
                [InlineData("abc")]
                [InlineData(null)]
                public void WhenLogEntryIsDefaultReturnsWhetherExpectedStateIsNull(string? expectedState)
                {
                    default(LogEntry).HasState(expectedState).Should().Be(expectedState is null);
                }
            }

            public class GivenFunctionParameter
            {
                [Theory]
                [InlineData(true)]
                [InlineData(false)]
                public void ReturnsResultOfFunction(bool valueReturnedByFunction)
                {
                    string? actualState = "abc";
                    string? capturedState = null;

                    var statePredicate = (string? state) =>
                    {
                        capturedState = state;
                        return valueReturnedByFunction;
                    };

                    var logEntry = new LogEntry(default, 0, () => "", new LogAttributes(actualState), null);

                    logEntry.HasState(statePredicate).Should().Be(valueReturnedByFunction);

                    capturedState.Should().BeSameAs(actualState);
                }

                [Fact]
                public void WhenLogEntryIsDefaultReturnsFalse()
                {
                    string? capturedState = null;

                    var statePredicate = (string? state) =>
                    {
                        capturedState = state;
                        return true;
                    };

                    default(LogEntry).HasState(statePredicate).Should().BeFalse();

                    capturedState.Should().BeNull();
                }

                [Fact]
                public void ThrowsWhenParameterIsNull()
                {
                    string? actualState = "abc";
                    Func<string?, bool> statePredicate = null!;

                    var logEntry = new LogEntry(default, 0, () => "", new LogAttributes(actualState), null);

                    logEntry.Invoking(x => x.HasState(statePredicate))
                        .Should().ThrowExactly<ArgumentNullException>();
                }
            }
        }
    }

    public class HasNoScopeMethod
    {
        [Fact]
        public void WhenScopeIsNullReturnsTrue()
        {
            var logEntry = new LogEntry(default, 0, () => "", new LogAttributes(null), null);

            logEntry.HasNoScope().Should().BeTrue();
        }

        [Fact]
        public void WhenScopeIsNotNullReturnsFalse()
        {
            var logEntry = new LogEntry(default, 0, () => "", new LogAttributes(null, 123), null);

            logEntry.HasNoScope().Should().BeFalse();
        }

        [Fact]
        public void WhenLogEntryIsDefaultReturnsTrue()
        {
            default(LogEntry).HasNoScope().Should().BeTrue();
        }
    }

    public class HasScopeMethod
    {
        public class GivenNoParameters
        {
            [Fact]
            public void WhenScopeIsNotNullReturnsTrue()
            {
                var logEntry = new LogEntry(default, 0, () => "", new LogAttributes(null, 123), null);

                logEntry.HasScope().Should().BeTrue();
            }

            [Fact]
            public void WhenScopeIsNullReturnsFalse()
            {
                var logEntry = new LogEntry(default, 0, () => "", new LogAttributes(null), null);

                logEntry.HasScope().Should().BeFalse();
            }

            [Fact]
            public void WhenLogEntryIsDefaultReturnsFalse()
            {
                default(LogEntry).HasScope().Should().BeFalse();
            }
        }

        public class GivenScopeParameter
        {
            [Theory]
            [InlineData("abc", "abc")]
            [InlineData(null, null)]
            public void WhenScopeMatchesReturnsTrue(object? actualScope, object? expectedScope)
            {
                var attributes = 
                    actualScope is not null
                    ? new LogAttributes(null, actualScope)
                    : new LogAttributes(null);
                var logEntry = new LogEntry(default, 0, () => "", attributes, null);

                logEntry.HasScope(expectedScope).Should().BeTrue();
            }

            [Theory]
            [InlineData("abc", "xyz")]
            [InlineData("abc", null)]
            [InlineData(null, "abc")]
            public void WhenScopeDoesNotMatchReturnsFalse(object? actualScope, object? expectedScope)
            {
                var attributes =
                    actualScope is not null
                    ? new LogAttributes(null, actualScope)
                    : new LogAttributes(null);
                var logEntry = new LogEntry(default, 0, () => "", attributes, null);

                logEntry.HasScope(expectedScope).Should().BeFalse();
            }

            [Theory]
            [InlineData("abc")]
            [InlineData(null)]
            public void WhenLogEntryIsDefaultReturnsWhetherExpectedScopeIsNull(object? expectedScope)
            {
                default(LogEntry).HasScope(expectedScope).Should().Be(expectedScope is null);
            }
        }

        public class GivenFunctionParameter
        {
            [Theory]
            [InlineData(true)]
            [InlineData(false)]
            public void ReturnsResultOfFunction(bool valueReturnedByFunction)
            {
                object? actualScope = "abc";
                object? capturedScope = null;

                var scopePredicate = (object scope) =>
                {
                    capturedScope = scope;
                    return valueReturnedByFunction;
                };

                var logEntry = new LogEntry(default, default, () => "", new LogAttributes(null, actualScope), null);

                logEntry.HasScope(scopePredicate).Should().Be(valueReturnedByFunction);

                capturedScope.Should().BeSameAs(actualScope);
            }

            [Fact]
            public void WhenLogEntryIsDefaultReturnsFalse()
            {
                object? capturedScope = null;

                var scopePredicate = (object scope) =>
                {
                    capturedScope = scope;
                    return true;
                };

                default(LogEntry).HasScope(scopePredicate).Should().BeFalse();

                capturedScope.Should().BeNull();
            }

            [Fact]
            public void ThrowsWhenParameterIsNull()
            {
                object actualScope = "abc";
                Func<object, bool> scopePredicate = null!;

                var logEntry = new LogEntry(default, 0, () => "", new LogAttributes(null, actualScope), null);

                logEntry.Invoking(x => x.HasScope(scopePredicate))
                    .Should().ThrowExactly<ArgumentNullException>();
            }
        }

        public class OfTypeTState
        {
            public class GivenScopeParameter
            {
                [Theory]
                [InlineData("abc", "abc")]
                [InlineData(null, null)]
                public void WhenScopeMatchesReturnsTrue(string? actualScope, string? expectedScope)
                {
                    var attributes =
                        actualScope is not null
                        ? new LogAttributes(null, actualScope)
                        : new LogAttributes(null);
                    var logEntry = new LogEntry(default, 0, () => "", attributes, null);

                    logEntry.HasScope(expectedScope).Should().BeTrue();
                }

                [Theory]
                [InlineData("abc", "xyz")]
                [InlineData("abc", null)]
                [InlineData(null, "abc")]
                public void WhenScopeDoesNotMatchReturnsFalse(string? actualScope, string? expectedScope)
                {
                    var attributes =
                        actualScope is not null
                        ? new LogAttributes(null, actualScope)
                        : new LogAttributes(null);
                    var logEntry = new LogEntry(default, 0, () => "", attributes, null);

                    logEntry.HasScope(expectedScope).Should().BeFalse();
                }

                [Theory]
                [InlineData("abc")]
                [InlineData(null)]
                public void WhenLogEntryIsDefaultReturnsWhetherExpectedScopeIsNull(string? expectedScope)
                {
                    default(LogEntry).HasScope(expectedScope).Should().Be(expectedScope is null);
                }
            }

            public class GivenFunctionParameter
            {
                [Theory]
                [InlineData(true)]
                [InlineData(false)]
                public void ReturnsResultOfFunction(bool valueReturnedByFunction)
                {
                    string? actualScope = "abc";
                    string? capturedScope = null;

                    var scopePredicate = (string scope) =>
                    {
                        capturedScope = scope;
                        return valueReturnedByFunction;
                    };

                    var logEntry = new LogEntry(default, default, () => "", new LogAttributes(null, actualScope), null);

                    logEntry.HasScope(scopePredicate).Should().Be(valueReturnedByFunction);

                    capturedScope.Should().BeSameAs(actualScope);
                }

                [Fact]
                public void WhenLogEntryIsDefaultReturnsFalse()
                {
                    string? capturedScope = null;

                    var scopePredicate = (string scope) =>
                    {
                        capturedScope = scope;
                        return true;
                    };

                    default(LogEntry).HasScope(scopePredicate).Should().BeFalse();

                    capturedScope.Should().BeNull();
                }

                [Fact]
                public void ThrowsWhenParameterIsNull()
                {
                    string actualScope = "abc";
                    Func<string, bool> scopePredicate = null!;

                    var logEntry = new LogEntry(default, 0, () => "", new LogAttributes(null, actualScope), null);

                    logEntry.Invoking(x => x.HasScope(scopePredicate))
                        .Should().ThrowExactly<ArgumentNullException>();
                }
            }
        }
    }

    public class HasNoExceptionMethod
    {
        [Fact]
        public void WhenExceptionIsNullReturnsTrue()
        {
            var logEntry = new LogEntry(default, 0, () => "", default, null);

            logEntry.HasNoException().Should().BeTrue();
        }

        [Fact]
        public void WhenExceptionIsNotNullReturnsFalse()
        {
            var logEntry = new LogEntry(default, 0, () => "", default, new Exception());

            logEntry.HasNoException().Should().BeFalse();
        }

        [Fact]
        public void WhenLogEntryIsDefaultReturnsTrue()
        {
            default(LogEntry).HasNoException().Should().BeTrue();
        }
    }

    public class HasExceptionMethod
    {
        public class GivenNoParameters
        {
            [Fact]
            public void WhenExceptionIsNotNullReturnsTrue()
            {
                var logEntry = new LogEntry(default, 0, () => "", default, new Exception());

                logEntry.HasException().Should().BeTrue();
            }

            [Fact]
            public void WhenExceptionIsNullReturnsFalse()
            {
                var logEntry = new LogEntry(default, 0, () => "", default, null);

                logEntry.HasException().Should().BeFalse();
            }

            [Fact]
            public void WhenLogEntryIsDefaultReturnsFalse()
            {
                default(LogEntry).HasException().Should().BeFalse();
            }
        }

        public class GivenExceptionParameter
        {
            [Fact]
            public void WhenExceptionMatchesReturnsTrue()
            {
                Exception? actualException = new();
                Exception? expectedException = actualException;

                var logEntry = new LogEntry(default, 0, () => "", default, actualException);

                logEntry.HasException(expectedException).Should().BeTrue();
            }

            [Fact]
            public void WhenExceptionDoesNotMatchReturnsFalse()
            {
                Exception? actualException = new();
                Exception? expectedException = new();

                var logEntry = new LogEntry(default, 0, () => "", default, actualException);

                logEntry.HasException(expectedException).Should().BeFalse();
            }

            [Fact]
            public void WhenActualAndExpectedExceptionAreNullReturnsTrue()
            {
                Exception? actualException = null;
                Exception? expectedException = null;

                var logEntry = new LogEntry(default, 0, () => "", default, actualException);

                logEntry.HasException(expectedException).Should().BeTrue();
            }

            [Fact]
            public void WhenLogEntryIsDefaultReturnsFalse()
            {
                Exception? expectedException = new();

                default(LogEntry).HasException(expectedException).Should().BeFalse();
            }
        }

        public class GivenFunctionParameter
        {
            [Theory]
            [InlineData(true)]
            [InlineData(false)]
            public void ReturnsResultOfFunction(bool valueReturnedByFunction)
            {
                Exception? actualException = new();
                Exception? capturedException = null;
                var exceptionPredicate = (Exception? exception) =>
                {
                    capturedException = exception;
                    return valueReturnedByFunction;
                };

                var logEntry = new LogEntry(default, default, () => "", default, actualException);

                logEntry.HasException(exceptionPredicate).Should().Be(valueReturnedByFunction);

                capturedException.Should().Be(actualException);
            }

            [Fact]
            public void WhenLogEntryIsDefaultReturnsResultOfFunction()
            {
                Exception? capturedException = new();
                bool valueReturnedByFunction = true;

                var exceptionPredicate = (Exception? exception) =>
                {
                    capturedException = exception;
                    return valueReturnedByFunction;
                };

                default(LogEntry).HasException(exceptionPredicate).Should().Be(valueReturnedByFunction);

                capturedException.Should().BeNull();
            }

            [Fact]
            public void ThrowsWhenParameterIsNull()
            {
                Exception? actualException = new();
                Func<Exception?, bool> exceptionPredicate = null!;

                var logEntry = new LogEntry(default, 0, () => "", default, actualException);

                logEntry.Invoking(x => x.HasException(exceptionPredicate))
                    .Should().ThrowExactly<ArgumentNullException>();
            }
        }

        public class OfTypeTException
        {
            public class GivenNoParameters
            {
                [Fact]
                public void WhenExceptionHasSameTypeAsTExceptionReturnsTrue()
                {
                    var logEntry = new LogEntry(default, 0, () => "", default, new InvalidOperationException());

                    logEntry.HasException<InvalidOperationException>().Should().BeTrue();
                }

                [Fact]
                public void WhenExceptionHasDifferentTypeFromTExceptionReturnsFalse()
                {
                    var logEntry = new LogEntry(default, 0, () => "", default, new ArgumentException());

                    logEntry.HasException<InvalidOperationException>().Should().BeFalse();
                }

                [Fact]
                public void WhenExceptionIsNullReturnsFalse()
                {
                    var logEntry = new LogEntry(default, 0, () => "", default, null);

                    logEntry.HasException<InvalidOperationException>().Should().BeFalse();
                }

                [Fact]
                public void WhenLogEntryIsDefaultReturnsFalse()
                {
                    default(LogEntry).HasException<InvalidOperationException>().Should().BeFalse();
                }
            }

            public class GivenFunctionParameter
            {
                //[Theory]
                //[InlineData(true)]
                //[InlineData(false)]
                //public void ReturnsResultOfFunction(bool valueReturnedByFunction)
                //{
                //    // TODO: Capture
                //    var exceptionPredicate = (InvalidOperationException exception) => valueReturnedByFunction;

                //    var logEntry = new LogEntry(default, default, () => "", default, new InvalidOperationException());

                //    logEntry.HasException(exceptionPredicate).Should().Be(valueReturnedByFunction);
                //}

                ////[Fact]
                ////public void WhenLogEntryIsDefaultReturnsFalse()
                ////{
                ////    var messagePredicate = (string message) => true;

                ////    default(LogEntry).HasMessage(messagePredicate).Should().BeFalse();
                ////}

                ////[Fact]
                ////public void ThrowsWhenParameterIsNull()
                ////{
                ////    string actualMessage = "abc";
                ////    Func<string, bool> messagePredicate = null!;

                ////    var logEntry = new LogEntry(default, 0, () => actualMessage, default, null);

                ////    logEntry.Invoking(x => x.HasMessage(messagePredicate))
                ////        .Should().ThrowExactly<ArgumentNullException>();
                ////}

                [Theory]
                [InlineData(true)]
                [InlineData(false)]
                public void ReturnsResultOfFunction(bool valueReturnedByFunction)
                {
                    InvalidOperationException? actualException = new();
                    InvalidOperationException? capturedException = null;
                    var exceptionPredicate = (InvalidOperationException exception) =>
                    {
                        capturedException = exception;
                        return valueReturnedByFunction;
                    };

                    var logEntry = new LogEntry(default, default, () => "", default, actualException);

                    logEntry.HasException(exceptionPredicate).Should().Be(valueReturnedByFunction);

                    capturedException.Should().Be(actualException);
                }

                [Fact]
                public void WhenLogEntryIsDefaultReturnsFalse()
                {
                    Exception? capturedException = null;

                    var exceptionPredicate = (InvalidOperationException exception) =>
                    {
                        capturedException = exception;
                        return true;
                    };

                    default(LogEntry).HasException(exceptionPredicate).Should().BeFalse();

                    capturedException.Should().BeNull();
                }

                [Fact]
                public void ThrowsWhenParameterIsNull()
                {
                    InvalidOperationException actualException = new();
                    Func<InvalidOperationException, bool> exceptionPredicate = null!;

                    var logEntry = new LogEntry(default, 0, () => "", default, actualException);

                    logEntry.Invoking(x => x.HasException(exceptionPredicate))
                        .Should().ThrowExactly<ArgumentNullException>();
                }
            }
        }
    }

    public class ToStringMethod
    {
        [Fact]
        public void WorksGivenOnlyLogLevel()
        {
            var logEntry = new LogEntry(LogLevel.Warning, 0, () => "", new LogAttributes(null), null);

            logEntry.ToString().Should().Be("""
            {
             LogLevel = Warning 
            }
            """);
        }

        [Fact]
        public void WorksGivenLogLevelAndEventId()
        {
            var logEntry = new LogEntry(LogLevel.Warning, 123, () => "", new LogAttributes(null), null);

            logEntry.ToString().Should().Be("""
            {
             LogLevel = Warning,
             EventId = 123 
            }
            """);
        }

        [Fact]
        public void WorksGivenLogLevelAndMessage()
        {
            var logEntry = new LogEntry(LogLevel.Warning, 0, () => "Hello world!", new LogAttributes(null), null);

            logEntry.ToString().Should().Be("""
            {
             LogLevel = Warning,
             Message = Hello world! 
            }
            """);
        }

        [Fact]
        public void WorksGivenLogLevelAndState()
        {
            var state = new DictionaryWithOverriddenToStringMethod("Hello world!")
            {
                { "foo", "abc" },
                { "bar", 123 }
            };

            var logEntry = new LogEntry(LogLevel.Warning, 0, () => "", new LogAttributes(state), null);

            logEntry.ToString().Should().Be("""
            {
             LogLevel = Warning,
             State = Hello world! { [foo] = abc, [bar] = 123 } 
            }
            """);
        }

        [Fact]
        public void WorksGivenLogLevelAndScope()
        {
            var scope = new DictionaryWithOverriddenToStringMethod("Hello world!")
            {
                { "foo", "abc" },
                { "bar", 123 }
            };

            var logEntry = new LogEntry(LogLevel.Warning, 0, () => "", new LogAttributes(null, scope), null);

            logEntry.ToString().Should().Be("""
            {
             LogLevel = Warning,
             Scope = Hello world! { [foo] = abc, [bar] = 123 } 
            }
            """);
        }

        [Fact]
        public void WorksGivenLogLevelAndScopeWithParentScope()
        {
            var innerScope = new DictionaryWithOverriddenToStringMethod("Hello world!")
            {
                { "foo", "abc" },
                { "bar", 123 }
            };

            var outerScope = new DictionaryWithOverriddenToStringMethod("Good-bye world!")
            {
                { "baz", "xyz" },
                { "qux", 789}
            };

            var logEntry = new LogEntry(LogLevel.Warning, 0, () => "", new LogAttributes(null, innerScope, outerScope), null);

            logEntry.ToString().Should().Be("""
            {
             LogLevel = Warning,
             Scope = Hello world! { [foo] = abc, [bar] = 123 },
             ParentScope = Good-bye world! { [baz] = xyz, [qux] = 789 } 
            }
            """);
        }

        [Fact]
        public void WorksGivenLogLevelAndException()
        {
            var exception = new InvalidOperationException("Oh, no!");

            var logEntry = new LogEntry(LogLevel.Warning, 0, () => "", new LogAttributes(null), exception);

            logEntry.ToString().Should().Be("""
            {
             LogLevel = Warning,
             Exception = System.InvalidOperationException: Oh, no! 
            }
            """);
        }

        [Fact]
        public void WorksGivenAllFields()
        {
            var state = new DictionaryWithOverriddenToStringMethod("Hello world!")
            {
                { "foo", "abc" }
            };

            var innerScope = new DictionaryWithOverriddenToStringMethod("Good-bye world!")
            {
                { "bar", 123 }
            };

            var outerScope = new DictionaryWithOverriddenToStringMethod("Greetings world!")
            {
                { "baz", true }
            };

            var exception = new InvalidOperationException("Oh, no!");

            var logEntry = new LogEntry(LogLevel.Warning, 123, () => "Well, well, well.", new LogAttributes(state, innerScope, outerScope), exception);

            logEntry.ToString().Should().Be("""
                {
                 LogLevel = Warning,
                 EventId = 123,
                 Message = Well, well, well.,
                 State = Hello world! { [foo] = abc },
                 Scope = Good-bye world! { [bar] = 123 },
                 ParentScope = Greetings world! { [baz] = True },
                 Exception = System.InvalidOperationException: Oh, no! 
                }
                """);
        }
    }
}

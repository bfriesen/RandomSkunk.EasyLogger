using FluentAssertions;
using Microsoft.Extensions.Logging;

namespace RandomSkunk.EasyLogging.Tests;

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

    // TODO: Add tests for log entry's "query" methods, e.g. IsDebug(), HasException(), etc.
 
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

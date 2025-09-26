using FluentAssertions;
using Microsoft.Extensions.Logging;

namespace RandomSkunk.Logging.Tests;

public class HighPerformanceLoggerTests
{
    public class Identity
    {
        [Fact]
        public void HighPerformanceLoggerImplementsILoggerInterface()
        {
            HighPerformanceLogger logger = new ConcreteHighPerformanceLogger();

            logger.Should().BeAssignableTo<ILogger>();
        }

        [Fact]
        public void HighPerformanceLoggerOfTCategoryNameInheritsFromHighPerformanceLogger()
        {
            HighPerformanceLogger<Identity> logger = new ConcreteHighPerformanceLogger<Identity>();

            logger.Should().BeAssignableTo<HighPerformanceLogger>();
        }

        [Fact]
        public void HighPerformanceLoggerOfTCategoryNameImplementsILoggerOfTCategoryName()
        {
            HighPerformanceLogger<Identity> logger = new ConcreteHighPerformanceLogger<Identity>();

            logger.Should().BeAssignableTo<ILogger<Identity>>();
        }
    }

    public class CategoryProperty
    {
        public class GivenNonGenericHighPerformanceLogger
        {
            [Fact]
            public void ReturnsTheConcreteLoggerTypeName()
            {
                HighPerformanceLogger logger = new ConcreteHighPerformanceLogger();
                logger.Category.Should().Be(typeof(ConcreteHighPerformanceLogger).ToString());
            }
        }

        public class GivenGenericHighPerformanceLogger
        {
            [Fact]
            public void ReturnsTheTypeNameOfTCategoryName()
            {
                HighPerformanceLogger<Identity> logger = new ConcreteHighPerformanceLogger<Identity>();
                logger.Category.Should().Be(typeof(Identity).ToString());
            }
        }
    }

    public class LogMethod
    {
        [Fact]
        public void CallsWriteLogEntryWhenIsEnabledIsTrue()
        {
            var logger = new CapturingHighPerformanceLogger();

            LogLevel logLevel = LogLevel.Information;
            EventId eventId = 123;
            decimal state = 987.654M;
            var exception = new InvalidOperationException("Oh, no!");
            string message = "Hello world!";

            decimal? formatterCapturedState = null;
            Exception? formatterCapturedException = null;

            var formatter = (decimal s, Exception? e) =>
            {
                formatterCapturedState = s;
                formatterCapturedException = e;
                return message;
            };

            logger.Log(logLevel, eventId, state, exception, formatter);

            // The formatter should be called when the log entry's Message property is accessed, not at initialization.
            formatterCapturedState.Should().BeNull();
            formatterCapturedException.Should().BeNull();

            Assert.NotNull(logger.CapturedLogEntry);
            var capturedLogEntry = logger.CapturedLogEntry;
            capturedLogEntry.LogLevel.Should().Be(logLevel);
            capturedLogEntry.EventId.Should().Be(eventId);
            capturedLogEntry.Message.Should().Be(message);
            capturedLogEntry.State.Should().Be(state);
            capturedLogEntry.Scope.Should().BeEmpty();
            capturedLogEntry.Exception.Should().BeSameAs(exception);

            // Now that Message has been accessed, we should have the formatter captured values.
            formatterCapturedState.Should().Be(state);
            formatterCapturedException.Should().BeSameAs(exception);
        }

        [Fact]
        public void DoesNotCallWriteLogEntryWhenIsEnabledIsFalse()
        {
            var logger = new CapturingHighPerformanceLogger();

            LogLevel logLevel = LogLevel.Trace;
            EventId eventId = 123;
            decimal state = 987.654M;
            var exception = new InvalidOperationException("Oh, no!");
            string message = "Hello world!";

            decimal? formatterCapturedState = null;
            Exception? formatterCapturedException = null;

            var formatter = (decimal s, Exception? e) =>
            {
                formatterCapturedState = s;
                formatterCapturedException = e;
                return message;
            };

            logger.Log(logLevel, eventId, state, exception, formatter);

            logger.CapturedLogEntry.Should().BeNull();

            formatterCapturedState.Should().BeNull();
            formatterCapturedException.Should().BeNull();
        }

        [Fact]
        public void CapturesCurrentScopeWhenItExists()
        {
            var logger = new CapturingHighPerformanceLogger();

            LogLevel logLevel = LogLevel.Information;
            EventId eventId = 123;
            decimal state = 987.654M;
            var exception = new InvalidOperationException("Oh, no!");
            string message = "Hello world!";

            using (logger.BeginScope("abc"))
            {
                using (logger.BeginScope(123))
                {
                    logger.Log(logLevel, eventId, state, exception, (s, e) => message);
                }
            }

            Assert.NotNull(logger.CapturedLogEntry);

            var capturedLogEntry = logger.CapturedLogEntry;
            logger.CapturedLogEntry = null;

            capturedLogEntry.LogLevel.Should().Be(logLevel);
            capturedLogEntry.EventId.Should().Be(eventId);
            capturedLogEntry.Message.Should().Be(message);
            capturedLogEntry.State.Should().Be(state);
            capturedLogEntry.Scope.Should().HaveCount(2);
            capturedLogEntry.Scope.First().Should().Be(123);
            capturedLogEntry.Scope.Skip(1).First().Should().Be("abc");
            capturedLogEntry.Exception.Should().BeSameAs(exception);
        }

        [Fact]
        public void GracefullyHandlesNullStateValue()
        {
            var logger = new CapturingHighPerformanceLogger();

            LogLevel logLevel = LogLevel.Information;
            EventId eventId = 123;
            var exception = new InvalidOperationException("Oh, no!");
            string message = "Hello world!";

            object? formatterCapturedState = new();
            Exception? formatterCapturedException = null;

            var formatter = (object? s, Exception? e) =>
            {
                formatterCapturedState = s;
                formatterCapturedException = e;
                return message;
            };

            logger.Log(logLevel, eventId, null, exception, formatter);

            // The formatter should be called when the log entry's Message property is accessed, not at initialization.
            Assert.NotNull(formatterCapturedState);
            formatterCapturedException.Should().BeNull();

            Assert.NotNull(logger.CapturedLogEntry);
            var capturedLogEntry = logger.CapturedLogEntry;
            capturedLogEntry.LogLevel.Should().Be(logLevel);
            capturedLogEntry.EventId.Should().Be(eventId);
            capturedLogEntry.Message.Should().Be(message);
            capturedLogEntry.State.Should().BeNull();
            capturedLogEntry.Scope.Should().BeEmpty();
            capturedLogEntry.Exception.Should().BeSameAs(exception);

            // Now that Message has been accessed, we should have the formatter captured values.
            formatterCapturedState.Should().BeNull();
            formatterCapturedException.Should().BeSameAs(exception);
        }

        private class CapturingHighPerformanceLogger : HighPerformanceLogger, ILogger
        {
            public ILogEntry? CapturedLogEntry { get; set; }

            public override void Write<TState>(in LogEntry<TState> logEntry)
            {
                if (CapturedLogEntry is not null)
                    throw new InvalidOperationException("CapturingHighPerformanceLogger must not be used to log more than once.");

                CapturedLogEntry = logEntry;
            }
        }
    }
}

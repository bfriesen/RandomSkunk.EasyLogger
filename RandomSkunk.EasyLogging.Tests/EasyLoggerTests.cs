using FluentAssertions;
using Microsoft.Extensions.Logging;

namespace RandomSkunk.EasyLogging.Tests;

public class EasyLoggerTests
{
    public class Identity
    {
        [Fact]
        public void EasyLoggerImplementsILoggerInterface()
        {
            EasyLogger logger = new();

            logger.Should().BeAssignableTo<ILogger>();
        }

        [Fact]
        public void EasyLoggerOfTCategoryNameInheritsFromEasyLogger()
        {
            EasyLogger<Identity> logger = new();

            logger.Should().BeAssignableTo<EasyLogger>();
        }

        [Fact]
        public void EasyLoggerOfTCategoryNameImplementsILoggerOfTCategoryName()
        {
            EasyLogger<Identity> logger = new();

            logger.Should().BeAssignableTo<ILogger<Identity>>();
        }
    }

    public class LogLevelProperty
    {
        [Fact]
        public void DefaultValueIsInformation()
        {
            var logger = new EasyLogger();

            logger.LogLevel.Should().Be(LogLevel.Information);
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(7)]
        public void GivenInvalidValueThrowsException(int logLevel)
        {
            var act = () => new EasyLogger { LogLevel = (LogLevel)logLevel };

            act.Should().Throw<ArgumentOutOfRangeException>();
        }
    }

    public class IncludeScopesProperty
    {
        [Fact]
        public void DefaultValueIsTrue()
        {
            var logger = new EasyLogger();

            logger.IncludeScopes.Should().BeTrue();
        }
    }

    public class CurrentScopeProperty
    {
        [Fact]
        public void DefaultValueIsNull()
        {
            var logger = new EasyLogger();

            logger.CurrentScope.Should().BeEmpty();
        }
    }

    public class IsEnabledMethod
    {
        [Theory]
        [InlineData(LogLevel.Trace, (LogLevel)(-1), false)]
        [InlineData(LogLevel.Trace, LogLevel.Trace, true)]
        [InlineData(LogLevel.Trace, LogLevel.Debug, true)]
        [InlineData(LogLevel.Trace, LogLevel.Information, true)]
        [InlineData(LogLevel.Trace, LogLevel.Warning, true)]
        [InlineData(LogLevel.Trace, LogLevel.Error, true)]
        [InlineData(LogLevel.Trace, LogLevel.Critical, true)]
        [InlineData(LogLevel.Trace, LogLevel.None, false)]
        [InlineData(LogLevel.Trace, (LogLevel)7, false)]
        [InlineData(LogLevel.Debug, (LogLevel)(-1), false)]
        [InlineData(LogLevel.Debug, LogLevel.Trace, false)]
        [InlineData(LogLevel.Debug, LogLevel.Debug, true)]
        [InlineData(LogLevel.Debug, LogLevel.Information, true)]
        [InlineData(LogLevel.Debug, LogLevel.Warning, true)]
        [InlineData(LogLevel.Debug, LogLevel.Error, true)]
        [InlineData(LogLevel.Debug, LogLevel.Critical, true)]
        [InlineData(LogLevel.Debug, LogLevel.None, false)]
        [InlineData(LogLevel.Debug, (LogLevel)7, false)]
        [InlineData(LogLevel.Information, (LogLevel)(-1), false)]
        [InlineData(LogLevel.Information, LogLevel.Trace, false)]
        [InlineData(LogLevel.Information, LogLevel.Debug, false)]
        [InlineData(LogLevel.Information, LogLevel.Information, true)]
        [InlineData(LogLevel.Information, LogLevel.Warning, true)]
        [InlineData(LogLevel.Information, LogLevel.Error, true)]
        [InlineData(LogLevel.Information, LogLevel.Critical, true)]
        [InlineData(LogLevel.Information, LogLevel.None, false)]
        [InlineData(LogLevel.Information, (LogLevel)7, false)]
        [InlineData(LogLevel.Warning, (LogLevel)(-1), false)]
        [InlineData(LogLevel.Warning, LogLevel.Trace, false)]
        [InlineData(LogLevel.Warning, LogLevel.Debug, false)]
        [InlineData(LogLevel.Warning, LogLevel.Information, false)]
        [InlineData(LogLevel.Warning, LogLevel.Warning, true)]
        [InlineData(LogLevel.Warning, LogLevel.Error, true)]
        [InlineData(LogLevel.Warning, LogLevel.Critical, true)]
        [InlineData(LogLevel.Warning, LogLevel.None, false)]
        [InlineData(LogLevel.Warning, (LogLevel)7, false)]
        [InlineData(LogLevel.Error, (LogLevel)(-1), false)]
        [InlineData(LogLevel.Error, LogLevel.Trace, false)]
        [InlineData(LogLevel.Error, LogLevel.Debug, false)]
        [InlineData(LogLevel.Error, LogLevel.Information, false)]
        [InlineData(LogLevel.Error, LogLevel.Warning, false)]
        [InlineData(LogLevel.Error, LogLevel.Error, true)]
        [InlineData(LogLevel.Error, LogLevel.Critical, true)]
        [InlineData(LogLevel.Error, LogLevel.None, false)]
        [InlineData(LogLevel.Error, (LogLevel)7, false)]
        [InlineData(LogLevel.Critical, (LogLevel)(-1), false)]
        [InlineData(LogLevel.Critical, LogLevel.Trace, false)]
        [InlineData(LogLevel.Critical, LogLevel.Debug, false)]
        [InlineData(LogLevel.Critical, LogLevel.Information, false)]
        [InlineData(LogLevel.Critical, LogLevel.Warning, false)]
        [InlineData(LogLevel.Critical, LogLevel.Error, false)]
        [InlineData(LogLevel.Critical, LogLevel.Critical, true)]
        [InlineData(LogLevel.Critical, LogLevel.None, false)]
        [InlineData(LogLevel.Critical, (LogLevel)7, false)]
        [InlineData(LogLevel.None, (LogLevel)(-1), false)]
        [InlineData(LogLevel.None, LogLevel.Trace, false)]
        [InlineData(LogLevel.None, LogLevel.Debug, false)]
        [InlineData(LogLevel.None, LogLevel.Information, false)]
        [InlineData(LogLevel.None, LogLevel.Warning, false)]
        [InlineData(LogLevel.None, LogLevel.Error, false)]
        [InlineData(LogLevel.None, LogLevel.Critical, false)]
        [InlineData(LogLevel.None, LogLevel.None, false)]
        [InlineData(LogLevel.None, (LogLevel)7, false)]
        public void ReturnsTheCorrectValue(LogLevel loggerLogLevel, LogLevel logEventLogLevel, bool expectedIsEnabled)
        {
            var logger = new EasyLogger { LogLevel = loggerLogLevel };

            var actualIsEnabled = logger.IsEnabled(logEventLogLevel);

            actualIsEnabled.Should().Be(expectedIsEnabled);
        }
    }

    public class BeginScopeMethod
    {
        [Fact]
        public void GivenNullStateObjectThrowsException()
        {
            var logger = new EasyLogger();

            logger.Invoking(x => x.BeginScope<object>(null!))
                .Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void GivenIncludeScopesIsFalseReturnsNull()
        {
            var logger = new EasyLogger { IncludeScopes = false };

            var scope = logger.BeginScope(123);

            scope.Should().BeNull();
            logger.CurrentScope.Should().BeEmpty();
        }

        [Fact]
        public void ReturnsScopeObjectAndSetsItToCurrentScope()
        {
            var logger = new EasyLogger();

            // Begin the first, outer scope.
            var scope1 = logger.BeginScope(123);

            scope1.Should().NotBeNull();
            logger.CurrentScope.Should().HaveCount(1);
            logger.CurrentScope.First().Should().Be(123);

            // Begin the second, inner scope.
            var scope2 = logger.BeginScope(456);

            scope2.Should().NotBeNull();
            logger.CurrentScope.Should().HaveCount(2);
            logger.CurrentScope.First().Should().Be(456);
            logger.CurrentScope.Skip(1).First().Should().Be(123);

            // Dispose the inner scope.
            scope2!.Dispose();

            logger.CurrentScope.Should().HaveCount(1);
            logger.CurrentScope.First().Should().Be(123);

            // Dispose the outer scope.
            scope1!.Dispose();

            logger.CurrentScope.Should().BeEmpty();
        }

        [Fact]
        public void HandlesOutOfOrderDisposalOfScopeObjects()
        {
            var logger = new EasyLogger();

            // Begin the first, outer scope.
            var scope1 = logger.BeginScope(123);
            
            // Begin the second, inner scope.
            var scope2 = logger.BeginScope(456);

            // Since this is the outer scope, disposing it should leave the logger without a scope.
            scope1!.Dispose();

            logger.CurrentScope.Should().BeEmpty();

            // This should do nothing since the logger no longer has a scope.
            scope2!.Dispose();

            logger.CurrentScope.Should().BeEmpty();
        }
    }

    public class LogMethod
    {
        [Fact]
        public void CallsWriteLogEntryWhenIsEnabledIsTrue()
        {
            var logger = new TestingLogger();

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

            // The formatter should be called when the log entry's GetMessage function is called, not at initialization.
            formatterCapturedState.Should().BeNull();
            formatterCapturedException.Should().BeNull();

            logger.CapturedLogEntry.Should().NotBeNull();
            var capturedLogEntry = logger.CapturedLogEntry!.Value;
            capturedLogEntry.LogLevel.Should().Be(logLevel);
            capturedLogEntry.EventId.Should().Be(eventId);
            capturedLogEntry.GetMessage().Should().Be(message);
            capturedLogEntry.State.Should().Be(state);
            capturedLogEntry.Scope.Should().BeEmpty();
            capturedLogEntry.Exception.Should().BeSameAs(exception);

            // Now that GetMessage has been invoked, we should have the formatter captured values.
            formatterCapturedState.Should().Be(state);
            formatterCapturedException.Should().BeSameAs(exception);
        }

        [Fact]
        public void DoesNotCallWriteLogEntryWhenIsEnabledIsFalse()
        {
            var logger = new TestingLogger(enabled: false);

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

            logger.CapturedLogEntry.Should().BeNull();

            formatterCapturedState.Should().BeNull();
            formatterCapturedException.Should().BeNull();
        }

        [Fact]
        public void CapturesCurrentScopeWhenItExists()
        {
            var logger = new TestingLogger();

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

            logger.CapturedLogEntry.Should().NotBeNull();

            var capturedLogEntry = logger.CapturedLogEntry!.Value;
            logger.CapturedLogEntry = null;

            capturedLogEntry.LogLevel.Should().Be(logLevel);
            capturedLogEntry.EventId.Should().Be(eventId);
            capturedLogEntry.GetMessage().Should().Be(message);
            capturedLogEntry.State.Should().Be(state);
            capturedLogEntry.Scope.Should().HaveCount(2);
            capturedLogEntry.Scope.First().Should().Be(123);
            capturedLogEntry.Scope.Skip(1).First().Should().Be("abc");
            capturedLogEntry.Exception.Should().BeSameAs(exception);
        }

        [Fact]
        public void GracefullyHandlesNullStateValue()
        {
            var logger = new TestingLogger();

            LogLevel logLevel = LogLevel.Information;
            EventId eventId = 123;
            var exception = new InvalidOperationException("Oh, no!");
            string message = "Hello world!";

            object? formatterCapturedState = new object();
            Exception? formatterCapturedException = null;

            var formatter = (object? s, Exception? e) =>
            {
                formatterCapturedState = s;
                formatterCapturedException = e;
                return message;
            };

            logger.Log(logLevel, eventId, null, exception, formatter);

            // The formatter should be called when the log entry's GetMessage function is called, not at initialization.
            formatterCapturedState.Should().NotBeNull();
            formatterCapturedException.Should().BeNull();

            logger.CapturedLogEntry.Should().NotBeNull();
            var capturedLogEntry = logger.CapturedLogEntry!.Value;
            capturedLogEntry.LogLevel.Should().Be(logLevel);
            capturedLogEntry.EventId.Should().Be(eventId);
            capturedLogEntry.GetMessage().Should().Be(message);
            capturedLogEntry.State.Should().BeNull();
            capturedLogEntry.Scope.Should().BeEmpty();
            capturedLogEntry.Exception.Should().BeSameAs(exception);

            // Now that GetMessage has been invoked, we should have the formatter captured values.
            formatterCapturedState.Should().BeNull();
            formatterCapturedException.Should().BeSameAs(exception);
        }

        private class TestingLogger(bool enabled = true) : EasyLogger, ILogger
        {
            public LogEntry? CapturedLogEntry { get; set; }

            public override void WriteLogEntry(LogEntry logEntry)
            {
                if (CapturedLogEntry is not null)
                    throw new InvalidOperationException("TestingLogger must not be used to log more than once.");

                CapturedLogEntry = logEntry;
            }

            bool ILogger.IsEnabled(LogLevel logLevel) => enabled;
        }
    }
}

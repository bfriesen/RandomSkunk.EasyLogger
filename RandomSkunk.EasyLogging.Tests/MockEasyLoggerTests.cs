using FakeItEasy;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NSubstitute;

namespace RandomSkunk.EasyLogging.Tests;

public class MockEasyLoggerTests
{
    public class Setup
    {
        [Fact]
        public void WorksInMoq()
        {
            var mockLogger = new Mock<EasyLogger>();
            var logger = mockLogger.Object;

            LogEntry capturedLogEntry = default;

            mockLogger.Setup(m => m.Write(It.IsAny<LogEntry>()))
                .Callback<LogEntry>(logEntry => capturedLogEntry = logEntry);

            logger.LogInformation("Hello, {Who}!", "world");

            capturedLogEntry.LogLevel.Should().Be(LogLevel.Information);
            capturedLogEntry.GetMessage().Should().Be("Hello, world!");
            capturedLogEntry.Attributes.Should().ContainKey("Who").WhoseValue.Should().Be("world");
        }

        [Fact]
        public void WorksInFakeItEasy()
        {
            var logger = A.Fake<EasyLogger>();

            LogEntry capturedLogEntry = default;

            A.CallTo(() => logger.Write(A<LogEntry>.Ignored))
                .Invokes((LogEntry logEntry) => capturedLogEntry = logEntry);

            logger.LogInformation("Hello, {Who}!", "world");

            capturedLogEntry.LogLevel.Should().Be(LogLevel.Information);
            capturedLogEntry.GetMessage().Should().Be("Hello, world!");
            capturedLogEntry.Attributes.Should().ContainKey("Who").WhoseValue.Should().Be("world");
        }

        [Fact]
        public void WorksInNSubstitute()
        {
            var logger = Substitute.For<EasyLogger>();

            LogEntry capturedLogEntry = default;

            logger.When(m => m.Write(Arg.Any<LogEntry>()))
                .Do(x => capturedLogEntry = x.Arg<LogEntry>());

            logger.LogInformation("Hello, {Who}!", "world");

            capturedLogEntry.LogLevel.Should().Be(LogLevel.Information);
            capturedLogEntry.GetMessage().Should().Be("Hello, world!");
            capturedLogEntry.Attributes.Should().ContainKey("Who").WhoseValue.Should().Be("world");
        }
    }

    public class Verification
    {
        [Fact]
        public void WorksInMoq()
        {
            var mockLogger = new Mock<EasyLogger>();
            var logger = mockLogger.Object;

            logger.LogInformation("Hello, {Who}!", "world");

            mockLogger.Verify(m => m.Write(It.Is<LogEntry>(log =>
                log.IsInformation() && log.HasMessage("Hello, world!") && log.HasAttribute("Who", "world"))));
        }

        [Fact]
        public void WorksInFakeItEasy()
        {
            var logger = A.Fake<EasyLogger>();

            logger.LogInformation("Hello, {Who}!", "world");

            A.CallTo(() => logger.Write(A<LogEntry>.That.Matches(log =>
                log.IsInformation() && log.HasMessage("Hello, world!") && log.HasAttribute("Who", "world"))))
                .MustHaveHappened();
        }

        [Fact]
        public void WorksInNSubstitute()
        {
            var logger = Substitute.For<EasyLogger>();

            logger.LogInformation("Hello, {Who}!", "world");

            logger.Received().Write(Arg.Is<LogEntry>(log =>
                log.IsInformation() && log.HasMessage("Hello, world!") && log.HasAttribute("Who", "world")));
        }
    }
}

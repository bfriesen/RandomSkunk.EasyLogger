using FakeItEasy;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NSubstitute;

namespace RandomSkunk.Logging.Tests;

public class MockEasyLoggerTests
{
    public class Setup
    {
        [Fact]
        public void WorksInMoq()
        {
            var mockLogger = new Mock<EasyLogger>();
            var logger = mockLogger.Object;

            ILogEntry? capturedLogEntry = default;

mockLogger.Setup(logger => logger.Write(It.Is<ILogEntry>(log =>
    log.IsInformation() && log.HasMessage("Test message"))));

            mockLogger.Setup(m => m.Write(It.IsAny<ILogEntry>()))
                .Callback<ILogEntry>(logEntry => capturedLogEntry = logEntry);

            logger.LogInformation("Hello, {Who}!", "world");

            capturedLogEntry!.LogLevel.Should().Be(LogLevel.Information);
            capturedLogEntry.Message.Should().Be("Hello, world!");
            capturedLogEntry.Attributes.Should().ContainKey("Who").WhoseValue.Should().Be("world");
        }

        [Fact]
        public void WorksInFakeItEasy()
        {
            var logger = A.Fake<EasyLogger>();

            ILogEntry? capturedLogEntry = default;

            A.CallTo(() => logger.Write(A<ILogEntry>.Ignored))
                .Invokes((ILogEntry logEntry) => capturedLogEntry = logEntry);

            logger.LogInformation("Hello, {Who}!", "world");

            capturedLogEntry!.LogLevel.Should().Be(LogLevel.Information);
            capturedLogEntry.Message.Should().Be("Hello, world!");
            capturedLogEntry.Attributes.Should().ContainKey("Who").WhoseValue.Should().Be("world");
        }

        [Fact]
        public void WorksInNSubstitute()
        {
            var logger = Substitute.For<EasyLogger>();

            ILogEntry? capturedLogEntry = default;

            logger.When(m => m.Write(Arg.Any<ILogEntry>()))
                .Do(x => capturedLogEntry = x.Arg<ILogEntry>());

            logger.LogInformation("Hello, {Who}!", "world");

            capturedLogEntry!.LogLevel.Should().Be(LogLevel.Information);
            capturedLogEntry.Message.Should().Be("Hello, world!");
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

            mockLogger.Verify(m => m.Write(It.Is<ILogEntry>(log =>
                log.IsInformation() && log.HasMessage("Hello, world!") && log.HasAttribute("Who", "world"))));
        }

        [Fact]
        public void WorksInFakeItEasy()
        {
            var logger = A.Fake<EasyLogger>();

            logger.LogInformation("Hello, {Who}!", "world");

            A.CallTo(() => logger.Write(A<ILogEntry>.That.Matches(log =>
                log.IsInformation() && log.HasMessage("Hello, world!") && log.HasAttribute("Who", "world"))))
                .MustHaveHappened();
        }

        [Fact]
        public void WorksInNSubstitute()
        {
            var logger = Substitute.For<EasyLogger>();

            logger.LogInformation("Hello, {Who}!", "world");

            logger.Received().Write(Arg.Is<ILogEntry>(log =>
                log.IsInformation() && log.HasMessage("Hello, world!") && log.HasAttribute("Who", "world")));
        }
    }
}

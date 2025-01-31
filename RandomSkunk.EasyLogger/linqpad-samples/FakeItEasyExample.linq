<Query Kind="Statements">
  <NuGetReference>FakeItEasy</NuGetReference>
  <NuGetReference>RandomSkunk.EasyLogger</NuGetReference>
  <Namespace>FakeItEasy</Namespace>
  <Namespace>Microsoft.Extensions.Logging</Namespace>
  <Namespace>RandomSkunk.Logging</Namespace>
</Query>

EasyLogger logger = A.Fake<EasyLogger>();

LogEntry? capturedLogEntry = null;

// Setup
A.CallTo(() => logger.Write(A<LogEntry>.Ignored))
    .Invokes((LogEntry logEntry) => capturedLogEntry = logEntry);

logger.LogInformation("Hello, {Who}!", "world");

// Verification
A.CallTo(() => logger.Write(A<LogEntry>.That.Matches(log =>
    log.IsInformation() && log.HasMessage("Hello, world!") && log.HasAttribute("Who", "world"))))
    .MustHaveHappened();

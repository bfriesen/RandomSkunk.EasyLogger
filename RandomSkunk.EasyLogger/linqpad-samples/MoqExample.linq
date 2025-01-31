<Query Kind="Statements">
  <NuGetReference>Moq</NuGetReference>
  <NuGetReference>RandomSkunk.EasyLogger</NuGetReference>
  <Namespace>Moq</Namespace>
  <Namespace>RandomSkunk.Logging</Namespace>
  <Namespace>Microsoft.Extensions.Logging</Namespace>
</Query>

Mock<EasyLogger> mockLogger = new Mock<EasyLogger>();
EasyLogger logger = mockLogger.Object;

LogEntry? capturedLogEntry = null;

// Setup
mockLogger.Setup(m => m.Write(It.IsAny<LogEntry>()))
	.Callback<LogEntry>(logEntry => capturedLogEntry = logEntry);

logger.LogInformation("Hello, {Who}!", "world");

// Verification
mockLogger.Verify(m => m.Write(It.Is<LogEntry>(log =>
	log.IsInformation() && log.HasMessage("Hello, world!") && log.HasAttribute("Who", "world"))));

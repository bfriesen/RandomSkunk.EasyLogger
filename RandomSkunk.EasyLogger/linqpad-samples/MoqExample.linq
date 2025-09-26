<Query Kind="Statements">
  <NuGetReference>Moq</NuGetReference>
  <NuGetReference>RandomSkunk.EasyLogger</NuGetReference>
  <Namespace>Moq</Namespace>
  <Namespace>RandomSkunk.Logging</Namespace>
  <Namespace>Microsoft.Extensions.Logging</Namespace>
</Query>

Mock<EasyLogger> mockLogger = new Mock<EasyLogger>();
EasyLogger logger = mockLogger.Object;

ILogEntry? capturedLogEntry = null;

// Setup
mockLogger.Setup(m => m.Write(It.IsAny<ILogEntry>()))
	.Callback<ILogEntry>(logEntry => capturedLogEntry = logEntry);

logger.LogInformation("Hello, {Who}!", "world");

// Verification
mockLogger.Verify(m => m.Write(It.Is<ILogEntry>(log =>
	log.IsInformation() && log.HasMessage("Hello, world!") && log.HasAttribute("Who", "world"))));

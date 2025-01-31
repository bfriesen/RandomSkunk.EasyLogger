<Query Kind="Statements">
  <NuGetReference>NSubstitute</NuGetReference>
  <NuGetReference>RandomSkunk.EasyLogger</NuGetReference>
  <Namespace>Microsoft.Extensions.Logging</Namespace>
  <Namespace>NSubstitute</Namespace>
  <Namespace>RandomSkunk.Logging</Namespace>
</Query>

EasyLogger logger = Substitute.For<EasyLogger>();

LogEntry? capturedLogEntry = null;

// Setup
logger.When(m => m.Write(Arg.Any<LogEntry>()))
	.Do(x => capturedLogEntry = x.Arg<LogEntry>());

logger.LogInformation("Hello, {Who}!", "world");

// Verification
logger.Received().Write(Arg.Is<LogEntry>(log =>
	log.IsInformation() && log.HasMessage("Hello, world!") && log.HasAttribute("Who", "world")));

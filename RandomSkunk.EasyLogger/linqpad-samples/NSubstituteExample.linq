<Query Kind="Statements">
  <NuGetReference>NSubstitute</NuGetReference>
  <NuGetReference>RandomSkunk.EasyLogger</NuGetReference>
  <Namespace>Microsoft.Extensions.Logging</Namespace>
  <Namespace>NSubstitute</Namespace>
  <Namespace>RandomSkunk.Logging</Namespace>
</Query>

EasyLogger logger = Substitute.For<EasyLogger>();

ILogEntry? capturedLogEntry = null;

// Setup
logger.When(m => m.Write(Arg.Any<ILogEntry>()))
	.Do(x => capturedLogEntry = x.Arg<ILogEntry>());

logger.LogInformation("Hello, {Who}!", "world");

// Verification
logger.Received().Write(Arg.Is<ILogEntry>(log =>
	log.IsInformation() && log.HasMessage("Hello, world!") && log.HasAttribute("Who", "world")));

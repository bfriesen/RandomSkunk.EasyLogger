# RandomSkunk.EasyLogging

This library provides a pair of simple logger implementations, `EasyLogger` and
`EasyLogger<TCategoryName>`, which implement `ILogger` and `ILogger<TCategoryName>` respectively.

Why "EasyLogger"?

- Correctly implementing the `ILogger.BeginScope` method is difficult.
  - `EasyLogger` takes care of it for you.
- Implementing `ILogger.IsEnabled` isn't terribly difficult, but if you're not careful, you could
  end up writing logs at the `None` log level.
  - `EasyLogger` implements this method and doesn't accidentally write `None` logs.
- Mocking the `ILogger.Log` in order to verify that a logging operation took place is difficult.
  - First, you need to deal with the generic type argument and the `state` parameter. It could be
    literally any type and any value. Where do you even begin?
  - What is the `formatter` parameter all about?
  - What about the logger's scope? What if you wanted to verify that the logger had a certain scope
    at the time of the log event? How would you mock that?
  - Mocking the `EasyLogger.WriteLogEntry` method to verify that a logging operation took place is
    easy.
- The ease of mocking is tested and verified with Moq, FakeItEasy, and NSubstitute.

## Mock Logger Setup and Verification

`EasyLogger` is easy to mock regardless of the mocking library - just specify that its abstract
`WriteLogEntry` method should be the target for setup or verification.

Note that the `LogEntry` struct contains numerous methods for querying whether it is a match for a
setup or verification. Methods used below in the examples are `IsInformation`, `HasMessage`, and
`HasAttribute`.

### Moq

```csharp
Mock<EasyLogger> mockLogger = new Mock<EasyLogger>();
EasyLogger logger = mockLogger.Object;

LogEntry? capturedLogEntry = null;

// Setup
mockLogger.Setup(m => m.WriteLogEntry(It.IsAny<LogEntry>()))
    .Callback<LogEntry>(logEntry => capturedLogEntry = logEntry);

logger.LogInformation("Hello, {Who}!", "world");

// Verification
mockLogger.Verify(m => m.WriteLogEntry(It.Is<LogEntry>(log =>
    log.IsInformation() && log.HasMessage("Hello, world!") && log.HasAttribute("Who", "world"))));
```

### FakeItEasy

```csharp
EasyLogger logger = A.Fake<EasyLogger>();

LogEntry? capturedLogEntry = null;

// Setup
A.CallTo(() => logger.WriteLogEntry(A<LogEntry>.Ignored))
    .Invokes((LogEntry logEntry) => capturedLogEntry = logEntry);

logger.LogInformation("Hello, {Who}!", "world");

// Verification
A.CallTo(() => logger.WriteLogEntry(A<LogEntry>.That.Matches(log =>
    log.IsInformation() && log.HasMessage("Hello, world!") && log.HasAttribute("Who", "world"))))
    .MustHaveHappened();
```

### NSubstitute

```csharp
EasyLogger logger = Substitute.For<EasyLogger>();

LogEntry? capturedLogEntry = null;

// Setup
logger.When(m => m.WriteLogEntry(Arg.Any<LogEntry>()))
    .Do(x => capturedLogEntry = x.Arg<LogEntry>());

logger.LogInformation("Hello, {Who}!", "world");

// Verification
logger.Received().WriteLogEntry(Arg.Is<LogEntry>(log =>
    log.IsInformation() && log.HasMessage("Hello, world!") && log.HasAttribute("Who", "world")));
```

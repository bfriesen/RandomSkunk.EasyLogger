namespace RandomSkunk.Logging.Tests;

internal class ConcreteLoggerBase : LoggerBase
{
    public ConcreteLoggerBase()
    {        
    }

    public ConcreteLoggerBase(string category)
        : base(category)
    {
    }
}

internal class ConcreteEasyLogger : EasyLogger
{
    public override void Write(ILogEntry logEntry)
    {
    }
}

internal class ConcreteEasyLogger<TCategoryName> : EasyLogger<TCategoryName>
{
    public override void Write(ILogEntry logEntry)
    {
    }
}

internal class ConcreteHighPerformanceLogger : HighPerformanceLogger
{
    public override void Write<TState>(in LogEntry<TState> logEntry)
    {
    }
}

internal class ConcreteHighPerformanceLogger<TCategoryName> : HighPerformanceLogger<TCategoryName>
{
    public override void Write<TState>(in LogEntry<TState> logEntry)
    {
    }
}

internal class DictionaryWithOverriddenToStringMethod(string toStringValue) : Dictionary<string, object>
{
    public override string ToString() => toStringValue;
}

internal class ListWithOverriddenToStringMethod(string toStringValue) : List<object>
{
    public override string ToString() => toStringValue;
}

internal class ClassWithOverriddenToStringMethod(string toStringValue)
{
    public override string ToString() => toStringValue;
}

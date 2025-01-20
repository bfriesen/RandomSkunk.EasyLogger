
namespace RandomSkunk.EasyLogging.Tests;

internal class ConcreteEasyLogger : RandomSkunk.EasyLogging.EasyLogger
{
    public override void Write(LogEntry logEntry)
    {
    }
}

internal class ConcreteEasyLogger<TCategoryName> : RandomSkunk.EasyLogging.EasyLogger<TCategoryName>
{
    public override void Write(LogEntry logEntry)
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

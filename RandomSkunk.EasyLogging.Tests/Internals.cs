namespace RandomSkunk.EasyLogging.Tests;

internal class DictionaryWithOverriddenToStringMethod(string toStringValue) : Dictionary<string, object>
{
    public override string ToString() => toStringValue;
}

internal class ListWithOverriddenToStringMethod(string toStringValue) : List<object>
{
    public override string ToString() => toStringValue;
}

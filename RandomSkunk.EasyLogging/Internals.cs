using System.Collections;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

[assembly: InternalsVisibleTo("RandomSkunk.EasyLogging.Tests")]

namespace RandomSkunk.EasyLogging;

internal interface ILoggerScope
{
    object State { get; }

    ILoggerScope? ParentScope { get; }
}

internal static class StringBuilderPool
{
    private static readonly ConcurrentBag<StringBuilder> _pool = [];

    public static StringBuilder Get() =>
        Debugger.IsAttached || !_pool.TryTake(out var sb) ? new StringBuilder() : sb;

    public static string ReturnToPool(this StringBuilder sb)
    {
        var str = sb.ToString();
        if (!Debugger.IsAttached && sb.Capacity <= 256)
        {
            sb.Length = 0;
            _pool.Add(sb);
        }
        return str;
    }
}

internal static class StringBuilderExtensions
{
    public static StringBuilder AppendState(
        this StringBuilder sb,
        object state,
        string? skipStateStringIfEqualTo = null)
    {
        var needsPadding = false;
        var hasOverriddenToStringMethod = state.GetType().HasOverriddenToStringMethod();
        if (hasOverriddenToStringMethod)
        {
            var stateString = state.ToString();
            if (stateString != skipStateStringIfEqualTo)
            {
                sb.Append(stateString);
                needsPadding = true;
            }
        }

        if (state is IEnumerable<KeyValuePair<string, object>> stateDictionary)
        {
            if (needsPadding)
                sb.Append(' ');

            sb.Append("{ ");
            var first = true;
            foreach (var item in stateDictionary)
            {
                if (first) first = false;
                else sb.Append(", ");

                sb.Append('[').Append(item.Key).Append("] = ").Append(item.Value);
            }

            sb.Append(" }");
        }
        else if (state is IEnumerable stateCollection and not string)
        {
            if (needsPadding)
                sb.Append(' ');

            sb.Append("[ ");

            var first = true;
            foreach (var item in stateCollection)
            {
                if (first) first = false;
                else sb.Append(", ");

                sb.Append(item);
            }

            sb.Append(" ]");
        }
        else if (!hasOverriddenToStringMethod)
        {
            sb.Append(state);
        }

        return sb;
    }
}

internal static class TypeExtensions
{
    private const BindingFlags _publicInstance = BindingFlags.Public | BindingFlags.Instance;

    private static readonly Type _objectType = typeof(object);
    private static readonly ConcurrentDictionary<Type, bool> _hasOverriddenToStringMethodLookup = [];

    public static bool HasOverriddenToStringMethod(this Type type) =>
        _hasOverriddenToStringMethodLookup.GetOrAdd(
            type,
            t =>
            {
                var toStringMethod = t.GetMethod(nameof(ToString), _publicInstance, Type.EmptyTypes)!;
                return toStringMethod.DeclaringType != _objectType;
            });
}

internal static class TupleExtensions
{
    public static IEnumerable<KeyValuePair<string, object>> ToKeyValuePairs(
        this IEnumerable<(string Key, object Value)> attributes) =>
        attributes
            .Where(x => x.Key is not null)
            .Select(x => new KeyValuePair<string, object>(x.Key, x.Value));
}

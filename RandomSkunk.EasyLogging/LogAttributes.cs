using System.Collections;
using System.Diagnostics;
using System.Text;

namespace RandomSkunk.EasyLogging;

/// <summary>
/// Represents a collection of key/value pairs derived from the state and scope of a log entry.
/// </summary>
[DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
public readonly struct LogAttributes
    : IEnumerable<KeyValuePair<string, object>>
{
    internal LogAttributes(object? state, ILoggerScope? scope)
    {
        State = state;
        Scope = scope;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="LogAttributes"/> struct.
    /// </summary>
    /// <param name="state">The entry to be written. Can be also an object.</param>
    /// <param name="scope">A collection of objects that represent a logger's current scope at the time of a log event. The first
    ///     object in the collection represents the logger's current scope, the second object represents its parent scope, the
    ///     third represents its grandparent scope, and so on.</param>
    /// <exception cref="ArgumentNullException">If any of the elements in <paramref name="scope"/> are <see langword="null"/>.
    ///     </exception>
    public LogAttributes(object? state, params object[] scope)
    {
        State = state;

        if (scope is null)
            return;

        for (int i = scope.Length - 1; i >= 0; i--)
        {
            if (scope[i] is null)
                throw new ArgumentException("Cannot have any null elements.", nameof(scope));

            Scope = new LoggerScope(scope[i], Scope);
        }
    }

    internal object? State { get; }

    internal ILoggerScope? Scope { get; }

    /// <inheritdoc/>
    public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
    {
        var state = State;

        if (state is IEnumerable<KeyValuePair<string, object>> stateAttributes)
        {
            if (state.GetType().HasOverriddenToStringMethod() && state.ToString() is string stateString)
                yield return new KeyValuePair<string, object>("State", stateString);

            foreach (var item in stateAttributes)
            {
                var key = item.Key;
                if (key is not null)
                    yield return new KeyValuePair<string, object>(key, item.Value ?? "");
            }
        }
        else if (state is IEnumerable stateItems and not string)
        {
            if (state.GetType().HasOverriddenToStringMethod() && state.ToString() is string stateString)
                yield return new KeyValuePair<string, object>("State", stateString);

            int index = 0;
            foreach (var item in stateItems)
                yield return new KeyValuePair<string, object>("State[" + index++ + "]", item ?? "");
        }
        else if (state is not null)
        {
            yield return new KeyValuePair<string, object>("State", state);
        }

        var scope = Scope;
        if (scope is null)
            yield break;

        var scopeKey = "Scope";

        while (true)
        {
            state = scope.State;
            if (state is IEnumerable<KeyValuePair<string, object>> scopeAttributes)
            {
                if (state.GetType().HasOverriddenToStringMethod() && state.ToString() is string stateString)
                    yield return new KeyValuePair<string, object>(scopeKey, stateString);

                foreach (var item in scopeAttributes)
                {
                    var key = item.Key;
                    if (key == "{OriginalFormat}")
                        yield return new KeyValuePair<string, object>("{" + scopeKey + ".OriginalFormat}", item.Value ?? "");
                    else if (key is not null)
                        yield return new KeyValuePair<string, object>(key, item.Value ?? "");
                }
            }
            else if (state is IEnumerable scopeItems and not string)
            {
                if (state.GetType().HasOverriddenToStringMethod() && state.ToString() is string stateString)
                    yield return new KeyValuePair<string, object>(scopeKey, stateString);

                int index = 0;
                foreach (var item in scopeItems)
                    yield return new KeyValuePair<string, object>(scopeKey + "[" + index++ + "]", item);
            }
            else
            {
                yield return new KeyValuePair<string, object>(scopeKey, state);
            }

            scope = scope.ParentScope;
            if (scope is null)
                break;

            scopeKey += ".ParentScope";
        }
    }

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    /// <inheritdoc/>
    public override string ToString()
    {
        var sb = StringBuilderPool.Get();
        Append(sb, State, Scope);
        return sb.ReturnToPool();
    }

    private string GetDebuggerDisplay()
    {
        var sb = new StringBuilder();
        Append(sb, State, Scope);
        return sb.ToString();
    }

    private static void Append(StringBuilder sb, object? state, ILoggerScope? loggerScope)
    {
        if (state is not null)
            sb.Append('{').AppendLine().Append(" State = ").AppendState(state);

        if (loggerScope is not null)
        {
            if (state is null)
                sb.Append('{');
            else
                sb.Append(',');

            sb.AppendLine().Append(' ');

            for (var scope = loggerScope; scope is not null; scope = scope.ParentScope)
            {
                if (ReferenceEquals(scope, loggerScope))
                    sb.Append("Scope = ").AppendState(scope.State);
                else
                    sb.Append(',').AppendLine().Append(" ParentScope = ").AppendState(scope.State);
            }
        }

        if (sb.Length == 0)
            sb.Append("{ }");
        else
            sb.Append(' ').AppendLine().Append('}');
    }

    private sealed record class LoggerScope(object State, ILoggerScope? ParentScope) : ILoggerScope;
}

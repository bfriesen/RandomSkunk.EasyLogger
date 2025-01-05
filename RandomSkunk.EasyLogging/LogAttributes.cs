using System.Collections;

namespace RandomSkunk.EasyLogging;

/// <summary>
/// Represents a collection of key/value pairs that describe the state and scope of a log entry.
/// </summary>
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
    /// <param name="scopes">A collection of objects that represent a logger's current scope at the
    /// time of a log event. The first object in the collection represents the logger's current
    /// scope, the second object represents its parent scope, the third represents its grandparent
    /// scope, and so on.</param>
    /// <exception cref="ArgumentNullException">If any of the elements in <paramref name="scopes"/>
    /// are <see langword="null"/>.</exception>
    public LogAttributes(object? state, params object[] scopes)
    {
        State = state;
        Scope = null;

        if (scopes is null)
            return;

        foreach (var scopeState in scopes.Reverse())
        {
            if (scopeState is null)
                throw new ArgumentNullException(nameof(scopes), "Cannot have any null elements.");

            Scope = new LoggerScope(scopeState, Scope);
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
                yield return new KeyValuePair<string, object>(item.Key, item.Value ?? "(null)");
        }
        else if (state is IEnumerable stateItems and not string)
        {
            if (state.GetType().HasOverriddenToStringMethod() && state.ToString() is string stateString)
                yield return new KeyValuePair<string, object>("State", stateString);

            int index = 0;
            foreach (var item in stateItems)
                yield return new KeyValuePair<string, object>("State[" + index++ + "]", item ?? "(null)");
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
            if (scope.State is IEnumerable<KeyValuePair<string, object>> scopeAttributes)
            {
                if (scope.State.GetType().HasOverriddenToStringMethod() && scope.State.ToString() is string stateString)
                    yield return new KeyValuePair<string, object>(scopeKey, stateString);

                foreach (var item in scopeAttributes)
                {
                    if (item.Key != "{OriginalFormat}")
                        yield return new KeyValuePair<string, object>(item.Key, item.Value ?? "(null)");
                    else
                        yield return new KeyValuePair<string, object>(scopeKey + ".OriginalFormat", item.Value ?? "(null)");
                }
            }
            else if (scope.State is IEnumerable scopeItems and not string)
            {
                if (scope.State.GetType().HasOverriddenToStringMethod() && scope.State.ToString() is string stateString)
                    yield return new KeyValuePair<string, object>(scopeKey, stateString);

                int index = 0;
                foreach (var item in scopeItems)
                    yield return new KeyValuePair<string, object>(scopeKey + "[" + index++ + "]", item);
            }
            else
            {
                yield return new KeyValuePair<string, object>(scopeKey, scope.State);
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

        if (State is not null)
            sb.Append("{\n State = ").AppendState(State);

        if (Scope is not null)
        {
            if (State is null)
                sb.Append("{\n ");
            else
                sb.Append(",\n ");

            for (var scope = Scope; scope is not null; scope = scope.ParentScope)
            {
                if (ReferenceEquals(scope, Scope))
                    sb.Append("Scope = ").AppendState(scope.State);
                else
                    sb.Append(",\n ParentScope = ").AppendState(scope.State);
            }
        }

        if (sb.Length == 0)
            sb.Append("{ }");
        else
            sb.Append("\n}");

        return sb.ReturnToPool();
    }

    private sealed record class LoggerScope(object State, ILoggerScope? ParentScope) : ILoggerScope;
}

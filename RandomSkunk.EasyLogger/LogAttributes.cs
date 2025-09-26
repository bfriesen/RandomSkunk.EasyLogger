using System.Collections;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace RandomSkunk.Logging;

/// <summary>
/// Represents a collection of key/value pairs derived from the state and scope of a log entry.
/// </summary>
[DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
public readonly struct LogAttributes<TState>
    : IEnumerable<KeyValuePair<string, object>>
{
    internal readonly TState State;
    internal readonly ILoggerScope? Scope;

    internal LogAttributes(in TState state, ILoggerScope? scope)
    {
        State = state;
        Scope = scope;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="LogAttributes{TState}"/> struct.
    /// </summary>
    /// <param name="state">The entry to be written. Can be also an object.</param>
    /// <param name="scope">A collection of objects that represent a logger's current scope at the time of a log event. The first
    ///     object in the collection represents the logger's current scope, the second object represents its parent scope, the
    ///     third represents its grandparent scope, and so on.</param>
    /// <exception cref="ArgumentNullException">If any of the elements in <paramref name="scope"/> are <see langword="null"/>.
    ///     </exception>
    public LogAttributes(in TState state, params object[] scope)
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

    /// <summary>
    /// Returns an enumerator that iterates through the collection.
    /// </summary>
    /// <returns>An enumerator that can be used to iterate through the collection.</returns>
    public Enumerator GetEnumerator() => new(this);

    /// <inheritdoc/>
    IEnumerator<KeyValuePair<string, object>> IEnumerable<KeyValuePair<string, object>>.GetEnumerator() => GetEnumerator();

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

    private static void Append(StringBuilder sb, TState state, ILoggerScope? loggerScope)
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

    /// <summary>
    /// Enumerates the key-value pairs representing the state and scope information of a log entry.
    /// </summary>
    public struct Enumerator : IEnumerator<KeyValuePair<string, object>>
    {
        private readonly LogAttributes<TState> _logAttributes;

        private EnumeratorState _enumeratorState;
        private KeyValuePair<string, object> _current;

        private IEnumerator<KeyValuePair<string, object>>? _stateAttributes;
        private int _stateItemsIndex;
        private IEnumerator? _stateItems;
        private ILoggerScope? _scope;
        private string? _scopeKey;

        internal Enumerator(LogAttributes<TState> logAttributes) => _logAttributes = logAttributes;

        /// <inheritdoc/>
        public readonly KeyValuePair<string, object> Current => _current;

        [ExcludeFromCodeCoverage]
        readonly object IEnumerator.Current => _current;

        /// <inheritdoc/>
        public bool MoveNext()
        {
            return _enumeratorState switch
            {
                EnumeratorState.Start => Start(),
                EnumeratorState.EnumerateStateAttributes => EnumerateStateAttributes(),
                EnumeratorState.EnumerateStateItems => EnumerateStateItems(),
                EnumeratorState.BeginScopes => BeginScopes(),
                EnumeratorState.ProcessScope => ProcessScope(),
                EnumeratorState.EnumerateScopeStateAttributes => EnumerateScopeStateAttributes(),
                EnumeratorState.EnumerateScopeStateItems => EnumerateScopeStateItems(),
                _ => throw new InvalidOperationException("Unknown enumerator state."),
            };
        }

        /// <inheritdoc/>
        public void Reset()
        {
            _enumeratorState = default;
            _current = default;
            _stateAttributes = default;
            _stateItemsIndex = default;
            _stateItems = default;
            _scope = default;
            _scopeKey = default;
        }

        /// <inheritdoc/>
        public void Dispose() => Reset();

        private bool Start()
        {
            if (_logAttributes.State is IEnumerable<KeyValuePair<string, object>> stateAttributes)
            {
                if (_logAttributes.State.GetType().HasOverriddenToStringMethod() && _logAttributes.State.ToString() is string stateString)
                {
                    _current = new KeyValuePair<string, object>("State", stateString);
                    _stateAttributes = stateAttributes.GetEnumerator();
                    _enumeratorState = EnumeratorState.EnumerateStateAttributes;
                    return true;
                }

                _stateAttributes = stateAttributes.GetEnumerator();
                _enumeratorState = EnumeratorState.EnumerateStateAttributes;
                return EnumerateStateAttributes();
            }
            else if (_logAttributes.State is IEnumerable stateItems and not string)
            {
                if (_logAttributes.State.GetType().HasOverriddenToStringMethod() && _logAttributes.State.ToString() is string stateString)
                {
                    _current = new KeyValuePair<string, object>("State", stateString);
                    _stateItems = stateItems.GetEnumerator();
                    _enumeratorState = EnumeratorState.EnumerateStateItems;
                    return true;
                }

                _stateItems = stateItems.GetEnumerator();
                _enumeratorState = EnumeratorState.EnumerateStateItems;
                return EnumerateStateItems();
            }
            else if (_logAttributes.State is not null)
            {
                object state;
                if (_logAttributes.State.GetType().HasOverriddenToStringMethod() && _logAttributes.State.ToString() is string stateString)
                    state = stateString;
                else
                    state = _logAttributes.State;

                _current = new KeyValuePair<string, object>("State", state);
                _enumeratorState = EnumeratorState.BeginScopes;
                return true;
            }

            _enumeratorState = EnumeratorState.BeginScopes;
            return BeginScopes();
        }

        private bool EnumerateStateAttributes()
        {
            if (_stateAttributes!.MoveNext())
            {
                _current = _stateAttributes.Current;
                return true;
            }

            _stateAttributes = null;
            _enumeratorState = EnumeratorState.BeginScopes;
            return BeginScopes();
        }

        private bool EnumerateStateItems()
        {
            if (_stateItems!.MoveNext())
            {
                _current = new KeyValuePair<string, object>("State[" + _stateItemsIndex++ + "]", _stateItems.Current ?? "");
                return true;
            }

            _stateItems = null;
            _stateItemsIndex = 0;
            _enumeratorState = EnumeratorState.BeginScopes;
            return BeginScopes();
        }

        private bool BeginScopes()
        {
            if (_logAttributes.Scope == null)
                return false;

            _scope = _logAttributes.Scope;
            _scopeKey = "Scope";

            _enumeratorState = EnumeratorState.ProcessScope;
            return ProcessScope();
        }

        private bool ProcessScope()
        {
            if (_scope == null)
                return false;

            if (_scope.State is IEnumerable<KeyValuePair<string, object>> stateAttributes)
            {
                if (_scope.State.GetType().HasOverriddenToStringMethod() && _scope.State.ToString() is string stateString)
                {
                    _current = new KeyValuePair<string, object>(_scopeKey!, stateString);
                    _stateAttributes = stateAttributes.GetEnumerator();
                    _enumeratorState = EnumeratorState.EnumerateScopeStateAttributes;
                    return true;
                }

                _stateAttributes = stateAttributes.GetEnumerator();

                _enumeratorState = EnumeratorState.EnumerateScopeStateAttributes;
                return EnumerateScopeStateAttributes();
            }
            else if (_scope.State is IEnumerable stateItems and not string)
            {
                if (_scope.State.GetType().HasOverriddenToStringMethod() && _scope.State.ToString() is string stateString)
                {
                    _current = new KeyValuePair<string, object>(_scopeKey!, stateString);
                    _stateItems = stateItems.GetEnumerator();
                    _enumeratorState = EnumeratorState.EnumerateScopeStateItems;
                    return true;
                }

                _stateItems = stateItems.GetEnumerator();

                _enumeratorState = EnumeratorState.EnumerateScopeStateItems;
                return EnumerateScopeStateItems();
            }
            else
            {
                object state;
                if (_scope.State.GetType().HasOverriddenToStringMethod() && _scope.State.ToString() is string stateString)
                    state = stateString;
                else
                    state = _scope.State;

                _current = new KeyValuePair<string, object>(_scopeKey!, state);

                _scope = _scope.ParentScope;
                _scopeKey += ".ParentScope";

                _enumeratorState = EnumeratorState.ProcessScope;
                return true;
            }
        }

        private bool EnumerateScopeStateAttributes()
        {
            if (_stateAttributes!.MoveNext())
            {
                _current = _stateAttributes.Current;
                return true;
            }

            _stateAttributes = null;
            _stateItemsIndex = 0;

            _scope = _scope!.ParentScope;
            _scopeKey += ".ParentScope";

            _enumeratorState = EnumeratorState.ProcessScope;
            return ProcessScope();
        }

        private bool EnumerateScopeStateItems()
        {
            if (_stateItems!.MoveNext())
            {
                _current = new KeyValuePair<string, object>(_scopeKey + "[" + _stateItemsIndex++ + "]", _stateItems.Current ?? "");
                return true;
            }

            _stateItems = null;
            _stateItemsIndex = 0;

            _scope = _scope!.ParentScope;
            _scopeKey += ".ParentScope";

            _enumeratorState = EnumeratorState.ProcessScope;
            return ProcessScope();
        }

        private enum EnumeratorState
        {
            Start = 0,
            EnumerateStateAttributes,
            EnumerateStateItems,
            BeginScopes,
            ProcessScope,
            EnumerateScopeStateAttributes,
            EnumerateScopeStateItems,
        }
    }

    private sealed record class LoggerScope(object State, ILoggerScope? ParentScope) : ILoggerScope;
}

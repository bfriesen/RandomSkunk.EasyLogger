using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace RandomSkunk.Logging;

/// <summary>
/// Represents a collection of logging scopes that can be enumerated.
/// </summary>
public readonly struct ScopeCollection : IEnumerable<object>
{
    private readonly ILoggerScope? _scope;

    internal ScopeCollection(ILoggerScope? scope) => _scope = scope;

    /// <inheritdoc/>
    public Enumerator GetEnumerator() => new(_scope);

    [ExcludeFromCodeCoverage]
    IEnumerator<object> IEnumerable<object>.GetEnumerator() => GetEnumerator();

    [ExcludeFromCodeCoverage]
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    /// <summary>
    /// Enumerates through a collection of logging scopes.
    /// </summary>
    public struct Enumerator : IEnumerator<object>
    {
        private readonly ILoggerScope? _scope;

        private ILoggerScope? _current;

        internal Enumerator(ILoggerScope? scope) => _scope = scope;

        /// <inheritdoc/>
        public readonly object Current => _current!.State;

        /// <inheritdoc/>
        public bool MoveNext()
        {
            if (_current == null)
                _current = _scope;
            else
                _current = _current.ParentScope;

            return _current != null;
        }

        /// <inheritdoc/>
        public void Reset() => _current = null;

        /// <inheritdoc/>
        public void Dispose() => Reset();
    }
}

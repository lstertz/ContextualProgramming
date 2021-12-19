namespace ContextualProgramming
{
    /// <summary>
    /// Encapsulates a value to be used within a context (<see cref="ContextAttribute"/>).
    /// </summary>
    /// <typeparam name="T">The type of value encapsulated.</typeparam>
    public class ContextState<T> : IEquatable<ContextState<T>>
    {
        public static implicit operator ContextState<T>(T? value) => new ContextState<T>(value);
        public static implicit operator T?(ContextState<T> contextValue) => contextValue._value;

        public static bool operator ==(ContextState<T>? a, ContextState<T>? b) => 
            Equals(a, null) ? Equals(b, null) : a.Equals(b);
        public static bool operator !=(ContextState<T>? a, ContextState<T>? b) =>
            Equals(a, null) ? !Equals(b, null) : !a.Equals(b);


        /// <summary>
        /// The encapsulated value of the context value.
        /// </summary>
        public T? Value { get => _value; set => _value = value; }
        private T? _value;


        /// <summary>
        /// Constructs a new context value with the specified value for it to encapsulate.
        /// </summary>
        /// <param name="value">The encapsulated value of the context value.</param>
        public ContextState(T? value)
        {
            _value = value;
        }

        /// <inheritdoc/>
        public override bool Equals(object? other)
        {
            return Equals(other as ContextState<T>);
        }

        /// <inheritdoc/>
        public bool Equals(ContextState<T>? other)
        {
            if (Equals(other, null))
                return false;

            if (_value == null)
                return other._value == null;

            return _value.Equals(other._value);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            if (_value == null)
                return 0;

            return _value.GetHashCode();
        }
    }
}

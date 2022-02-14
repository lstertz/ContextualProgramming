using ContextualProgramming.Internal;

namespace ContextualProgramming
{
    /// <summary>
    /// Encapsulates a value to be used within 
    /// a context (<see cref="BaseContextAttribute"/>).
    /// </summary>
    /// <typeparam name="T">The type of value encapsulated.
    /// This type should be a primitive-like type (int, string, etc.) and not 
    /// an object or struct with internal values.</typeparam>
    public class ReadonlyContextState<T> : State<T>
    {
        /// <summary>
        /// Implicitly converts a value to its equivalent readonly context state.
        /// </summary>
        /// <param name="value">The value to be converted.</param>
        public static implicit operator ReadonlyContextState<T>(T? value) => new(value);


        /// <summary>
        /// Constructs a new readonly context state with the specified value for it to encapsulate.
        /// </summary>
        /// <param name="value">The encapsulated value of the readonly context state.</param>
        public ReadonlyContextState(T? value) : base(value) { }


        /// <inheritdoc/>
        protected override State<T>? Convert(object? other) => other is T? ?
            new ReadonlyContextState<T>((T?)other) : null;
    }
}

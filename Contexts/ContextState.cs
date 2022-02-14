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
    public class ContextState<T> : State<T>, IBindableState
    {
        /// <summary>
        /// Implicitly converts a value to its equivalent context state.
        /// </summary>
        /// <param name="value">The value to be converted.</param>
        public static implicit operator ContextState<T>(T? value) => new(value);


        /// <summary>
        /// The encapsulated value of the context state.
        /// </summary>
        public new T? Value 
        {
            get => base.Value; 
            set
            {
                T? v = base.Value;
                if (v == null && value == null)
                    return;
                else if (v != null && v.Equals(value))
                    return;

                base.Value = value;
                _onChange?.Invoke();
            }
        }

        private Action? _onChange;


        /// <summary>
        /// Constructs a new context state with the specified value for it to encapsulate.
        /// </summary>
        /// <param name="value">The encapsulated value of the context state.</param>
        public ContextState(T? value) : base(value) { }


        /// <inheritdoc/>
        protected override State<T>? Convert(object? other) => other is T? ?
            new ContextState<T>((T?)other) : null;

        /// <inheritdoc/>
        void IBindableState.Bind(Action onChange) => _onChange = onChange ?? 
            throw new ArgumentNullException($"The binding action cannot be null. " +
                $"If attempting to unbind, use {nameof(IBindableState.Unbind)}.");

        /// <inheritdoc/>
        void IBindableState.Unbind() => _onChange = null;
    }
}

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
        public static implicit operator ContextState<T>(T value) => new(value);

        /// <summary>
        /// Implicitly converts a context state to its underlying value.
        /// </summary>
        /// <param name="state">The context state to be converted.</param>
        public static implicit operator T(ContextState<T> state) => state.InternalValue;


        /// <inheritdoc/>
        bool IBindableState.IsBound => _onChange != null;

        /// <summary>
        /// The encapsulated value of the context state.
        /// </summary>
        public T Value 
        {
            get => InternalValue; 
            set
            {
                if (InternalValue == null && value == null)
                    return;
                else if (InternalValue != null && InternalValue.Equals(value))
                    return;

                InternalValue = value;
                _onChange?.Invoke();
            }
        }

        private Action? _onChange;


        /// <summary>
        /// Constructs a new context state with the specified value for it to encapsulate.
        /// </summary>
        /// <param name="value">The encapsulated value of the context state.</param>
        public ContextState(T value) : base(value) { }


        /// <inheritdoc/>
        protected override State<T>? Convert(object? other)
        {
            if (other is ContextState<T> contextState)
                return new ContextState<T>(contextState.InternalValue);
            
            if (other is T value)
                return new ContextState<T>(value);

            return null;
        }


        /// <inheritdoc/>
        void IBindableState.Bind(Action onChange) => _onChange = onChange ?? 
            throw new ArgumentNullException(nameof(onChange), $"The binding action cannot " +
                $"be null. If attempting to unbind, use {nameof(IBindableState.Unbind)}.");

        /// <inheritdoc/>
        void IBindableState.Unbind() => _onChange = null;
    }
}

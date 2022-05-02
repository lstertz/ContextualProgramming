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
    public class ContextState<T> : State<T?>, IBindableState
    {
        /// <summary>
        /// Implicitly converts a value to its equivalent context state.
        /// </summary>
        /// <param name="value">The value to be converted.</param>
        public static implicit operator ContextState<T>(T? value) => new(value);

        /// <summary>
        /// Implicitly converts a context state to its underlying value.
        /// </summary>
        /// <param name="state">The context state to be converted.</param>
        public static implicit operator T?(ContextState<T> state) => state.InternalValue.Value;


        /// <summary>
        /// The encapsulated value of the context state.
        /// </summary>
        public T? Value
        {
            get => InternalValue.Value;
            set
            {
                if (InternalValue.Value == null && value == null)
                    return;
                else if (InternalValue.Value != null && InternalValue.Value.Equals(value))
                    return;

                InternalValue.Value = value;
                InternalValue.FlagAsChanged();
            }
        }


        /// <summary>
        /// Constructs a new context state with the specified value for it to encapsulate.
        /// </summary>
        /// <param name="value">The encapsulated value of the context state.</param>
        public ContextState(T? value) : base(value) { }


        /// <inheritdoc/>
        protected override State<T?>? Convert(object? other) => other is T? ?
            new ContextState<T>((T?)other) : null;


        /// <inheritdoc/>
        void IBindableState.Bind(Func<IBindableValue, IBindableValue> bindingCallback)
        {
            if (bindingCallback == null)
                throw new ArgumentNullException(nameof(bindingCallback), $"The binding callback " +
                    $"cannot be null. If attempting to unbind, " +
                    $"use {nameof(IBindableState.Unbind)}.");

            IBindableValue<T?>? boundValue = bindingCallback(InternalValue) as IBindableValue<T?>;
            if (boundValue == null)
                throw new InvalidOperationException("A bound value must have a value type that " +
                    "matches the state that it is being bound to.");

            InternalValue = boundValue;
        }

        /// <inheritdoc/>
        void IBindableState.Unbind() => InternalValue = new BindableValue<T?>(InternalValue.Value);
    }
}

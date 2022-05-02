namespace ContextualProgramming.Internal
{
    /// <summary>
    /// An encapsulation of a value that can be bound to an action 
    /// that should be invoked whenever the value changes.
    /// </summary>
    public interface IBindableState
    {
        /// <summary>
        /// Binds the state by registering it to the appropriate bindable value and providing 
        /// that bindable value back to the state as the state's intended internal value.
        /// </summary>
        /// <param name="bindingCallback">The function to be invoked with the state's 
        /// current bindable value, returning either the registered version of that 
        /// value or a different bindable value that should serve as the 
        /// state's internal value.</param>
        void Bind(Func<IBindableValue, IBindableValue> bindingCallback);

        /// <summary>
        /// Unbinds the state, which restores its internal bindable value to a 
        /// standalone unbound value.
        /// </summary>
        void Unbind();
    }
}

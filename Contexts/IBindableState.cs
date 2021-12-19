namespace ContextualProgramming.Internal
{
    /// <summary>
    /// An encapsulation of a value that can be bound to an action 
    /// that should be invoked whenever the value changes.
    /// </summary>
    public interface IBindableState
    {
        /// <summary>
        /// Binds the state to the provided action.
        /// </summary>
        /// <remarks>
        /// Binding is only expected to be performed once.
        /// </remarks>
        /// <param name="onChange">The action to be invoked whenever the value 
        /// of the state changes.</param>
        void Bind(Action onChange);
    }
}

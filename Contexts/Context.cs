namespace ContextualProgramming;

/// <summary>
/// The base of all contexts that define the execution context of the <see cref="App"/>.
/// </summary>
public abstract record Context
{
    /// <summary>
    /// Constructs a new context and registers it with the <see cref="App"/>.
    /// </summary>
    public Context()
    {
        App.Register(this);
    }
}
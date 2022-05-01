namespace ContextualProgramming.Internal;

/// <summary>
/// Represents a record of a change for a state of a context.
/// </summary>
public class ContextChange
{
    /// <summary>
    /// The context that was changed.
    /// </summary>
    public object Context { get; private set; }

    /// <summary>
    /// The name of the state that was changed.
    /// </summary>
    public string StateName { get; private set; }


    /// <summary>
    /// Constructs a new context change to hold details of a changed context.
    /// </summary>
    /// <param name="context"><see cref="Context"/></param>
    /// <param name="stateName"><see cref="StateName"/></param>
    public ContextChange(object context, string stateName) => (Context, StateName) =
        (context.EnsureNotNull(), stateName.EnsureNotNull());
}

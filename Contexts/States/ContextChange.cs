namespace ContextualProgramming.Internal;

/// <summary>
/// Represents a record of a change for a property of a context.
/// </summary>
public class ContextChange
{
    /// <summary>
    /// The context that was changed.
    /// </summary>
    public object Context { get; set; }

    /// <summary>
    /// The name of the property that was changed.
    /// </summary>
    public string PropertyName { get; set; }


    /// <summary>
    /// Constructs a new context change to hold details of a changed context.
    /// </summary>
    /// <param name="context"><see cref="Context"/></param>
    /// <param name="propertyName"><see cref="PropertyName"/></param>
    public ContextChange(object context, string propertyName) => (Context, PropertyName) =
        (context.EnsureNotNull(), propertyName.EnsureNotNull());
}

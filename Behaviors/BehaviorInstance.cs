namespace ContextualProgramming.Internal;

/// <summary>
/// Encapsulates an instance of a behavior with its contexts.
/// </summary>
public class BehaviorInstance
{
    /// <summary>
    /// The encapsulated behavior instance.
    /// </summary>
    public object Behavior { get; set; }

    /// <summary>
    /// The contexts (dependencies) of the encapsulated behavior keyed by their names.
    /// </summary>
    public Dictionary<string, object> Contexts { get; init; }

    /// <summary>
    /// The contexts (dependencies) of the encapsulated behavior mapped to their names.
    /// </summary>
    public Dictionary<object, string> ContextNames { get; init; }

    /// <summary>
    /// The contexts created by the behavior.
    /// </summary>
    public object[] SelfCreatedContexts { get; private set; }


    /// <summary>
    /// Constructs a new behavior instance to link an instance 
    /// of a behavior with its contexts.
    /// </summary>
    /// <param name="behavior"><see cref="Behavior"/></param>
    /// <param name="contexts"><see cref="Contexts"/></param>
    /// <param name="selfCreatedContexts"><see cref="SelfCreatedContexts"/></param>
    public BehaviorInstance(object behavior, Dictionary<string, object> contexts,
        object[] selfCreatedContexts) =>
        (Behavior, Contexts, ContextNames, SelfCreatedContexts) =
        (behavior.EnsureNotNull(), contexts, contexts.Flip(), selfCreatedContexts);
}

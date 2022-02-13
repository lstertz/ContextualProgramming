namespace ContextualProgramming.Concepts.Basics.Behaviors;

/// <summary>
/// Behaviors define the functionality of a Contextual Programming application.
/// 
/// They should have no state of their own, besides possible constants.
/// 
/// An instance of a behavior is instantiated by the app whenever its dependencies, 
/// as defined through the <see cref="DependencyAttribute{T}"/>, have been fulfilled.
/// Each dependency must be a context. The presence of a dependency when a behavior 
/// performs its functionality is how state is persisted and progressed for the application. 
/// 
/// Behaviors can have different types of dependency fulfillments:
/// - Existing = Dependencies that must exist prior to the behavior's instantiation.
/// - Self-Created = Dependencies that the behavior will fulfill itself when it's instantiated.
/// 
/// The dependencies can have different types of bindings, which specify how a dependency 
/// is bound to the behavior. These bindings essentially determine whether a behavior 
/// will use a context already bound to another behavior of the same type (Shared) or if it 
/// requires a context that is not bound to any other behavior of the same type (Unique). 
/// A context may be re-used as a dependency for different types of behaviors regardless of 
/// its use as a dependency for this type of behavior.
/// 
/// An instance of a behavior will continue to exist for as long as its dependencies 
/// are contextualized. If any of its dependencies are decontextualized, then the 
/// behavior instance is destroyed. Any of its dependencies that are still 
/// contextualized are re-purposed by the app for future instances of the behavior and 
/// are still able to be used by any other behavior instances that have them 
/// as dependencies.
///
/// Any class can be designated as a behavior through the <see cref="BehaviorAttribute"/>.
/// </summary>
[Behavior]
[Dependency<AppState>(Binding.Unique, Fulfillment.Existing, AppState)]
[Dependency<BasicContext>(Binding.Unique, Fulfillment.SelfCreated, BasicContext)]
public class BasicBehavior
{
    /// <summary>
    /// Behavior dependencies are identified by a name.
    /// This name must be the same anywhere the dependency is to be used, 
    /// whether it is as a parameter or in an attribute declaration.
    /// Using a constant for the name helps reduce errors for attribute declarations.
    /// </summary>
    private const string AppState = "appState";
    private const string BasicContext = "basicContext";


    /// <summary>
    /// The constructor for the behavior.
    /// This is only required if the behavior has a self-created dependency.
    /// All self-created dependencies must be created through the constructor of the behavior.
    /// </summary>
    /// <param name="basicContext">The self-created <see cref="Contexts.BasicContext"/>.</param>
    public BasicBehavior(out BasicContext basicContext)
    {
        basicContext = new();
    }


    /// <summary>
    /// This is an operation.
    /// An operation is where non-constructive functionality for a behavior is.
    /// 
    /// This specific operation will be called during the update cycle of the app 
    /// if the StateList state of the "basicContext" dependency has changed.
    /// 
    /// It is provided the "basicContext" dependency context and the "appState" 
    /// dependency context. It performs its functionality with/upon the provided 
    /// contexts, which results in further state changes to be handled as part 
    /// of the next update cycle of the app.
    /// </summary>
    /// <param name="basicContext">The basicContext dependency that has had 
    /// its state list changed.</param>
    /// <param name="appState">The appState dependency that will be 
    /// changed if the basiccontext has qualified for some condition.</param>
    [OnChange(BasicContext, nameof(Contexts.BasicContext.StateList))]
    private void SampleContextStateChangeOperation(BasicContext basicContext, AppState appState)
    {
        if (basicContext.ExampleQualifier())
            appState.ContinueRunning.Value = false;
    }


    // Behaviors can have static functions to perform standard functionality that 
    // their operations may use.

    // Note that behaviors and their operations are never referenced anywhere,
    // so their names can be as long and specific as needed for better
    // documentation since readability as API elements is not as relevant.
}

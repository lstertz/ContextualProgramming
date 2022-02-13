namespace ContextualProgramming.Concepts.Basics.Behaviors;

/// <summary>
/// Any behavior that only has self-created dependencies is referred to as 
/// an initialization behavior.
/// 
/// These behaviors are instantiated once when the app is initialized. They create 
/// their context(s) and persist for as long as their context is contextualized.
/// 
/// Initialization behaviors define the startup state of an application and will often trigger 
/// subsequent functionality through their OnInitialize operations.
/// </summary>
[Behavior]
[Dependency<AppState>(Binding.Unique, Fulfillment.SelfCreated, AppState)]
public class InitializationBehavior
{
    private const string AppState = "appState";


    /// <summary>
    /// The constructor for this initialization behavior.
    /// </summary>
    /// <param name="appState">The self-created <see cref="Contexts.AppState"/>.</param>
    public InitializationBehavior(out AppState appState)
    {
        appState = new();
    }


    // TODO :: OnInitialize operation.


    // Any type of behavior can have OnInitialize operations, not only initialization behaviors.

    // Initialization behaviors can also have any other type of operation as well, but are 
    // limited to only operating upon the contexts that they created.
}

using ContextualProgramming.Concepts.Contexts;

namespace ContextualProgramming.Concepts.Behaviors;

[Behavior]
[Dependency<AppState>(Binding.Unique, Fulfillment.SelfCreated, AppState)]
public class InitializationBehavior
{
    private const string AppState = "appState";


    /// <summary>
    /// Constructs a new test behavior A.
    /// </summary>
    /// <param name="appState">The self-created <see cref="Contexts.AppState"/> dependency.</param>
    public InitializationBehavior(out AppState appState)
    {
        appState = new();
    }
}

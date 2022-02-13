namespace ContextualProgramming.Concepts.Basics.Behaviors;

[Behavior]
[Dependency<AppState>(Binding.Unique, Fulfillment.SelfCreated, AppState)]
public class InitializationBehavior
{
    private const string AppState = "appState";


    public InitializationBehavior(out AppState appState)
    {
        appState = new();
    }
}

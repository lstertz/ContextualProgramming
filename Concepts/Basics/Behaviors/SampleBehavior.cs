using ContextualProgramming.Concepts.Contexts;

namespace ContextualProgramming.Concepts.Behaviors;

[Behavior]
[Dependency<AppState>(Binding.Unique, Fulfillment.Existing, AppState)]
[Dependency<SampleContext>(Binding.Unique, Fulfillment.SelfCreated, SampleContext)]
public class SampleBehavior
{
    private const string AppState = "appState";
    private const string SampleContext = "sampleContext";


    public SampleBehavior(out SampleContext sampleContext)
    {
        sampleContext = new();
    }
}

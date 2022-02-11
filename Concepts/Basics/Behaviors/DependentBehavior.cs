using ContextualProgramming.Concepts.Contexts;

namespace ContextualProgramming.Concepts.Behaviors;

[Behavior]
[Dependency<SampleContext>(Binding.Unique, Fulfillment.Existing, SampleContext)]
public class DependentBehavior
{
    private const string SampleContext = "sampleContext";


}

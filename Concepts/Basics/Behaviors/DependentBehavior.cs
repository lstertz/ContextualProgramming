using ContextualProgramming.Concepts.Basics.Contexts;

namespace ContextualProgramming.Concepts.Basics.Behaviors;

[Behavior]
[Dependency<BasicContext>(Binding.Unique, Fulfillment.Existing, BasicContext)]
public class DependentBehavior
{
    private const string BasicContext = "basicContext";

    [OnChange(BasicContext)]
    private void SampleContextOperation(BasicContext basicContext)
    {
        if (basicContext.State != 0 && basicContext.StateList.Elements?.Length == 0)
            basicContext.StateList.Add(basicContext.State);
    }
}

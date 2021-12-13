using Tests.Contexts;

namespace Tests.Behaviors;

[Behavior]
[Dependency<ContextA>(Binding.Unique, Fulfillment.SelfCreated, "contextA")]
public class BehaviorA
{
    /// <summary>
    /// Constructs a new test behavior A.
    /// </summary>
    /// <param name="contextA">The self-created <see cref="ContextA"/> dependency.</param>
    public BehaviorA(out ContextA contextA)
    {
        contextA = new ContextA();
    }
}

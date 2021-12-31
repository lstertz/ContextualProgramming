using Tests.Contexts;

namespace Tests.Behaviors;

[Behavior]
[Dependency<ContextA>(Binding.Unique, Fulfillment.SelfCreated, ContextName)]
public class BehaviorA
{
    private const string ContextName = "contextA";


    /// <summary>
    /// Constructs a new test behavior A.
    /// </summary>
    /// <param name="contextA">The self-created <see cref="ContextName"/> dependency.</param>
    public BehaviorA(out ContextA contextA)
    {
        contextA = new ContextA();

        Console.WriteLine("Behavior A has been created.");
    }

    /// <summary>
    /// Handles the destruction of test behavior A.
    /// </summary>
    ~BehaviorA()
    {
        Console.WriteLine("Behavior A has been destroyed.");
    }


    /// <summary>
    /// Logs the value of the Context A's state, whenever the state changes.
    /// </summary>
    [Operation]
    [OnChange(ContextName, nameof(ContextA.State))]
    public void OutputUpdatedState(ContextA contextA)
    {
        Console.WriteLine($"Updated state: {contextA.State.Value}");
    }
}

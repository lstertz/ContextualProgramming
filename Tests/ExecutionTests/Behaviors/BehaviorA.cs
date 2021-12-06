using Tests.Contexts;

namespace Tests.Behaviors;

[Behavior]
[Dependency<ContextA>(Binding.Unique, Fulfillment.SelfCreated, "contextA")]
public class BehaviorA
{

}

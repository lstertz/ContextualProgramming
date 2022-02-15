namespace ContextualProgramming.Concepts.Basics.Behaviors;

/// <summary>
/// Any type of behavior that required an existing context for its dependencies to be 
/// completely fulfilled is referred to as a dependent behavior, since it depends upon 
/// some other behavior to fulfill its dependencies.
/// </summary>
[Behavior]
[Dependency<BasicContext>(Binding.Unique, Fulfillment.Existing, BasicContext)]
public class DependentBehavior
{
    private const string BasicContext = "basicContext";


    /// <summary>
    /// This operation will be called during the update cycle of the app 
    /// if any of the states of the "basicContext" dependency have changed.
    /// 
    /// It is provided the "basicContext" dependency context as its only input, 
    /// so any functionality must be referencing and ultimately manipulating the 
    /// state(s) of that context to be of any purpose.
    /// 
    /// Since this operation is not specific about the changed state, it will be 
    /// invoked on the update cycle after it manipulates the context's state, even though 
    /// it was the operation that changed the state. This kind of self-changing reactive 
    /// loop can be useful, but should be handled carefully to keep unintended 
    /// operations from occurring often.
    /// </summary>
    /// <param name="basicContext">The basicContext dependency that has been 
    /// changed and which will be further changed.</param>
    [OnChange(BasicContext)]
    private void SampleContextOperation(BasicContext basicContext)
    {
        if (basicContext.State != 0 && basicContext.StateList.Count == 0)
            basicContext.StateList.Add(basicContext.State);
    }
}

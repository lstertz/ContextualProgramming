namespace ContextualProgramming;

/// <summary>
/// Declares a class as a behavior, which permits its instances to be managed as 
/// constructs that depend upon and manipulate, through contextual execution, 
/// the app's contextual state.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
public abstract class BaseBehaviorAttribute : Attribute
{
}

/// <inheritdoc/>
public class BehaviorAttribute : BaseBehaviorAttribute
{
}
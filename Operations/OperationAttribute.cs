namespace ContextualProgramming;


/// <summary>
/// Declares a behavior's method as an operation, which specifies that the method 
/// should be invoked with its specified dependencies under conditions defined by other 
/// operation-detailing attributes on the method.
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
public abstract class BaseOperationAttribute : Attribute
{
}

/// <inheritdoc/>
public class OperationAttribute : BaseBehaviorAttribute
{
}
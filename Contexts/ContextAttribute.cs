namespace ContextualProgramming;

/// <summary>
/// Declares a class as a context, which defines its instances as managed data constructs that 
/// are considered with all other contexts to define the app's contextual state.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
public abstract class BaseContextAttribute : Attribute
{
}

/// <inheritdoc/>
public class ContextAttribute : BaseContextAttribute
{
}
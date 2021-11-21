namespace ContextualProgramming;

/// <summary>
/// Declares a class as a Context, which defines its instances as managed data constructs that 
/// are considered with all other Contexts to define the app's contextual state.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public class ContextAttribute : Attribute
{
}
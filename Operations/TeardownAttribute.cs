namespace ContextualProgramming;


/// <summary>
/// Defines an operation as being responsive to the teardown of a behavior.
/// </summary>
/// <remarks>
/// A behavior is torn down as a result of one of its dependencies 
/// being decontextualized. Teardown happens immediately before the behavior's destruction.
/// </remarks>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
public class TeardownAttribute : Attribute { }
namespace ContextualProgramming.Internal;


/// <summary>
/// Defines an operation that should be invoked every update cycle.
/// </summary>
/// <remarks>
/// Within the update cycle, update operations are invoked after behaviors are torn down 
/// and before new behaviors have been initialized.
/// </remarks>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
public class OnUpdateAttribute : Attribute { }
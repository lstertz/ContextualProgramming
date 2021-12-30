namespace ContextualProgramming;


/// <summary>
/// Defines an operation as being responsive to a specified context's change in state.
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
public class OnChangeAttribute : Attribute
{
    /// <summary>
    /// The optional name of the dependency's specific context state whose 
    /// changes would invoke the operation.
    /// </summary>
    public string? ContextStateName { get; init; }

    /// <summary>
    /// The name of the behavior's specific dependency whose changes would invoke the operation.
    /// </summary>
    public string DependencyName { get; init; }


    /// <summary>
    /// Constructs a new on change attribute.
    /// </summary>
    /// <param name="dependencyName"><see cref="DependencyName"/></param>
    /// <param name="contextStateName"><see cref="ContextStateName"/></param>
    /// <exception cref="ArgumentException">Thrown if <paramref name="dependencyName"/>
    /// is null or empty.</exception>
    public OnChangeAttribute(string dependencyName, string? contextStateName = null)
    {
        if (string.IsNullOrEmpty(dependencyName))
            throw new ArgumentException($"'{nameof(dependencyName)}' cannot be null or empty.",
                nameof(dependencyName));

        DependencyName = dependencyName;
        ContextStateName = contextStateName;
    }
}
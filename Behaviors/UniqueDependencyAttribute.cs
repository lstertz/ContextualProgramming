namespace ContextualProgramming;

/// <summary>
/// Declares a unique dependency of a behavior (<see cref="BehaviorAttribute"/>) for the 
/// specified type of context.
/// </summary>
/// <typeparam name="T">The type of the dependency, a type of a context.</typeparam>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
public class UniqueDependencyAttribute<T> : Attribute
{
    /// <summary>
    /// The name that identifies the dependency for all operations within the behavior.
    /// </summary>
    public string DependencyName { get; init; }

    /// <summary>
    /// The type of the dependency, a type of a context.
    /// </summary>
    public Type DependencyType { get; init; }

    /// <summary>
    /// Whether the dependency is fulfilled by the dependent behavior during the 
    /// behavior's construction.
    /// </summary>
    public bool IsSelfFulfilled { get; init; }


    /// <summary>
    /// Constructs a new unique dependency attribute.
    /// </summary>
    /// <param name="dependencyName"><see cref="DependencyName"/></param>
    /// <param name="isSelfFulfilled"><see cref="IsSelfFulfilled"/></param>
    public UniqueDependencyAttribute(string dependencyName, bool isSelfFulfilled = false) =>
        (DependencyName, DependencyType, IsSelfFulfilled) = 
        (dependencyName, typeof(T), isSelfFulfilled);
}
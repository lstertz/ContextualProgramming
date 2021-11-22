namespace ContextualProgramming;

/// <summary>
/// Declares a dependency of a behavior (<see cref="BehaviorAttribute"/>) for 
/// a type of context.
/// </summary>
public abstract class DependencyAttribute : Attribute
{
    /// <summary>
    /// The type of the context that the behavior is dependent upon.
    /// </summary>
    public Type ContextType { get; init; }

    /// <summary>
    /// Whether the dependency is fulfilled by the dependent behavior during the 
    /// behavior's construction.
    /// </summary>
    public bool IsSelfFulfilled { get; init; }

    /// <summary>
    /// The name that identifies the dependency for all operations within the behavior.
    /// </summary>
    public string Name { get; init; }

    /// <summary>
    /// The type of the dependency.
    /// </summary>
    public DependencyType Type { get; init; }


    /// <summary>
    /// Constructs a new unique dependency attribute.
    /// </summary>
    /// <param name="type"><see cref="Type"/></param>
    /// <param name="name"><see cref="Name"/></param>
    /// <param name="contextType"><see cref="ContextType"/></param>
    /// <param name="isSelfFulfilled"><see cref="IsSelfFulfilled"/></param>
    protected DependencyAttribute(DependencyType type, string name, Type contextType, 
        bool isSelfFulfilled)
    {
        if (string.IsNullOrEmpty(name))
        {
            throw new ArgumentException($"'{nameof(name)}' cannot be null or empty.", nameof(name));
        }

        if (contextType is null)
        {
            throw new ArgumentNullException(nameof(contextType));
        }

        Type = type;
        Name = name;
        ContextType = contextType;
        IsSelfFulfilled = isSelfFulfilled;
    }
}


/// <summary>
/// Declares a unique dependency of a behavior (<see cref="BehaviorAttribute"/>) for the 
/// specified type of context.
/// </summary>
/// <typeparam name="T">The type of the dependency, a type of a context.</typeparam>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
public class DependencyAttribute<T> : DependencyAttribute
{
    /// <summary>
    /// Constructs a new unique dependency attribute.
    /// </summary>
    /// <param name="type"><see cref="DependencyAttribute.Type"/></param>
    /// <param name="name"><see cref="DependencyAttribute.Name"/></param>
    /// <param name="isSelfFulfilled"><see cref="DependencyAttribute.IsSelfFulfilled"/></param>
    public DependencyAttribute(DependencyType type, string name, bool isSelfFulfilled = false) :
        base(type, name, typeof(T), isSelfFulfilled) { }
}


/// <summary>
/// The types of dependencies.
/// </summary>
public enum DependencyType
{
    /// <summary>
    /// A shared dependency, meaning that the dependency may fulfill the requirements of 
    /// more than one of the same type of behavior.
    /// </summary>
    Shared,
    /// <summary>
    /// A unique dependency, meaning that the dependency may fulfill the requirements of 
    /// only one of the same type of behavior.
    /// </summary>
    Unique
}
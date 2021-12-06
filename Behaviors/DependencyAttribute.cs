namespace ContextualProgramming;

/// <summary>
/// Declares a dependency of a behavior (<see cref="BehaviorAttribute"/>) for 
/// a type of context.
/// </summary>
public abstract class DependencyAttribute : Attribute
{
    /// <summary>
    /// Specifies how the dependency is to be bound to the behavior.
    /// </summary>
    public Binding Binding { get; init; }

    /// <summary>
    /// Specifies how the dependency is to be fulfilled.
    /// </summary>
    public Fulfillment Fulfillment { get; init; }

    /// <summary>
    /// The name that identifies the dependency for all operations within the behavior.
    /// </summary>
    public string Name { get; init; }

    /// <summary>
    /// The type of the context that the behavior is dependent upon.
    /// </summary>
    public Type Type { get; init; }


    /// <summary>
    /// Constructs a new dependency attribute.
    /// </summary>
    /// <param name="type"><see cref="Binding"/></param>
    /// <param name="fulfillment"><see cref="Fulfillment"/></param>
    /// <param name="name"><see cref="Name"/></param>
    /// <param name="contextType"><see cref="Type"/></param>
    protected DependencyAttribute(Binding type, Fulfillment fulfillment, 
        string name, Type contextType)
    {
        if (string.IsNullOrEmpty(name))
        {
            throw new ArgumentException($"'{nameof(name)}' cannot be null or empty.", nameof(name));
        }

        if (contextType is null)
        {
            throw new ArgumentNullException(nameof(contextType));
        }

        Binding = type;
        Name = name;
        Fulfillment = fulfillment;
        Type = contextType;
    }
}


/// <summary>
/// Declares a dependency of a behavior (<see cref="BehaviorAttribute"/>) for the 
/// specified type of context.
/// </summary>
/// <typeparam name="T">The type of the dependency, a type of a context.</typeparam>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
public class DependencyAttribute<T> : DependencyAttribute
{
    /// <summary>
    /// Constructs a new dependency attribute.
    /// </summary>
    /// <param name="binding"><see cref="DependencyAttribute.Binding"/></param>
    /// <param name="name"><see cref="DependencyAttribute.Name"/></param>
    /// <param name="fulfillment"><see cref="DependencyAttribute.Fulfillment"/></param>
    public DependencyAttribute(Binding binding, Fulfillment fulfillment, 
        string name) : base(binding, fulfillment, name, typeof(T)) { }
}


/// <summary>
/// The different means by which a dependency is expected to be fulfilled for a behavior.
/// </summary>
public enum Fulfillment
{
    /// <summary>
    /// The dependency will automatically be satisfied by a default instance of the context.
    /// </summary>
    //Default,  // Not currently supported.
    /// <summary>
    /// The dependency can only be fulfilled by an existing qualifying context.
    /// </summary>
    //Existing,  // Not currently supported.
    /// <summary>
    /// The dependency will be fulfilled by an existing qualifying context, if one exists, 
    /// otherwise it is expected to be created by the dependent behavior during its construction.
    /// </summary>
    //ExistingOrSelfCreated,  // Not currently supported.
    /// <summary>
    /// The dependency is expected to be created by the dependent behavior during its construction.
    /// </summary>
    SelfCreated
}

/// <summary>
/// The different ways that a dependency can be bound to a behavior.
/// </summary>
public enum Binding
{
    /// <summary>
    /// A shared binding, meaning that the dependency may fulfill the requirements of 
    /// more than one of the same type of behavior.
    /// </summary>
    //Shared,  // Not currently supported.
    /// <summary>
    /// A unique binding, meaning that the dependency may fulfill the requirements of 
    /// only one of the same type of behavior.
    /// </summary>
    Unique
}
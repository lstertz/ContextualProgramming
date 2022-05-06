namespace ContextualProgramming;

/// <summary>
/// Declares a mutualistic relationship of a type of context (<see cref="BaseContextAttribute"/>) 
/// with a type of context.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
public abstract class BaseMutualismAttribute : Attribute
{
    /// <summary>
    /// The name that identifies the mutualist context.
    /// </summary>
    public string Name { get; init; }

    /// <summary>
    /// The relationship that the mutualist is required to have with teh host.
    /// </summary>
    public Relationship Relationship { get; init; }

    /// <summary>
    /// The type of the mutualist context.
    /// </summary>
    public Type Type { get; init; }


    /// <summary>
    /// Constructs a new mutualism attribute.
    /// </summary>
    /// <param name="name"><see cref="Name"/></param>
    /// <param name="relationship"><see cref="Relationship"/></param>
    /// <param name="type"><see cref="Type"/></param>
    /// <exception cref="ArgumentException">Thrown if <paramref name="name"/> 
    /// is null or empty.</exception>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="type"/> 
    /// is null.</exception>
    protected BaseMutualismAttribute(string name, Relationship relationship, Type type)
    {
        if (string.IsNullOrEmpty(name))
            throw new ArgumentException($"'{nameof(name)}' cannot be null or empty.", 
                nameof(name));

        Name = name;
        Relationship = relationship;
        Type = type.EnsureNotNull();
    }
}

/// <inheritdoc/>
public abstract class MutualismAttribute : BaseMutualismAttribute
{
    /// <inheritdoc/>
    public MutualismAttribute(string name, Relationship relationship, Type type) : 
        base(name, relationship, type) { }
}

/// <inheritdoc/>
/// <typeparam name="T">The type of the mutualist context.</typeparam>
public class MutualismAttribute<T> : MutualismAttribute
{
    /// <summary>
    /// Constructs a new mutualism attribute.
    /// </summary>
    /// <param name="name"><see cref="BaseMutualismAttribute.Name"/></param>
    /// <param name="relationship"><see cref="BaseMutualismAttribute.Relationship"/></param>
    public MutualismAttribute(string name, Relationship relationship) : 
        base(name, relationship, typeof(T)) { }
}


/// <summary>
/// The different ways that a mutualist can be bound to a host.
/// </summary>
public enum Relationship
{
    /// <summary>
    /// An exclusive relationship, meaning that any automatically provided mutualist may 
    /// fulfill the requirements of only one of the same type of host.
    /// </summary>
    Exclusive,
    ///// <summary>
    ///// A nonexclusive relationship, meaning that any automatically provided mutualist may 
    ///// fulfill the requirements of more than one of the same type of host.
    ///// </summary>
    //Nonexclusive  // Not currently supported.
}
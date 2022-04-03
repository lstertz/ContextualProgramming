namespace ContextualProgramming;

/// <summary>
/// Declares a contract of a context (<see cref="BaseContextAttribute"/>) for 
/// a type of context.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
public abstract class BaseContractAttribute : Attribute
{
    /// <summary>
    /// The name that identifies the contracted context.
    /// </summary>
    public string Name { get; init; }

    /// <summary>
    /// The type of the context that is being contracted.
    /// </summary>
    public Type Type { get; init; }


    /// <summary>
    /// Constructs a new contract attribute.
    /// </summary>
    /// <param name="name"><see cref="Name"/></param>
    /// <param name="type"><see cref="Type"/></param>
    /// <exception cref="ArgumentException">Thrown if <paramref name="name"/> 
    /// is null or empty.</exception>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="type"/> 
    /// is null.</exception>
    protected BaseContractAttribute(string name, Type type)
    {
        if (string.IsNullOrEmpty(name))
            throw new ArgumentException($"'{nameof(name)}' cannot be null or empty.", 
                nameof(name));

        Name = name;
        Type = type.EnsureNotNull();
    }
}


/// <inheritdoc/>
public abstract class ContractAttribute : BaseContractAttribute
{
    /// <inheritdoc/>
    public ContractAttribute(string name, Type type) : base(name, type) { }
}

/// <inheritdoc/>
/// <typeparam name="T">The type of the contracted context.</typeparam>
public class ContractAttribute<T> : ContractAttribute
{
    /// <summary>
    /// Constructs a new contract attribute.
    /// </summary>
    /// <param name="name"><see cref="BaseContractAttribute.Name"/></param>
    public ContractAttribute(string name) : base(name, typeof(T)) { }
}
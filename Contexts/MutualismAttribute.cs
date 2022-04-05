namespace ContextualProgramming;

/// <summary>
/// Declares a mutulistic relationship of a type of context (<see cref="BaseContextAttribute"/>) 
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
    /// The type of the mutualist context.
    /// </summary>
    public Type Type { get; init; }


    /// <summary>
    /// Constructs a new mutualism attribute.
    /// </summary>
    /// <param name="name"><see cref="Name"/></param>
    /// <param name="type"><see cref="Type"/></param>
    /// <exception cref="ArgumentException">Thrown if <paramref name="name"/> 
    /// is null or empty.</exception>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="type"/> 
    /// is null.</exception>
    protected BaseMutualismAttribute(string name, Type type)
    {
        if (string.IsNullOrEmpty(name))
            throw new ArgumentException($"'{nameof(name)}' cannot be null or empty.", 
                nameof(name));

        Name = name;
        Type = type.EnsureNotNull();
    }
}


/// <inheritdoc/>
public abstract class MutualismAttribute : BaseMutualismAttribute
{
    /// <inheritdoc/>
    public MutualismAttribute(string name, Type type) : base(name, type) { }
}

/// <inheritdoc/>
/// <typeparam name="T">The type of the mutualist context.</typeparam>
public class MutualismAttribute<T> : MutualismAttribute
{
    /// <summary>
    /// Constructs a new mutualism attribute.
    /// </summary>
    /// <param name="name"><see cref="BaseMutualismAttribute.Name"/></param>
    public MutualismAttribute(string name) : base(name, typeof(T)) { }
}
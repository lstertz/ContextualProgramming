namespace ContextualProgramming;

/// <summary>
/// Declares a state of a context to be mutual with the state of a mutalist context.
/// </summary>
/// <remarks>
/// The values of mutual states will always be the same and a change to one is 
/// recorded as a change to the other.
/// </remarks>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = true, Inherited = true)]
public abstract class BaseMutualAttribute : Attribute
{
    /// <summary>
    /// The name that identifies the mutualist context.
    /// </summary>
    public string MutualistName { get; init; }

    /// <summary>
    /// The name that identifies the mutual state of the mutuallist context.
    /// </summary>
    public string StateName { get; init; }


    /// <summary>
    /// Constructs a new mutual attribute.
    /// </summary>
    /// <param name="mutualistName"><see cref="MutualistName"/></param>
    /// <param name="stateName"><see cref="StateName"/></param>
    /// <exception cref="ArgumentException">Thrown if <paramref name="mutualistName"/> 
    /// is null or empty.</exception>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="stateName"/> 
    /// is null or empty.</exception>
    protected BaseMutualAttribute(string mutualistName, string stateName)
    {
        if (string.IsNullOrEmpty(mutualistName))
            throw new ArgumentException($"'{nameof(mutualistName)}' cannot be null or empty.", 
                nameof(mutualistName));

        if (string.IsNullOrEmpty(stateName))
            throw new ArgumentException($"'{nameof(stateName)}' cannot be null or empty.",
                nameof(stateName));

        MutualistName = mutualistName;
        StateName = stateName;
    }
}


/// <inheritdoc/>
public class MutualAttribute : BaseMutualAttribute
{
    /// <inheritdoc/>
    public MutualAttribute(string mutualistName, string stateName) : 
        base(mutualistName, stateName) { }
}
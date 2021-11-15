namespace ContextualProgramming;

/// <summary>
/// Declares a property of a behavior (<see cref="BehaviorAttribute"/>) as being a 
/// dependency of the behavior.
/// </summary>
/// <remarks>The decorated property must be a <see cref="Context"/> or 
/// of a interface. An interface dependency will only be fulfilled if a <see cref="Context"/> 
/// that implements that interface is instantiated.
/// </remarks>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public class DependencyAttribute : Attribute
{
    /// <summary>
    /// Denotes the source of the dependency.
    /// </summary>
    public DependencySource Source { get; }

    /// <summary>
    /// Constructs a new dependency.
    /// </summary>
    /// <param name="source">The source of the dependency.</param>
    public DependencyAttribute(DependencySource source) => (Source) = source;
}

/// <summary>
/// Defines the possible sources of dependencies.
/// </summary>
public enum DependencySource
{
    /// <summary>
    /// The dependent will create its dependency itself at some point during its 
    /// initialization or construction.
    /// </summary>
    Self,
    /// <summary>
    /// The dependent will expect the dependency to be fulfilled, but the provided dependency
    /// may be shared among other instances of the same type of dependent.
    /// </summary>
    Shared,
    /// <summary>
    /// The dependent will expect the dependency to be fulfilled, but the provided dependency
    /// can not be shared among other instances of the same type of dependent.
    /// </summary>
    Unique
}
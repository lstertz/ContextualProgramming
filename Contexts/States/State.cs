namespace ContextualProgramming.Internal;

/// <summary>
/// Encapsulates a value as a state.
/// </summary>
/// <typeparam name="T">The type of value encapsulated.</typeparam>
public abstract class State<T> : IEquatable<State<T>>
{
    /// <summary>
    /// Checks for equality between two states.
    /// </summary>
    /// <param name="a">The first state.</param>
    /// <param name="b">The second state.</param>
    /// <returns>Whether the two states are equal.</returns>
    public static bool operator ==(State<T>? a, State<T>? b) =>
        Equals(a, null) ? Equals(b, null) : a.Equals(b);

    /// <summary>
    /// Checks for inequality between two states.
    /// </summary>
    /// <param name="a">The first state.</param>
    /// <param name="b">The second state.</param>
    /// <returns>Whether the two states are inequal.</returns>
    public static bool operator !=(State<T>? a, State<T>? b) =>
        Equals(a, null) ? !Equals(b, null) : !a.Equals(b);


    /// <summary>
    /// The encapsulated value of the state.
    /// </summary>
    protected IBindableValue<T> InternalValue { get; set; }


    /// <summary>
    /// Constructs a new state with the specified value for it to encapsulate.
    /// </summary>
    /// <param name="value">The encapsulated value of the state.</param>
    protected State(T value) => InternalValue = new BindableValue<T>(value);


    /// <summary>
    /// Converts the provided object to its equivalent state.
    /// </summary>
    /// <param name="other">The object to be converted.</param>
    /// <returns>The resulting state, if conversion was possibly, null otherwise.</returns>
    protected abstract State<T>? Convert(object? other);

    /// <inheritdoc/>
    public override bool Equals(object? other) => Equals(Convert(other));

    /// <inheritdoc/>
    public virtual bool Equals(State<T>? other)
    {
        if (Equals(other, null))
            return false;

        if (InternalValue.Value == null)
            return other.InternalValue.Value == null;

        return InternalValue.Value.Equals(other.InternalValue.Value);
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        if (InternalValue.Value == null)
            return 0;

        return InternalValue.Value.GetHashCode();
    }

    /// <inheritdoc/>
    public override string? ToString() => InternalValue.Value == null ? "" : 
        InternalValue.Value.ToString();
}

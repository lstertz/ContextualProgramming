namespace ContextualProgramming.Internal;

/// <summary>
/// Encapsulates a value as a state.
/// </summary>
/// <typeparam name="T">The type of value encapsulated.</typeparam>
public abstract class State<T> : IEquatable<State<T>>
{
    /// <summary>
    /// Implicitly converts a state to its underlying value.
    /// </summary>
    /// <param name="state">The state to be converted.</param>
    public static implicit operator T?(State<T> state) => state._value;

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
    public T? Value { get => _value; protected set => _value = value; }
    private T? _value;


    /// <summary>
    /// Constructs a new state with the specified value for it to encapsulate.
    /// </summary>
    /// <param name="value">The encapsulated value of the state.</param>
    protected State(T? value)
    {
        _value = value;
    }


    /// <summary>
    /// Converts the provided object to its equivalent state.
    /// </summary>
    /// <param name="other">The object to be converted.</param>
    /// <returns>The resulting state, if conversion was possibly, null otherwise.</returns>
    protected abstract State<T>? Convert(object? other);

    /// <inheritdoc/>
    public override bool Equals(object? other) => Equals(Convert(other));

    /// <inheritdoc/>
    public bool Equals(State<T>? other)
    {
        if (Equals(other, null))
            return false;

        if (_value == null)
            return other._value == null;

        return _value.Equals(other._value);
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        if (_value == null)
            return 0;

        return _value.GetHashCode();
    }

    /// <inheritdoc/>
    public override string? ToString() => _value == null ? "" : _value.ToString();
}

namespace ContextualProgramming.Internal;

/// <summary>
/// Encapsulates a value bound as a state to at least one context.
/// </summary>
/// <remarks>
/// A bound value tracks the states it is registered as being bound to and notifies 
/// subscribers of <see cref="OnChange"/> when it has been flagged as having been changed.
/// </remarks>
public interface IBoundValue
{
    /// <summary>
    /// The action invoked whenever the bound value has been flagged as having been changed.
    /// </summary>
    /// <remarks>
    /// Subscribers are notified for each supposed change (even if an actual change has 
    /// not occurred) and for each registered context state. The context and the 
    /// name of the state that has changed are captured in the 
    /// provided <see cref="ContextChange"/>.
    /// </remarks>
    public event Action<ContextChange> OnChange
    {
        add { }
        remove { }
    }


    /// <summary>
    /// Deregisters the provided context's state from this bound value, 
    /// thus unbinding the state from the value.
    /// </summary>
    /// <param name="context">The context whose state is to be deregistered.</param>
    void DeregisterContextState(object context);

    /// <summary>
    /// Registers the provided context's state with this bound value, 
    /// thus binding the state to the value.
    /// </summary>
    /// <remarks>
    /// Registering a different state for a previously registered context will override 
    /// any previous registration.
    /// </remarks>
    /// <param name="context">The context whose state is to be registered.</param>
    /// <param name="stateName">The name of the state to be registered.</param>
    void RegisterContextState(object context, string stateName);


    /// <summary>
    /// Flags the bound value as having been changed, which results in the 
    /// invocation of <see cref="OnChange"/> for each registered context state.
    /// </summary>
    void FlagAsChanged();
}

/// <inheritdoc/>
public interface IBoundValue<T> : IBoundValue 
{
    /// <summary>
    /// The encapsulated value of the bound value.
    /// </summary>
    T Value { get; set; }
}

/// <inheritdoc/>
public class BoundValue<T> : IBoundValue<T>
{
    /// <inheritdoc/>
    public T Value 
    { 
        get => throw new NotImplementedException(); 
        set => throw new NotImplementedException(); 
    }


    /// <summary>
    /// Constructs a new bound value with the specified initial value.
    /// </summary>
    /// <param name="value">The initial value of the bound value.</param>
    public BoundValue(T value) => Value = value;


    /// <inheritdoc/>
    public void DeregisterContextState(object context)
    {
        throw new NotImplementedException();
    }
    /// <inheritdoc/>

    public void RegisterContextState(object context, string stateName)
    {
        throw new NotImplementedException();
    }


    /// <inheritdoc/>
    public void FlagAsChanged()
    {
        throw new NotImplementedException();
    }
}

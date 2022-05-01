using System.Diagnostics.CodeAnalysis;

namespace ContextualProgramming.Internal;

/// <summary>
/// Encapsulates a value that can be bound to at least one context state.
/// </summary>
/// <remarks>
/// A bindable value tracks the states it is registered as being bound to and notifies 
/// subscribers of <see cref="OnChange"/> when it has been flagged as having been changed.
/// </remarks>
public interface IBindableValue
{
    /// <summary>
    /// The action invoked whenever the value has been flagged as having been changed.
    /// </summary>
    /// <remarks>
    /// Subscribers are notified for each supposed change (even if an actual change has 
    /// not occurred) and for each registered context state. The context and the 
    /// name of the state that has changed are captured in the 
    /// provided <see cref="ContextChange"/>.
    /// </remarks>
    event Action<ContextChange> OnChange;


    /// <summary>
    /// Deregisters the provided context's state from this value, 
    /// thus unbinding the state from the value.
    /// </summary>
    /// <param name="context">The context whose state is to be deregistered.</param>
    void DeregisterContextState(object context);

    /// <summary>
    /// Registers the provided context's state with this value, 
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
    /// Flags the value as having been changed, which results in the 
    /// invocation of <see cref="OnChange"/> for each registered context state.
    /// </summary>
    void FlagAsChanged();
}

/// <inheritdoc/>
public interface IBindableValue<T> : IBindableValue
{
    /// <summary>
    /// The encapsulated value of the bindable value.
    /// </summary>
    T Value { get; set; }
}

/// <inheritdoc/>
public class BindableValue<T> : IBindableValue<T>
{
    /// <inheritdoc/>
    public event Action<ContextChange> OnChange
    {
        add
        {
            lock (_objectLock)
                _onChange += value;
        }
        remove
        {
            lock (_objectLock)
                _onChange -= value;
        }
    }
    private Action<ContextChange>? _onChange;
    private readonly object _objectLock = new();

    /// <inheritdoc/>
    public T Value { get; set; }


    private readonly List<ContextChange> _contextStates = new();


    /// <summary>
    /// Constructs a new bindable value with the specified initial value.
    /// </summary>
    /// <param name="value">The initial value of the bindable value.</param>
    public BindableValue(T value) => Value = value;


    /// <inheritdoc/>
    public void DeregisterContextState(object context)
    {
        context.EnsureNotNull();

        for (int c = _contextStates.Count - 1; c >= 0; c--)
            if (_contextStates[c].Context == context)
                _contextStates.RemoveAt(c);
    }

    /// <inheritdoc/>
    public void RegisterContextState(object context, string stateName)
    {
        _contextStates.Add(new ContextChange(context, stateName));
    }


    /// <inheritdoc/>
    public void FlagAsChanged()
    {
        for (int c = 0, count = _contextStates.Count; c < count; c++)
            _onChange?.Invoke(_contextStates[c]);
    }
}

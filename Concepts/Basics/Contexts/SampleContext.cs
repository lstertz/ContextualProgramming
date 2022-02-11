using ContextualProgramming.Concepts.Behaviors; 

namespace ContextualProgramming.Concepts.Contexts;


/// <summary>
/// Contexts define the state of a Contextual Programming application.
/// 
/// They are managed by any instance of App that they are contextualized for
/// with <see cref="App.Contextualize{T}(T)"/>, until they are decontextualized with
/// <see cref="App.Decontextualize{T}(T)"/>. Contexts self-created by a behavior, 
/// as is done by an <see cref="InitializationBehavior"/>, are automatically contextualized.
///
/// Any class can be designated as a context through the <see cref="ContextAttribute"/>.
/// </summary>
[Context]
public class SampleContext
{
    // All members of a context should either be a state property or a qualifier.

    /// <summary>
    /// This is a standard state property.
    /// 
    /// It must be of a type such <see cref="ContextState{T}"/> and its generic 
    /// typing should be a primitive-like value (int, string, etc.).
    /// 
    /// The ContextState value itself should not change after the context is instantiated, so 
    /// the properties should be 'get; init;' as it is here.
    /// 
    /// The state's value should be set through <see cref="ContextState{T}.Value"/>, 
    /// but it can be accessed directly through implicit conversion.
    /// 
    /// These restrictions ensure that the app knows when a state's value has been changed, so 
    /// that it can be properly processed by the relevant behaviors.
    /// </summary>
    public ContextState<int> State { get; init; } = 0;

    /// <summary>
    /// This is a standard state list property.
    /// 
    /// It has the same restrictions as the non-list property, but provides a means of 
    /// managing a collection of primitive-like values instead of only a single value.
    /// </summary>
    public ContextStateList<int> StateList { get; init; } = Array.Empty<int>();


    /// <summary>
    /// This is a qualifier.
    /// It is the only type of method permitted on a context.
    /// 
    /// Qualifiers provide a means of evaluating more abstractly whether a context is in a 
    /// specific state that will likely drive behavior.
    /// </summary>
    /// <returns>
    /// Whether the context has qualified for a certain condition.
    /// </returns>
    public bool ExampleQualifier() => State > 0 && StateList.Elements?.Length > 0;
}
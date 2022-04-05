namespace ContextualProgramming.Internal;

/// <summary>
/// Fulfills the mutualistic relationships of a context.
/// </summary>
public interface IMutualismFulfiller
{
    /// <summary>
    /// Provides the types of the mutualist contexts that would be instantiated 
    /// by this factory.
    /// </summary>
    /// <remarks>
    /// This includes one type for each mutualist, regardless of duplicates.
    /// </remarks>
    Type[] MutualistContextTypes { get; }


    /// <summary>
    /// Fulfills the mutualistic relationships of the provided context by instantiating the 
    /// mutualist contexts and binding any mutual states.
    /// </summary>
    /// <param name="context">The context whose mutualistic relationsihps 
    /// are to be fulfilled.</param>
    /// <returns>The instantiated mutualist contexts (not yet contextualized).</returns>
    object[] Fulfill(object context);
}


/// <inheritdoc cref="IMutualismFulfiller"/>
public class MutualismFulfiller : IMutualismFulfiller
{
    /// <inheritdoc/>
    public Type[] MutualistContextTypes { get; private set; }


    /// <summary>
    /// Constructs a new mutualism fulfiller.
    /// </summary>
    /// <param name="mutualists">The names and types of mutualist contexts that 
    /// would be instantiated and set up to fulfill the mutualistic relationships of 
    /// the fulfiller's context.</param>
    public MutualismFulfiller(Dictionary<string, Type>? mutualists)
    {
        MutualistContextTypes = mutualists != null ?
            mutualists.Values.ToArray() : Array.Empty<Type>();
    }

    /// <inheritdoc/>
    public object[] Fulfill(object context)
    {
        context.EnsureNotNull();

        object[] mutualists = new object[MutualistContextTypes.Length];
        for (int c = 0, count = mutualists.Length; c < count; c++)
            mutualists[c] = Activator.CreateInstance(
                MutualistContextTypes[c]).EnsureNotNull();

        return mutualists;
    }
}
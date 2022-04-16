namespace ContextualProgramming.Internal;

/// <summary>
/// Fulfills the mutualistic relationships of a context.
/// </summary>
public interface IMutualismFulfiller
{
    /// <summary>
    /// Provides the names of the mutualist contexts that would be instantiated 
    /// by this factory.
    /// </summary>
    /// <remarks>
    /// This includes one name for each mutualist. Each name is unique within 
    /// the contexts of the fulfiller.
    /// </remarks>
    string[] MutualistContextNames { get; }

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
    /// <returns>The instantiated mutualist contexts (not yet contextualized) paired 
    /// with their mutualist names.</returns>
    Tuple<string, object>[] Fulfill(object context);
}


/// <inheritdoc cref="IMutualismFulfiller"/>
public class MutualismFulfiller : IMutualismFulfiller
{
    /// <inheritdoc/>
    public string[] MutualistContextNames { get; private set; }

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
        MutualistContextNames = mutualists != null ?
            mutualists.Keys.ToArray() : Array.Empty<string>();
    }

    /// <inheritdoc/>
    public Tuple<string,object>[] Fulfill(object context)
    {
        context.EnsureNotNull();

        Tuple<string, object>[] mutualists = new Tuple<string, object>[
            MutualistContextTypes.Length];
        for (int c = 0, count = mutualists.Length; c < count; c++)
            mutualists[c] = new (MutualistContextNames[c], Activator.CreateInstance(
                MutualistContextTypes[c]).EnsureNotNull());

        return mutualists;
    }
}
using System.Reflection;

namespace ContextualProgramming.Internal;

/// <summary>
/// Fulfills the contracts of a context.
/// </summary>
public interface IContractFulfiller
{
    /// <summary>
    /// Provides the types of the contracted contexts that would be instantiated 
    /// by this factory.
    /// </summary>
    /// <remarks>
    /// This includes one type for each contract, regardless of duplicates.
    /// </remarks>
    Type[] ContractedContextTypes { get; }


    /// <summary>
    /// Fulfills the contracts of the provided context by instantiated the 
    /// contracted contexts and binding any contracted states.
    /// </summary>
    /// <param name="context">The context whose contracts are to be fulfilled.</param>
    /// <returns>The instantiated contracted contexts (not yet contextualized).</returns>
    object[] Fulfill(object context);
}


/// <inheritdoc cref="IContractFulfiller"/>
public class ContractFulfiller : IContractFulfiller
{
    /// <inheritdoc/>
    public Type[] ContractedContextTypes { get; private set; }


    /// <summary>
    /// Constructs a new contract fulfiller.
    /// </summary>
    /// <param name="contracts">The names and types of contracted contexts that 
    /// would be instantiated and set up to fulfill the contracts of 
    /// the fulfiller's context.</param>
    public ContractFulfiller(Dictionary<string, Type> contracts)
    {
        ContractedContextTypes = contracts != null ? 
            contracts.Values.ToArray() : Array.Empty<Type>();
    }

    /// <inheritdoc/>
    public object[] Fulfill(object context)
    {
        context.EnsureNotNull();

        object[] contractedContexts =new object[ContractedContextTypes.Length];
        for (int c = 0, count = contractedContexts.Length; c < count; c++)
            contractedContexts[c] = Activator.CreateInstance(
                ContractedContextTypes[c]).EnsureNotNull();

        return contractedContexts;
    }
}
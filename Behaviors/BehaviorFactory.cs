using System.Reflection;

namespace ContextualProgramming.Internal;

/// <summary>
/// Creates the <see cref="BehaviorInstance"/>s of a specific type of behavior, based on available 
/// contexts that are required to fulfill that behavior type's dependencies.
/// </summary>
public class BehaviorFactory
{
    /// <summary>
    /// Specifies whether this factory can instantiate an instance of its behavior 
    /// if <see cref="Process"/> was called.
    /// </summary>
    public bool CanInstantiate => NumberOfPendingInstantiations != 0;

    /// <summary>
    /// Provides the number of instantiations that would be made 
    /// through <see cref="Process"/>.
    /// </summary>
    /// <remarks>
    /// A value of -1 indicates that an infinite number of instantiations can be made.
    /// Under this condition, <see cref="Process"/> will only instantiate one instance 
    /// for each call.
    /// </remarks>
    public int NumberOfPendingInstantiations { get; private set; }

    private readonly Dictionary<string, HashSet<object>> _availableDependencies = new();

    private readonly ConstructorInfo _constructor;

    private readonly Dictionary<Type, string[]> _dependencyTypesNames = new();

    public BehaviorFactory(ConstructorInfo constructor, Tuple<string, Type>[] behaviorDependencies)
    {
        _constructor = constructor;

        for (int c = 0, count = behaviorDependencies.Length; c < count; c++)
        {
            // TODO :: Parse the dependencies.
        }

        NumberOfPendingInstantiations = DeterminePendingInstantiations();
    }


    public bool AddAvailableDependency(object dependency)
    {
        // TODO :: Add dependency.

        NumberOfPendingInstantiations = DeterminePendingInstantiations();
        return CanInstantiate;
    }

    public BehaviorInstance[] Process()
    {
        int instantiationCount = NumberOfPendingInstantiations;

        // No dependencies are required, infinite instantiations are possible.
        // Instantiate only one per process call to make the instantiations controllable.
        if (instantiationCount == -1)
            instantiationCount = 1;

        BehaviorInstance[] newInstances = new BehaviorInstance[instantiationCount];
        for (int c = 0, count = newInstances.Length; c < count; c++)
            newInstances[c] = InstantiateBehavior();

        return newInstances;
    }


    private int DeterminePendingInstantiations()
    {
        int instantiationCount = -1;
        foreach (var dependencySet in _availableDependencies.Values)
        {
            if (instantiationCount == -1)
                instantiationCount = dependencySet.Count;
            else if (instantiationCount < dependencySet.Count)
                instantiationCount = dependencySet.Count;

            if (instantiationCount == 0)
                return 0;
        }

        return instantiationCount;
    }

    /// <summary>
    /// Instantiates an instance of the factory's behavior, using/associating any available 
    /// dependencies as required by the behavior.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if the behavior does not 
    /// construct the expected self-created dependency during its instantiation.</exception>
    private BehaviorInstance InstantiateBehavior()
    {
        ParameterInfo[] parameters = _constructor.GetParameters();

        object[] arguments = new object[parameters.Length];
        object behavior = _constructor.Invoke(arguments);

        Dictionary<string, object> contexts = new();
        for (int c = 0, count = parameters.Length; c < count; c++)
        {
            try
            {
                contexts.Add(parameters[c].Name.EnsureNotNull(), arguments[c].EnsureNotNull());
            }
            catch (ArgumentNullException)
            {
                Type behaviorType = _constructor.DeclaringType.EnsureNotNull();
                throw new InvalidOperationException($"The default constructor for the " +
                    $"behavior of type {behaviorType.FullName} did not construct an " +
                    $"expected dependency of type {parameters[c].ParameterType.FullName}.");
            }
        }

        return new(behavior, contexts, arguments);
    }
}
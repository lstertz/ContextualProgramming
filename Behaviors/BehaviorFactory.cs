using System.Reflection;

namespace ContextualProgramming.Internal;

/// <summary>
/// Creates the <see cref="BehaviorInstance"/>s of a specific type of behavior, based on available 
/// contexts that are required to fulfill that behavior type's dependencies.
/// </summary>
public interface IBehaviorFactory
{
    /// <summary>
    /// Specifies whether this factory can instantiate an instance of its behavior 
    /// if <see cref="Process"/> was called.
    /// </summary>
    bool CanInstantiate { get; }

    /// <summary>
    /// Provides the number of instantiations that would be made 
    /// through <see cref="Process"/>.
    /// </summary>
    /// <remarks>
    /// A value of -1 indicates that an infinite number of instantiations can be made.
    /// Under this condition, <see cref="Process"/> will only instantiate one instance 
    /// for each call.
    /// </remarks>
    int NumberOfPendingInstantiations { get; }

    /// <summary>
    /// Provides the types of the dependencies required for this factory to 
    /// instantiate a behavior.
    /// </summary>
    Type[] RequiredDependencyTypes { get; }


    /// <summary>
    /// Adds the provided dependency as an available dependency to be used in 
    /// the instantiation of a behavior.
    /// </summary>
    /// <param name="dependency">The dependency to be added.</param>
    /// <returns>Whether an instantiation can be made with the dependencies 
    /// that are available, after this addition.</returns>
    bool AddAvailableDependency(object dependency);

    /// <summary>
    /// Processes the available dependencies to instantiate all possible behaviors, unless 
    /// an infinite number of dependencies can be instantiated, in which only one 
    /// instance will be created.
    /// </summary>
    /// <returns>Any newly instantiated behaviors.</returns>
    BehaviorInstance[] Process();

    /// <summary>
    /// Removes the provided dependency as an available dependency to be used in 
    /// the instantiation of a behavior.
    /// </summary>
    /// <param name="dependency">The dependency to be removed.</param>
    /// <returns>Whether an instantiation can be made with the dependencies 
    /// that are available, after this removal.</returns>
    bool RemoveAvailableDependency(object dependency);
}


/// <inheritdoc cref="IBehaviorFactory"/>
public class BehaviorFactory : IBehaviorFactory
{
    /// <inheritdoc/>
    public bool CanInstantiate => NumberOfPendingInstantiations != 0;

    /// <inheritdoc/>
    public int NumberOfPendingInstantiations { get; private set; }

    /// <inheritdoc/>
    public Type[] RequiredDependencyTypes { get; private set; }


    private readonly Dictionary<Type, HashSet<object>> _availableDependencies = new();

    private readonly ConstructorInfo _constructor;

    private readonly Dictionary<Type, List<string>> _dependencyTypesNames = new();


    /// <summary>
    /// Constructs a new behavior factory.
    /// </summary>
    /// <param name="constructor">The constructor of the behaviors that would be 
    /// instantiated by this factory.</param>
    /// <param name="requiredDependencies">The dependencies that must be fulfilled for 
    /// a behavior to be instantiated.</param>
    public BehaviorFactory(ConstructorInfo constructor,
        Dictionary<string, Type> requiredDependencies)
    {
        _constructor = constructor.EnsureNotNull();

        HashSet<Type> dependencyTypes = new();
        foreach (string dependencyName in requiredDependencies.Keys)
        {
            Type dependencyType = requiredDependencies[dependencyName];
            dependencyTypes.Add(dependencyType);
            _availableDependencies.TryAdd(dependencyType, new());

            if (!_dependencyTypesNames.ContainsKey(dependencyType))
                _dependencyTypesNames.Add(dependencyType, new());

            _dependencyTypesNames[dependencyType].Add(dependencyName);
        }

        RequiredDependencyTypes = dependencyTypes.ToArray();
        NumberOfPendingInstantiations = DeterminePendingInstantiations();
    }


    /// <inheritdoc/>
    public bool AddAvailableDependency(object dependency)
    {
        dependency.EnsureNotNull();

        Type dependencyType = dependency.GetType();
        if (!_dependencyTypesNames.ContainsKey(dependencyType))
            return CanInstantiate;

        _availableDependencies[dependencyType].Add(dependency);

        NumberOfPendingInstantiations = DeterminePendingInstantiations();
        return CanInstantiate;
    }

    /// <inheritdoc/>
    public BehaviorInstance[] Process()
    {
        int instantiationCount = NumberOfPendingInstantiations;

        // No dependencies are required; infinite instantiations are possible.
        // Instantiate only one per process call to make the instantiations controllable.
        if (instantiationCount == -1)
            instantiationCount = 1;

        BehaviorInstance[] newInstances = new BehaviorInstance[instantiationCount];
        for (int c = 0, count = newInstances.Length; c < count; c++)
            newInstances[c] = InstantiateBehavior();

        NumberOfPendingInstantiations = DeterminePendingInstantiations();

        return newInstances;
    }

    /// <inheritdoc/>
    public bool RemoveAvailableDependency(object dependency)
    {
        dependency.EnsureNotNull();

        Type dependencyType = dependency.GetType();
        if (!_dependencyTypesNames.ContainsKey(dependencyType))
            return CanInstantiate;

        _availableDependencies[dependencyType].Remove(dependency);

        NumberOfPendingInstantiations = DeterminePendingInstantiations();
        return CanInstantiate;
    }


    /// <summary>
    /// Determines the number of instantiations that could be performed with the 
    /// currently available dependencies.
    /// </summary>
    /// <returns>The number of potential instantiations or -1 if 
    /// an inifinite number of instantiations are possible.</returns>
    private int DeterminePendingInstantiations()
    {
        int instantiationCount = -1;
        foreach (var type in _availableDependencies.Keys)
        {
            int setCount = _availableDependencies[type].Count /
                _dependencyTypesNames[type].Count;

            if (instantiationCount == -1)
                instantiationCount = setCount;
            else if (instantiationCount > setCount)
                instantiationCount = setCount;

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
        Dictionary<string, object> contexts = new();
        foreach (Type dependencyType in _availableDependencies.Keys)
        {
            List<string> dependencyNames = _dependencyTypesNames[dependencyType];
            for (int c = 0, count = dependencyNames.Count; c < count; c++)
            {
                object dependency = _availableDependencies[dependencyType].First();
                contexts.Add(dependencyNames[c], dependency);
                _availableDependencies[dependencyType].Remove(dependency);
            }
        }

        ParameterInfo[] parameters = _constructor.GetParameters();

        int selfCreatedContextCount = 0;
        object[] arguments = new object[parameters.Length];
        for (int c = 0, count = parameters.Length; c < count; c++)
            if (!parameters[c].IsOut)
                arguments[c] = contexts[parameters[c].Name.EnsureNotNull()];
            else
                selfCreatedContextCount++;

        object[] selfCreatedContexts = new object[selfCreatedContextCount];
        object behavior = _constructor.Invoke(arguments);

        for (int c = 0, sc = 0, count = parameters.Length; c < count; c++)
        {
            if (!parameters[c].IsOut)
                continue;

            selfCreatedContexts[sc] = arguments[c];
            sc++;

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

        return new(behavior, contexts, selfCreatedContexts);
    }
}
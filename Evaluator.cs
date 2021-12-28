using ContextualProgramming.Internal;
using System.Reflection;

namespace ContextualProgramming;


/// <summary>
/// Evaluates and caches code constructs to define behaviors, contexts, and their relationships.
/// </summary>
public abstract class Evaluator
{
    private bool _isInitialized = false;


    /// <summary>
    /// Evaluates all assemblies and their types to determine all behaviors and contexts 
    /// to be known by this evaluator.
    /// </summary>
    /// <remarks>
    /// Does nothing if initialization has already been performed.
    /// </remarks>
    public void Initialize()
    {
        if (_isInitialized)
            return;

        InitializeContextTypes();
        InitializeBehaviorTypes();

        _isInitialized = true;
    }

    /// <summary>
    /// Evaluates all assemblies and their types to determine all behaviors 
    /// to be known by this evaluator.
    /// </summary>
    protected abstract void InitializeBehaviorTypes();

    /// <summary>
    /// Evaluates all assemblies and their types to determine all contexts 
    /// to be known by this evaluator.
    /// </summary>
    protected abstract void InitializeContextTypes();

    /// <summary>
    /// Validates that the evaluator has been initialized.
    /// </summary>
    /// <exception cref="InvalidOperationException">The exception thrown if the 
    /// evaluator has not been initialized.</exception>
    protected void ValidateInitialization()
    {
        if (!_isInitialized)
            throw new InvalidOperationException($"This evaluator has not yet been initialized. " +
                $"Initialize with {nameof(Evaluator.Initialize)} prior to using the evaluator.");
    }

    /// <summary>
    /// Provides the constructor for the specified behavior.
    /// </summary>
    /// <param name="behaviorType">The type of behavior whose constructor 
    /// is to be provided.</param>
    /// <returns>The specified behavior's constructor.</returns>
    public abstract ConstructorInfo GetBehaviorConstructor(Type behaviorType);

    /// <summary>
    /// Provides the behavior types found by this evaluator.
    /// </summary>
    /// <returns>All types found to be behaviors.</returns>
    public abstract Type[] GetBehaviorTypes();

    /// <summary>
    /// Provides the bindable state property infos for the specified context type.
    /// </summary>
    /// <param name="contextType">The type of context whose bindable state property infos
    /// are to be provided.</param>
    /// <returns>The property infos of all bindable states found for the 
    /// specified context type.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the specified context 
    /// type is not a type of context known to this evaluator.</exception>
    public abstract PropertyInfo[] GetBindableStateInfos(Type contextType);

    /// <summary>
    /// Provides the context types found by this evaluator.
    /// </summary>
    /// <returns>All types found to be contexts.</returns>
    public abstract Type[] GetContextTypes();

    /// <summary>
    /// Provides the behavior types of behaviors that can instantiate without 
    /// any required dependencies, as found by this evaluator.
    /// </summary>
    /// <returns>All behavior types found to be behaviors that can be 
    /// instantiated without any required dependencies.</returns>
    public abstract Type[] GetInitializationBehaviorTypes();

    /// <summary>
    /// Specifies whether the provided type is a type of context known by this evaluator.
    /// </summary>
    /// <param name="type">The type to be checked.</param>
    /// <returns>Whether the provided type is a type of context.</returns>
    public abstract bool IsContextType(Type type);
}

/// <inheritdoc/>
/// <typeparam name="TContextAttribute">The type of attribute that defines a context.</typeparam>
/// <typeparam name="TBehaviorAttribute">The type of attribute that defines a behavior.</typeparam>
/// <typeparam name="TDependencyAttribute">The type of attribute that defines 
/// a dependency of a behavior.</typeparam>
public class Evaluator<TContextAttribute, TBehaviorAttribute, TDependencyAttribute> : Evaluator
    where TContextAttribute : ContextAttribute
    where TBehaviorAttribute : BehaviorAttribute
    where TDependencyAttribute : DependencyAttribute
{
    /// <summary>
    /// A mapping of each behavior to its constructor.
    /// </summary>
    private readonly Dictionary<Type, ConstructorInfo> _behaviorConstructors = new();

    /// <summary>
    /// The types of all dependencies of all behavior types.
    /// </summary>
    private readonly Dictionary<Type, Type[]> _behaviorDependencies = new();

    /// <summary>
    /// The names and their dependency indices for the self-created 
    /// dependencies of all behavior types.
    /// </summary>
    private readonly Dictionary<Type, Dictionary<string, int>>
        _behaviorSelfCreatedDependencies = new();

    /// <summary>
    /// All found behavior types.
    /// </summary>
    private readonly HashSet<Type> _behaviorTypes = new();

    /// <summary>
    /// A mapping of context types to their properties that can be bound when 
    /// new instances are contextualized.
    /// </summary>
    private readonly Dictionary<Type, PropertyInfo[]> _contextBindableProperties = new();

    /// <summary>
    /// A mapping of context types to the behavior types that depend upon them.
    /// </summary>
    private readonly Dictionary<Type, List<Type>> _contextTypesToBehaviorTypes = new();

    /// <summary>
    /// All found context types.
    /// </summary>
    private readonly HashSet<Type> _contextTypes = new();

    /// <summary>
    /// All found initializaiton behaviors.
    /// </summary>
    private Type[] _initializationBehaviors = Array.Empty<Type>();


    /// <inheritdoc/>
    protected override void InitializeBehaviorTypes()
    {
        Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
        for (int c = 0, count = assemblies.Length; c < count; c++)
        {
            Type[] types = assemblies[c].GetTypes();
            for (int cc = 0, cCount = types.Length; cc < cCount; cc++)
                if (CacheBehaviorType(types[cc]))
                {
                    CacheBehaviorDependencies(types[cc]);
                    CacheBehaviorConstructors(types[cc]);
                }
        }

        CacheInitializationBehaviorTypes();
    }

    /// <inheritdoc/>
    protected override void InitializeContextTypes()
    {
        Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
        for (int c = 0, count = assemblies.Length; c < count; c++)
        {
            Type[] types = assemblies[c].GetTypes();
            for (int cc = 0, cCount = types.Length; cc < cCount; cc++)
            {
                CacheContextType(types[cc]);
                CacheBindableStateInfos(types[cc]);
            }
        }
    }


    /// <inheritdoc/>
    public override ConstructorInfo GetBehaviorConstructor(Type behaviorType)
    {
        ValidateInitialization();

        if (!_behaviorConstructors.ContainsKey(behaviorType))
            throw new ArgumentException($"Behavior constructors cannot be retrieved " +
                $"for type {behaviorType.FullName} since it is not a behavior " +
                $"known to an evaluator with behaviors defined " +
                $"by {typeof(TBehaviorAttribute).FullName}.");

        return _behaviorConstructors[behaviorType];
    }

    /// <inheritdoc/>
    public override Type[] GetBehaviorTypes()
    {
        ValidateInitialization();

        return _behaviorTypes.ToArray();
    }

    /// <inheritdoc/>
    public override PropertyInfo[] GetBindableStateInfos(Type contextType)
    {
        ValidateInitialization();

        if (!_contextTypes.Contains(contextType))
            throw new ArgumentException($"Bindable states cannot be retrieved " +
                $"for type {contextType.FullName} since it is not a context " +
                $"known to an evaluator with contexts defined " +
                $"by {typeof(TContextAttribute).FullName}.");

        return _contextBindableProperties[contextType];
    }

    /// <inheritdoc/>
    public override Type[] GetContextTypes()
    {
        ValidateInitialization();

        return _contextTypes.ToArray();
    }

    /// <inheritdoc/>
    public override Type[] GetInitializationBehaviorTypes()
    {
        ValidateInitialization();

        return _initializationBehaviors;
    }

    /// <inheritdoc/>
    public override bool IsContextType(Type type)
    {
        ValidateInitialization();

        return _contextTypes.Contains(type);
    }


    /// <summary>
    /// Validates and caches constructors for instantiating behaviors.
    /// </summary>
    /// <param name="behaviorType">The type of behavior whose constructors should 
    /// be validated and cached.</param>
    /// <exception cref="InvalidOperationException">Thrown if the behavior's 
    /// constructors are not valid for its declared dependencies.</exception>
    private void CacheBehaviorConstructors(Type behaviorType)
    {
        Type[] depTypes = _behaviorDependencies[behaviorType];
        Dictionary<string, int> selfCreatedDeps = _behaviorSelfCreatedDependencies[behaviorType];

        // Only self-created dependencies are currently supported, so the Behavior can be created.
        ConstructorInfo[] constructors = behaviorType.GetConstructors(
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance |
            BindingFlags.FlattenHierarchy);
        if (constructors.Length == 0)
            throw new InvalidOperationException($"Behavior of type {behaviorType.FullName} " +
                $"does not have a valid constructor.");

        ConstructorInfo constructor = constructors[0]; // Assume one constructor for now.
        ParameterInfo[] parameters = constructor.GetParameters();
        if (parameters.Length != depTypes.Length)
            throw new InvalidOperationException($"The default constructor for the behavior " +
                $"of type {behaviorType.FullName} is not valid for its dependencies.");

        for (int pc = 0, pCount = parameters.Length; pc < pCount; pc++)
        {
            ParameterInfo parameterInfo = parameters[pc];
            string? parameterName = parameterInfo.Name;

            if (parameterName == null)
                throw new InvalidOperationException($"The default constructor for the behavior " +
                    $"of type {behaviorType.FullName} is not valid for its dependencies.");

            if (!selfCreatedDeps.ContainsKey(parameterName))
                throw new InvalidOperationException($"The default constructor for the behavior " +
                    $"of type {behaviorType.FullName} is not valid for its dependencies.");

            if (!parameterInfo.IsOut)
                throw new InvalidOperationException($"The default constructor for the behavior " +
                    $"of type {behaviorType.FullName} is not valid for its dependencies.");

            Type refDepType = depTypes[selfCreatedDeps[parameterName]].MakeByRefType();
            if (refDepType != parameterInfo.ParameterType)
                throw new InvalidOperationException($"The default constructor for the behavior " +
                    $"of type {behaviorType.FullName} is not valid for its dependencies.");
        }

        _behaviorConstructors.Add(behaviorType, constructor);
    }

    /// <summary>
    /// Validates and caches the dependency-related details of the behavior type.
    /// </summary>
    /// <param name="behaviorType">The type of behavior whose dependency-related details 
    /// are to be cached.</param>
    /// <exception cref="InvalidOperationException">Thrown if a dependency is 
    /// not a type of context known to this evaluator.</exception>
    private void CacheBehaviorDependencies(Type behaviorType)
    {
        IEnumerable<DependencyAttribute> attrs =
            behaviorType.GetCustomAttributes<DependencyAttribute>(true);

        Type[] depTypes = new Type[attrs.Count()];
        Dictionary<string, int> selfCreatedDeps = new();

        int depIndex = 0;
        foreach (DependencyAttribute attr in attrs)
        {
            Type t = attr.Type;
            if (!_contextTypes.Contains(t))
                throw new InvalidOperationException($"The dependency named {attr.Name} of type" +
                    $"{t} for the behavior of type {behaviorType.FullName} is not a context " +
                    $"known to an evaluator with contexts defined " +
                    $"by {typeof(TContextAttribute).FullName}.");

            depTypes[depIndex] = t;
            switch (attr.Fulfillment)
            {
                case Fulfillment.SelfCreated:
                    selfCreatedDeps.Add(attr.Name, depIndex);
                    break;
                default:
                    break;
            }

            List<Type> dependents = _contextTypesToBehaviorTypes.GetValueOrDefault(t, new());
            if (!dependents.Contains(behaviorType))
                dependents.Add(behaviorType);

            depIndex++;
        }

        _behaviorDependencies.Add(behaviorType, depTypes);
        _behaviorSelfCreatedDependencies.Add(behaviorType, selfCreatedDeps);
    }

    /// <summary>
    /// Validates whether the provided type is a type of behavior known to this evaluator 
    /// and caches it if it is.
    /// </summary>
    /// <param name="type">The type to validated and cached.</param>
    /// <returns>Whether the type is a cached behavior type.</returns>
    private bool CacheBehaviorType(Type type)
    {
        if (_behaviorTypes.Contains(type))
            return true;
        else if (type.GetCustomAttribute<TBehaviorAttribute>(true) != null)
        {
            _behaviorTypes.Add(type);
            return true;
        }
        return false;
    }

    /// <summary>
    /// If not already cached, finds and caches all bindable state property infos 
    /// found within the provided context type.
    /// </summary>
    /// <param name="contextType">The type of context whose bindable state property 
    /// infos should be cached.</param>
    private void CacheBindableStateInfos(Type contextType)
    {
        if (_contextBindableProperties.ContainsKey(contextType))
            return;

        PropertyInfo[] properties = contextType.GetProperties(BindingFlags.Instance |
            BindingFlags.Public | BindingFlags.NonPublic);
        List<PropertyInfo> bindableProperties = new();
        for (int c = 0, count = properties.Length; c < count; c++)
            if (typeof(IBindableState).IsAssignableFrom(properties[c].PropertyType))
                bindableProperties.Add(properties[c]);

        _contextBindableProperties.Add(contextType, bindableProperties.ToArray());
    }

    /// <summary>
    /// Validates whether the provided type is a type of context known to this evaluator 
    /// and caches it if it is.
    /// </summary>
    /// <param name="type">The type to validated and cached.</param>
    /// <returns>Whether the type is a cached context type.</returns>
    private bool CacheContextType(Type type)
    {
        if (_contextTypes.Contains(type))
            return true;
        else if (type.GetCustomAttribute<TContextAttribute>(true) != null)
        {
            _contextTypes.Add(type);
        }
        return false;
    }

    /// <summary>
    /// Determines and caches the behavior types of initialization behaviors.
    /// </summary>
    private void CacheInitializationBehaviorTypes()
    {
        List<Type> initializationBehaviors = new();
        foreach (Type behaviorType in _behaviorSelfCreatedDependencies.Keys)
            if (_behaviorSelfCreatedDependencies[behaviorType].Count == 
                _behaviorDependencies[behaviorType].Length)
                initializationBehaviors.Add(behaviorType);

        _initializationBehaviors = initializationBehaviors.ToArray();
    }
}
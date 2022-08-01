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

        string executingAssemblyName = Assembly.GetExecutingAssembly().GetName().FullName;
        IEnumerable<Assembly> assembliesToEvaluate = AppDomain.CurrentDomain.GetAssemblies()
            .Where(a => a.GetReferencedAssemblies()
            .Select(name => name.FullName)
            .Contains(executingAssemblyName))
            .Append(Assembly.GetExecutingAssembly());

        InitializeContextTypes(assembliesToEvaluate);
        InitializeBehaviorTypes(assembliesToEvaluate);

        _isInitialized = true;
    }

    /// <summary>
    /// Evaluates all assemblies and their types to determine all behaviors 
    /// to be known by this evaluator.
    /// </summary>
    /// <param name="assembliesToEvaluate">The assemblies whose types should 
    /// be evaluated for behavior types.</param>
    protected abstract void InitializeBehaviorTypes(IEnumerable<Assembly> assembliesToEvaluate);

    /// <summary>
    /// Evaluates all assemblies and their types to determine all contexts 
    /// to be known by this evaluator.
    /// </summary>
    /// <param name="assembliesToEvaluate">The assemblies whose types should 
    /// be evaluated for context types.</param>
    protected abstract void InitializeContextTypes(IEnumerable<Assembly> assembliesToEvaluate);

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
    /// Provides a new factory for instantiating the specified behavior.
    /// </summary>
    /// <param name="behaviorType">The type of behavior whose factory  
    /// is to be provided.</param>
    /// <returns>A factory for the specified behavior.</returns>
    public abstract IBehaviorFactory BuildBehaviorFactory(Type behaviorType);

    /// <summary>
    /// Provides a new factory for instantiating the mutualist contexts of 
    /// the specified context.
    /// </summary>
    /// <param name="contextType">The type of context whose mutualism factory 
    /// is to be provided.</param>
    /// <returns>A fulfiller for the fulfilling the mutualistic relationships 
    /// of the specified context or null if the context has no mutualistic relationships.</returns>
    public abstract IMutualismFulfiller? BuildMutualismFulfiller(Type contextType);

    /// <summary>
    /// Provides the dependencies required by the specified behavior for an 
    /// instance of that behavior to be instantiated.
    /// </summary>
    /// <param name="behaviorType">The type of behavior whose required dependencies 
    /// are to be provided.</param>
    /// <returns>The types of the specified behavior's required dependencies.
    /// Duplicates indicate how many of the same type of dependency that is required.</returns>
    public abstract Type[] GetBehaviorRequiredDependencies(Type behaviorType);

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
    /// The mutual state infos, paired with their mutualist's name, for the specified context type.
    /// </summary>
    /// <param name="contextType">The type of context whose mutual state  
    /// infos are to be provided.</param>
    /// <returns>An array of mutual state info.</returns>
    public abstract MutualStateInfo[] GetMutualStateInfos(Type contextType);

    /// <summary>
    /// Provides the operations of the specified behavior type that should 
    /// be invoked when a context of an instance of that behavior type has changed.
    /// </summary>
    /// <remarks>Excluding the <paramref name="contextStateName"/> will only provide 
    /// the on change operations that do not define a state name.</remarks>
    /// <param name="behaviorType">The type of the behavior whose operations 
    /// are to be provided.</param>
    /// <param name="contextName">The name of the behavior's context whose 
    /// on change operations should be provided.</param>
    /// <param name="contextStateName">The name of the behavior's context's 
    /// specific state whose on change operaitons should be provided.</param>
    /// <returns>The behavior type's operations that should be invoked 
    /// for the specified context or context state change.</returns>
    public abstract MethodInfo[] GetOnChangeOperations(Type behaviorType,
        string contextName, string? contextStateName = null);

    /// <summary>
    /// Provides the operations of the specified behavior type that should 
    /// be invoked when the behavior is being torn down.
    /// </summary>
    /// <param name="behaviorType">The type of the behavior whose operations 
    /// are to be provided.</param>
    /// <returns>The behavior type's operations that should be invoked 
    /// when the behavior is being torn down.</returns>
    public abstract MethodInfo[] GetOnTeardownOperations(Type behaviorType);

    /// <summary>
    /// Specifies whether the provided type is a type of context known by this evaluator.
    /// </summary>
    /// <param name="type">The type to be checked.</param>
    /// <returns>Whether the provided type is a type of context.</returns>
    public abstract bool IsContextType(Type type);
}

/// <inheritdoc/>
/// <typeparam name="TContextAttribute">The type of attribute that defines a context.</typeparam>
/// <typeparam name="TMutualismAttribute">The base type of attribute that defines a 
/// a mutualistic relationship of a context with a context.</typeparam>
/// <typeparam name="TMutualAttribute">The base type of attribute that defines a 
/// context state as being mutual with a mutualist context's state.</typeparam>
/// <typeparam name="TBehaviorAttribute">The type of attribute that defines a behavior.</typeparam>
/// <typeparam name="TDependencyAttribute">The base type of attribute 
/// that defines a dependency of a behavior.</typeparam>
/// <typeparam name="TOperationAttribute">The type of attribute 
/// that defines an operation of a behavior.</typeparam>
public class Evaluator<TContextAttribute, TMutualismAttribute, TMutualAttribute,
    TBehaviorAttribute, TDependencyAttribute, TOperationAttribute> : Evaluator
    where TContextAttribute : BaseContextAttribute
    where TMutualismAttribute : BaseMutualismAttribute
    where TMutualAttribute : BaseMutualAttribute
    where TBehaviorAttribute : BaseBehaviorAttribute
    where TDependencyAttribute : BaseDependencyAttribute
    where TOperationAttribute : BaseOperationAttribute
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
    /// The names and their dependency indices for the existing 
    /// dependencies of all behavior types.
    /// </summary>
    private readonly Dictionary<Type, Dictionary<string, int>>
        _behaviorExistingDependencies = new();

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
    /// A mapping of context types to their mutual properties and their paired 
    /// mutualists' properties with their paired mutualist names.
    /// </summary>
    private readonly Dictionary<Type, MutualStateInfo[]> _contextMutualProperties = new();

    /// <summary>
    /// The names and mutualist context types for all context types.
    /// </summary>
    /// <remarks>
    /// Contexts without mutualistic relationships are not included in this collection.
    /// </remarks>
    private Dictionary<Type, Dictionary<string, Type>> _contextMutualists = new();

    /// <summary>
    /// All found context types.
    /// </summary>
    private readonly HashSet<Type> _contextTypes = new();

    /// <summary>
    /// All found operations that should be invoked upon a context change.
    /// </summary>
    private readonly Dictionary<Type, Dictionary<string, Dictionary<string, List<MethodInfo>>>>
        _onChangeOperations = new();

    /// <summary>
    /// All found operations that should be invoked upon teardown.
    /// </summary>
    private readonly Dictionary<Type, List<MethodInfo>> _onTeardownOperations = new();


    /// <inheritdoc/>
    protected override void InitializeBehaviorTypes(IEnumerable<Assembly> assembliesToEvaluate)
    {
        foreach (Assembly assembly in assembliesToEvaluate)
        {
            Type[] types = assembly.GetTypes();
            for (int c = 0, count = types.Length; c < count; c++)
                if (CacheBehaviorType(types[c]))
                {
                    CacheBehaviorDependencies(types[c]);
                    CacheBehaviorConstructors(types[c]);
                    CacheOperations(types[c]);
                }
        }
    }

    /// <inheritdoc/>
    protected override void InitializeContextTypes(IEnumerable<Assembly> assembliesToEvaluate)
    {
        foreach (Assembly assembly in assembliesToEvaluate)
        {
            Type[] types = assembly.GetTypes();
            for (int c = 0, count = types.Length; c < count; c++)
                CacheContextType(types[c]);

            foreach (Type type in _contextTypes)
            {
                CacheContextMutualisms(type);
                CacheBindableStateInfos(type);
                CacheMutualStateInfos(type);
            }
        }
    }


    /// <inheritdoc/>
    public override IBehaviorFactory BuildBehaviorFactory(Type behaviorType)
    {
        ValidateInitialization();

        if (!_behaviorConstructors.ContainsKey(behaviorType))
            throw new ArgumentException($"A behavior factory cannot be built " +
                $"for type {behaviorType.FullName} since it is not a behavior " +
                $"known to an evaluator with behaviors defined " +
                $"by {typeof(TBehaviorAttribute).FullName}.");

        Dictionary<string, Type> requiredDependencies = new();
        foreach (var dependency in _behaviorExistingDependencies[behaviorType])
            requiredDependencies.Add(dependency.Key,
                _behaviorDependencies[behaviorType][dependency.Value]);

        return new BehaviorFactory(_behaviorConstructors[behaviorType], requiredDependencies);
    }

    /// <inheritdoc/>
    public override IMutualismFulfiller? BuildMutualismFulfiller(Type contextType)
    {
        ValidateInitialization();

        if (!_contextTypes.Contains(contextType))
            throw new ArgumentException($"A mutualism fulfiller cannot be built " +
                $"for type {contextType.FullName} since it is not a context " +
                $"known to an evaluator with contexts defined " +
                $"by {typeof(TContextAttribute).FullName}.");

        if (!_contextMutualists.ContainsKey(contextType))
            return null;

        return new MutualismFulfiller(_contextMutualists[contextType]);
    }

    /// <inheritdoc/>
    public override Type[] GetBehaviorRequiredDependencies(Type behaviorType)
    {
        ValidateInitialization();

        if (!_behaviorExistingDependencies.ContainsKey(behaviorType))
            throw new ArgumentException($"Behavior dependencies cannot be retrieved " +
                $"for type {behaviorType.FullName} since it is not a behavior " +
                $"known to an evaluator with behaviors defined " +
                $"by {typeof(TBehaviorAttribute).FullName}.");

        Dictionary<string, int> existingDeps = _behaviorExistingDependencies[behaviorType];
        if (existingDeps.Count == 0)
            return Array.Empty<Type>();

        Type[] allDependencies = _behaviorDependencies[behaviorType];
        HashSet<Type> requiredDependencies = new();
        foreach (int depIndex in existingDeps.Values)
            requiredDependencies.Add(allDependencies[depIndex]);

        return requiredDependencies.ToArray();
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
    public override MutualStateInfo[] GetMutualStateInfos(Type contextType)
    {
        ValidateInitialization();

        if (!_contextTypes.Contains(contextType))
            throw new ArgumentException($"Mutual states cannot be retrieved " +
                $"for type {contextType.FullName} since it is not a context " +
                $"known to an evaluator with contexts defined " +
                $"by {typeof(TContextAttribute).FullName}.");

        return _contextMutualProperties[contextType];
    }

    /// <inheritdoc/>
    public override MethodInfo[] GetOnChangeOperations(Type behaviorType,
        string contextName, string? contextStateName = null)
    {
        ValidateInitialization();

        if (!_behaviorTypes.Contains(behaviorType))
            throw new ArgumentException($"On Change Operations cannot be retrieved " +
                $"for type {behaviorType.FullName} since it is not a behavior " +
                $"known to an evaluator with behaviors defined " +
                $"by {typeof(TBehaviorAttribute).FullName}.");

        if (contextName == null)
            throw new ArgumentNullException(nameof(contextName));

        if (!_onChangeOperations.ContainsKey(behaviorType))
            return Array.Empty<MethodInfo>();

        if (!_onChangeOperations[behaviorType].ContainsKey(contextName))
            return Array.Empty<MethodInfo>();

        string stateName = contextStateName ?? "";
        if (!_onChangeOperations[behaviorType][contextName].ContainsKey(stateName))
            return Array.Empty<MethodInfo>();

        return _onChangeOperations[behaviorType][contextName][stateName].ToArray();
    }

    /// <inheritdoc/>
    public override MethodInfo[] GetOnTeardownOperations(Type behaviorType)
    {
        ValidateInitialization();

        if (!_behaviorTypes.Contains(behaviorType))
            throw new ArgumentException($"Teardown Operations cannot be retrieved " +
                $"for type {behaviorType.FullName} since it is not a behavior " +
                $"known to an evaluator with behaviors defined " +
                $"by {typeof(TBehaviorAttribute).FullName}.");

        if (!_onTeardownOperations.ContainsKey(behaviorType))
            return Array.Empty<MethodInfo>();

        return _onTeardownOperations[behaviorType].ToArray();
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
        Dictionary<string, int> existingDeps = _behaviorExistingDependencies[behaviorType];

        ConstructorInfo[] constructors = behaviorType.GetConstructors(
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance |
            BindingFlags.FlattenHierarchy);
        if (constructors.Length == 0)
            throw new InvalidOperationException($"Behavior of type {behaviorType.FullName} " +
                $"does not have a valid constructor.");

        ConstructorInfo constructor = constructors[0]; // Assume one constructor for now.
        ValidateConstructor(behaviorType.FullName, depTypes, existingDeps,
            selfCreatedDeps, constructor);

        _behaviorConstructors.Add(behaviorType, constructor);
    }

    /// <summary>
    /// Validates and caches the dependency-related details of the behavior type.
    /// </summary>
    /// <param name="behaviorType">The type of behavior whose dependency-related details 
    /// are to be cached.</param>
    /// <exception cref="InvalidOperationException">Thrown if a dependency is 
    /// not a type of context known to this evaluator or if multiple dependencies 
    /// have the same name.</exception>
    private void CacheBehaviorDependencies(Type behaviorType)
    {
        IEnumerable<TDependencyAttribute> attrs =
            behaviorType.GetCustomAttributes<TDependencyAttribute>(true);

        Type[] depTypes = new Type[attrs.Count()];
        Dictionary<string, int> selfCreatedDeps = new();
        Dictionary<string, int> existingDeps = new();

        int depIndex = 0;
        foreach (TDependencyAttribute attr in attrs)
        {
            Type t = attr.Type;
            string name = attr.Name;
            if (!_contextTypes.Contains(t))
                throw new InvalidOperationException($"The dependency named {name} of type " +
                    $"{t.FullName} for the behavior of type {behaviorType.FullName} is not a " +
                    $"context known to an evaluator with contexts defined " +
                    $"by {typeof(TContextAttribute).FullName}.");

            if (existingDeps.ContainsKey(name) || selfCreatedDeps.ContainsKey(name))
                throw new InvalidOperationException($"The dependency named {name} of type " +
                    $"{t.FullName} for the behavior of type {behaviorType.FullName} has the " +
                    $"same name as another of its behavior's dependencies.");

            depTypes[depIndex] = t;
            switch (attr.Fulfillment)
            {
                case Fulfillment.Existing:
                    existingDeps.Add(name, depIndex);
                    break;
                case Fulfillment.SelfCreated:
                    selfCreatedDeps.Add(name, depIndex);
                    break;
                default:
                    break;
            }

            depIndex++;
        }

        _behaviorDependencies.Add(behaviorType, depTypes);
        _behaviorExistingDependencies.Add(behaviorType, existingDeps);
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
        else if (type.GetCustomAttribute<TBehaviorAttribute>() != null)
        {
            _behaviorTypes.Add(type);
            return true;
        }
        return false;
    }

    /// <summary>
    /// If not already cached, validates and caches all bindable state property infos 
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
    /// If not already cached, validates and caches the mutualism-related details of 
    /// the provided context type.
    /// </summary>
    /// <param name="contextType">The type of context whose mutualism-related details 
    /// are to be cached.</param>
    /// <exception cref="InvalidOperationException">Thrown if a mutualist is 
    /// not a type of context known to this evaluator or if multiple mutualists 
    /// have the same name.</exception>
    private void CacheContextMutualisms(Type contextType)
    {
        if (_contextMutualists.ContainsKey(contextType))
            return;

        IEnumerable<TMutualismAttribute> attrs =
            contextType.GetCustomAttributes<TMutualismAttribute>(true);

        if (attrs.Count() == 0)
            return;

        Dictionary<string, Type> mutualists = new();

        foreach (TMutualismAttribute attr in attrs)
        {
            Type t = attr.Type;
            string name = attr.Name;
            if (!_contextTypes.Contains(t))
                throw new InvalidOperationException($"The mutualist named {name} of type " +
                    $"{t.FullName} for the context of type {contextType.FullName} is not a " +
                    $"context known to an evaluator with contexts defined " +
                    $"by {typeof(TContextAttribute).FullName}.");

            if (mutualists.ContainsKey(name))
                throw new InvalidOperationException($"The mutualist named {name} of type " +
                    $"{t.FullName} for the context of type {contextType.FullName} has the " +
                    $"same name as another of its context's mutualists.");

            mutualists.Add(name, t);
        }

        _contextMutualists.Add(contextType, mutualists);
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
        else if (type.GetCustomAttribute<TContextAttribute>() != null)
        {
            _contextTypes.Add(type);
        }
        return false;
    }

    /// <summary>
    /// If not already cached, validates and caches all mutual state property infos 
    /// found within the provided context type.
    /// </summary>
    /// <param name="contextType">The type of context whose mutual state property 
    /// infos should be cached.</param>
    private void CacheMutualStateInfos(Type contextType)
    {
        if (_contextMutualProperties.ContainsKey(contextType))
            return;

        PropertyInfo[] properties = contextType.GetProperties(BindingFlags.Instance |
            BindingFlags.Public | BindingFlags.NonPublic);
        if (properties.Length == 0)
        {
            _contextMutualProperties.Add(contextType, Array.Empty<MutualStateInfo>());
            return;
        }

        Dictionary<string, Type>? mutualists = null;
        HashSet<MutualStateInfo> mutualProperties = new();
        for (int c = 0, count = properties.Length; c < count; c++)
        {
            var attrs = properties[c].GetCustomAttributes<TMutualAttribute>(true);
            if (attrs.Count() == 0)
                continue;

            if (mutualists == null)
                mutualists = RetrieveMutualists(contextType);

            foreach (TMutualAttribute attr in attrs)
                mutualProperties.Add(new(properties[c], attr.MutualistName, 
                    RetrieveMutualistProperty(contextType, properties[c].PropertyType,
                        mutualists, attr)));
        }

        _contextMutualProperties.Add(contextType, mutualProperties.ToArray());
    }

    /// <summary>
    /// Provides the mutual property specified by the provided attribute, from the 
    /// provided mutualists.
    /// </summary>
    /// <param name="contextType">The type of context whose mutualist's property 
    /// is being retrieved.</param>
    /// <param name="propertyType">The type of the host's property.</param>
    /// <param name="mutualists">The mutualists that should provide the property.</param>
    /// <param name="attr">The attribute identifying the mutualist and property 
    /// to be provided.</param>
    /// <returns>The identified mutual property.</returns>
    /// <exception cref="Exception"></exception>
    private static PropertyInfo RetrieveMutualistProperty(Type contextType, Type propertyType,
        Dictionary<string, Type> mutualists, TMutualAttribute attr)
    {
        if (!mutualists.ContainsKey(attr.MutualistName))
            throw new InvalidOperationException($"The host context type {contextType} " +
                $"has a mutual property that identifies a mutualist, {attr.MutualistName}, " +
                $"not known to the host.");

        PropertyInfo? mutualistProperty = mutualists[attr.MutualistName]
            .GetProperty(attr.StateName, BindingFlags.Instance |
                BindingFlags.Public | BindingFlags.NonPublic);
        if (mutualistProperty == null)
            throw new InvalidOperationException($"The host context type {contextType} " +
                $"has a mutual property that identifies a state, {attr.StateName}, " +
                $"for the mutualist, {attr.MutualistName}, that does not " +
                $"exist on the mutualist.");

        if (mutualistProperty.PropertyType != propertyType)
            throw new InvalidOperationException($"The host context type {contextType} " +
                $"has a mutual property that identifies a state, {attr.StateName}, " +
                $"for the mutualist, {attr.MutualistName}, that has a value type " +
                $"different from the host's state's ({propertyType}).");

        return mutualistProperty;
    }

    /// <summary>
    /// Provides the mutualists for the specified context type.
    /// </summary>
    /// <param name="contextType">The type whose mutualists are to be provided.</param>
    /// <returns>The mutualists for the specified context type.</returns>
    /// <exception cref="Exception"></exception>
    private Dictionary<string, Type> RetrieveMutualists(Type contextType)
    {
        if (!_contextMutualists.ContainsKey(contextType))
            throw new InvalidOperationException($"The context type {contextType} " +
                $"has a mutual property but the context is not part of any " +
                $"mutualistic relationship. Decorate the context with the " +
                $"\"Mutualism\" attribute to define such a relationship.");

        return _contextMutualists[contextType];
    }

    /// <summary>
    /// Determines, validates, and caches the operations 
    /// of the specified behavior type.
    /// </summary>
    /// <param name="behaviorType">The type of behavior whose operations 
    /// should be cached.</param>
    private void CacheOperations(Type behaviorType)
    {
        MethodInfo[] methods = behaviorType.GetMethods(BindingFlags.Instance |
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);

        for (int c = 0, count = methods.Length; c < count; c++)
            if (methods[c].GetCustomAttribute<TOperationAttribute>() != null)
            {
                ValidateOperation(behaviorType, methods[c]);

                CacheOnChangeOperations(behaviorType, methods[c]);
                CacheOnTeardownOperations(behaviorType, methods[c]);
                // TODO :: Support other types of operation caching here.
            }
    }

    /// <summary>
    /// Validates and caches the provided operation as an on change operation 
    /// for each such declaration found to be associated with the operation.
    /// </summary>
    /// <param name="behaviorType">The type of behavior whose operation is being evaluated.</param>
    /// <param name="operation">The operation being evaluated, validated, and cached 
    /// for on change declarations.</param>
    private void CacheOnChangeOperations(Type behaviorType, MethodInfo operation)
    {
        IEnumerable<OnChangeAttribute> attrs = operation
            .GetCustomAttributes<OnChangeAttribute>(true);
        if (attrs.Count() > 0 && !_onChangeOperations.ContainsKey(behaviorType))
            _onChangeOperations.Add(behaviorType, new());

        foreach (OnChangeAttribute attr in attrs)
        {
            ValidateOnChangeOperation(behaviorType, operation, attr);

            string dn = attr.DependencyName;
            if (!_onChangeOperations[behaviorType].ContainsKey(dn))
                _onChangeOperations[behaviorType].Add(dn, new());

            string sn = string.IsNullOrEmpty(attr.ContextStateName) ? "" : attr.ContextStateName;
            if (!_onChangeOperations[behaviorType][dn].ContainsKey(sn))
                _onChangeOperations[behaviorType][dn].Add(sn, new());

            _onChangeOperations[behaviorType][dn][sn].Add(operation);
        }
    }

    /// <summary>
    /// Validates and caches the provided operation as an on teardown operation 
    /// for each such declaration found to be associated with the operation.
    /// </summary>
    /// <param name="behaviorType">The type of behavior whose operation is being evaluated.</param>
    /// <param name="operation">The operation being evaluated, validated, and cached 
    /// for on teardown declarations.</param>
    private void CacheOnTeardownOperations(Type behaviorType, MethodInfo operation)
    {
        IEnumerable<OnTeardownAttribute> attrs = operation
            .GetCustomAttributes<OnTeardownAttribute>(true);
        if (attrs.Count() == 0)
            return;

        if (!_onTeardownOperations.ContainsKey(behaviorType))
            _onTeardownOperations.Add(behaviorType, new());

        _onTeardownOperations[behaviorType].Add(operation);
    }


    /// <summary>
    /// Ensures the validity of a behavior's constructor.
    /// </summary>
    /// <param name="behaviorName">The name of the behavior whose constructor 
    /// is to be validated.</param>
    /// <param name="depTypes">The types of the behavior's dependencies.</param>
    /// <param name="existingDeps">The names and indices of the dependencies 
    /// that the behavior requires to already be existing when it is instantiated.</param>
    /// <param name="selfCreatedDeps">The names and indices of the dependencies 
    /// that the behavior will create when it is instantiated.</param>
    /// <param name="constructor">The constructor of the behavior to be validated.</param>
    /// <exception cref="InvalidOperationException">
    /// Thrown if the constructor is not valid for the behavior with the specified dependencies.
    /// </exception>
    private static void ValidateConstructor(string? behaviorName, Type[] depTypes,
        Dictionary<string, int> existingDeps, Dictionary<string, int> selfCreatedDeps,
        ConstructorInfo constructor)
    {
        ParameterInfo[] parameters = constructor.GetParameters();
        if (parameters.Length < selfCreatedDeps.Count)
            throw new InvalidOperationException($"The default constructor for the behavior " +
                $"of type {behaviorName} is not valid for its dependencies.");
        else if (parameters.Length > selfCreatedDeps.Count + existingDeps.Count)
            throw new InvalidOperationException($"The default constructor for the behavior " +
                $"of type {behaviorName} is not valid for its dependencies.");

        for (int pc = 0, pCount = parameters.Length; pc < pCount; pc++)
        {
            ParameterInfo parameterInfo = parameters[pc];
            string? parameterName = parameterInfo.Name;

            if (parameterName == null)
                throw new InvalidOperationException($"The default constructor for the behavior " +
                    $"of type {behaviorName} is not valid for its dependencies.");

            if (parameterInfo.IsOut)
            {
                if (!selfCreatedDeps.ContainsKey(parameterName))
                    throw new InvalidOperationException($"The default constructor for the " +
                        $"behavior of type {behaviorName} is not valid for its dependencies.");

                Type refDepType = depTypes[selfCreatedDeps[parameterName]].MakeByRefType();
                if (refDepType != parameterInfo.ParameterType)
                    throw new InvalidOperationException($"The default constructor for the " +
                        $"behavior of type {behaviorName} is not valid for its dependencies.");
            }
            else
            {
                if (!existingDeps.ContainsKey(parameterName))
                    throw new InvalidOperationException($"The default constructor for the " +
                        $"behavior of type {behaviorName} is not valid for its dependencies.");

                if (depTypes[existingDeps[parameterName]] != parameterInfo.ParameterType)
                    throw new InvalidOperationException($"The default constructor for the " +
                        $"behavior of type {behaviorName} is not valid for its dependencies.");
            }
        }
    }

    /// <summary>
    /// Validates the provided on change declaration for the specified operation and behavior type.
    /// </summary>
    /// <param name="behaviorType">The behavior type whose on change operation 
    /// is being validated.</param>
    /// <param name="operation">The operation whose on change declaration 
    /// is being validated.</param>
    /// <param name="attribute">The on change declaration being validated.</param>
    /// <exception cref="InvalidOperationException">Thrown if the declaration is 
    /// invalid with respect to its operation and behavior type.</exception>
    private void ValidateOnChangeOperation(Type behaviorType, MethodInfo operation,
        OnChangeAttribute attribute)
    {
        string dName = attribute.DependencyName;

        int dependencyIndex;
        if (_behaviorSelfCreatedDependencies[behaviorType].ContainsKey(dName))
            dependencyIndex = _behaviorSelfCreatedDependencies[behaviorType][dName];
        else if (_behaviorExistingDependencies[behaviorType].ContainsKey(dName))
            dependencyIndex = _behaviorExistingDependencies[behaviorType][dName];
        else
            throw new InvalidOperationException($"The on change declaration of " +
                $"the operation {operation.Name} of the behavior type {behaviorType.FullName} " +
                $"has an invalid dependency name, {dName}. All dependency names must match the " +
                $"name of the behavior dependency that is relevant to the operation.");

        string? csName = attribute.ContextStateName;
        if (csName == null)
            return;

        Type dependencyType = _behaviorDependencies[behaviorType][dependencyIndex];
        if (!_contextBindableProperties[dependencyType].Select(p => p.Name).Contains(csName))
            throw new InvalidOperationException($"The on change declaration of " +
                $"the operation {operation.Name} of the behavior type {behaviorType.FullName} " +
                $"has an invalid context state name, {csName}. All context state names must " +
                $"match a non-readonly context state property name of the relevant context, " +
                $"or be null if there is no specific relevant state.");
    }

    /// <summary>
    /// Validates the provided operation of the specific behavior type.
    /// </summary>
    /// <param name="behaviorType">The behavior type whose operation is being validated.</param>
    /// <param name="operation">The operation being validated.</param>
    /// <exception cref="InvalidOperationException">Thrown if the operation is 
    /// invalid with respect to the behavior type.</exception>
    private void ValidateOperation(Type behaviorType, MethodInfo operation)
    {
        ParameterInfo[] parameters = operation.GetParameters();
        for (int c = 0, count = parameters.Length; c < count; c++)
        {
            string pName = parameters[c].Name.EnsureNotNull();
            Type pType = parameters[c].ParameterType;

            int dependencyIndex;
            if (_behaviorSelfCreatedDependencies[behaviorType].ContainsKey(pName))
                dependencyIndex = _behaviorSelfCreatedDependencies[behaviorType][pName];
            else if (_behaviorExistingDependencies[behaviorType].ContainsKey(pName))
                dependencyIndex = _behaviorExistingDependencies[behaviorType][pName];
            else
                throw new InvalidOperationException($"The operation {operation.Name} " +
                    $"of the behavior type {behaviorType.FullName} has an invalid " +
                    $"parameter name, {pName}. All parameter names must match the name " +
                    $"of the dependency expected to be provided to the operation.");

            if (pType != _behaviorDependencies[behaviorType][dependencyIndex])
                throw new InvalidOperationException($"The operation {operation.Name} " +
                    $"of the behavior type {behaviorType.FullName} has an invalid " +
                    $"type for parameter {pName}. Expected " +
                    $"{_behaviorDependencies[behaviorType][dependencyIndex].FullName} but " +
                    $"was {pType.FullName}.");
        }
    }
}
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
            .Where(a => a.GetReferencedAssemblies().Select(name => name.FullName)
            .Contains(executingAssemblyName));

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
    /// Specifies whether the provided type is a type of context known by this evaluator.
    /// </summary>
    /// <param name="type">The type to be checked.</param>
    /// <returns>Whether the provided type is a type of context.</returns>
    public abstract bool IsContextType(Type type);
}

/// <inheritdoc/>
/// <typeparam name="TContextAttribute">The type of attribute that defines a context.</typeparam>
/// <typeparam name="TBehaviorAttribute">The type of attribute that defines a behavior.</typeparam>
/// <typeparam name="TDependencyAttribute">The base type of attribute that defines 
/// a dependency of a behavior.</typeparam>
public class Evaluator<TContextAttribute, TBehaviorAttribute,
    TDependencyAttribute, TOperationAttribute> : Evaluator
    where TContextAttribute : BaseContextAttribute
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

    /// <summary>
    /// All found operations that should be invoked upon a context change.
    /// </summary>
    private readonly Dictionary<Type, Dictionary<string, Dictionary<string, List<MethodInfo>>>>
        _onChangeOperations = new();


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

        CacheInitializationBehaviorTypes();
    }

    /// <inheritdoc/>
    protected override void InitializeContextTypes(IEnumerable<Assembly> assembliesToEvaluate)
    {
        foreach (Assembly assembly in assembliesToEvaluate)
        {
            Type[] types = assembly.GetTypes();
            for (int c = 0, count = types.Length; c < count; c++)
            {
                CacheContextType(types[c]);
                CacheBindableStateInfos(types[c]);
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
        ValidateConstructor(behaviorType.FullName, depTypes, selfCreatedDeps, constructor);

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
        IEnumerable<TDependencyAttribute> attrs =
            behaviorType.GetCustomAttributes<TDependencyAttribute>(true);

        Type[] depTypes = new Type[attrs.Count()];
        Dictionary<string, int> selfCreatedDeps = new();

        int depIndex = 0;
        foreach (TDependencyAttribute attr in attrs)
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
            if (methods[c].GetCustomAttribute<TOperationAttribute>(true) != null)
            {
                CacheOnChangeOperations(behaviorType, methods[c]);
                // TODO :: Support other types of operation caching here.
            }
    }

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
    /// Ensures the validity of a behavior's constructor.
    /// </summary>
    /// <param name="behaviorName">The name of the behavior whose constructor 
    /// is to be validated.</param>
    /// <param name="depTypes">The types of the behavior's dependencies.</param>
    /// <param name="selfCreatedDeps">The names and indices of the dependencies 
    /// that the behavior will create when it is instantiated.</param>
    /// <param name="constructor">The constructor of the behavior to be validated.</param>
    /// <exception cref="InvalidOperationException"></exception>
    private static void ValidateConstructor(string? behaviorName, Type[] depTypes,
        Dictionary<string, int> selfCreatedDeps, ConstructorInfo constructor)
    {
        ParameterInfo[] parameters = constructor.GetParameters();
        if (parameters.Length != depTypes.Length)
            throw new InvalidOperationException($"The default constructor for the behavior " +
                $"of type {behaviorName} is not valid for its dependencies.");

        for (int pc = 0, pCount = parameters.Length; pc < pCount; pc++)
        {
            ParameterInfo parameterInfo = parameters[pc];
            string? parameterName = parameterInfo.Name;

            if (parameterName == null)
                throw new InvalidOperationException($"The default constructor for the behavior " +
                    $"of type {behaviorName} is not valid for its dependencies.");

            if (!selfCreatedDeps.ContainsKey(parameterName))
                throw new InvalidOperationException($"The default constructor for the behavior " +
                    $"of type {behaviorName} is not valid for its dependencies.");

            if (!parameterInfo.IsOut)
                throw new InvalidOperationException($"The default constructor for the behavior " +
                    $"of type {behaviorName} is not valid for its dependencies.");

            Type refDepType = depTypes[selfCreatedDeps[parameterName]].MakeByRefType();
            if (refDepType != parameterInfo.ParameterType)
                throw new InvalidOperationException($"The default constructor for the behavior " +
                    $"of type {behaviorName} is not valid for its dependencies.");
        }
    }

    private static void ValidateOnChangeOperation(Type behaviorType, MethodInfo operation,
        OnChangeAttribute attribute)
    {

    }
}
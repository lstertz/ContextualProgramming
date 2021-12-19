using System.Reflection;

namespace ContextualProgramming;

/// <summary>
/// Handles the state and behavior of the running application by managing 
/// registered contexts (<see cref="ContextAttribute"/>) and the resulting behaviors 
/// (<see cref="BehaviorAttribute"/>).
/// </summary>
public static class App
{
    /// <summary>
    /// The types of all dependencies of all behavior types.
    /// </summary>
    private static readonly Dictionary<Type, Type[]> BehDepTypes = new();

    /// <summary>
    /// The names and their dependency indices for the self-created 
    /// dependencies of all behavior types.
    /// </summary>
    private static readonly Dictionary<Type, Dictionary<string, int>> BehSelfCreatedDeps = new();

    /// <summary>
    /// Details of each registered context type.
    /// </summary>
    private static readonly HashSet<Type> ContextInfos = new();

    /// <summary>
    /// All currently contextualized context instances, keyed by their class type.
    /// </summary>
    private static readonly Dictionary<Type, HashSet<object>> Contexts = new();

    /// <summary>
    /// A mapping of context instances to the behavior instances that created them, 
    /// as self-created dependencies.
    /// </summary>
    private static readonly Dictionary<object, object> ContextBehaviors = new();

    /// <summary>
    /// A mapping of context types to the behavior types that depend upon them.
    /// </summary>
    private static readonly Dictionary<Type, List<Type>> ContextDependents = new();




    /// <summary>
    /// Initializes the contextual execution of the application by registering all 
    /// declared contexts (<see cref="ContextAttribute"/>) and all 
    /// declared Behaviors (<see cref="BehaviorAttribute"/>).
    /// </summary>
    /// <remarks>
    /// Behaviors without any dependencies will be instantiated.
    /// </remarks>
    public static void Initialize()
    {
        List<Type> contexts = new();
        List<Type> behaviors = new();

        Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
        for (int c = 0, count = assemblies.Length; c < count; c++)
        {
            Type[] types = assemblies[c].GetTypes();
            for (int cc = 0, cCount = types.Length; cc < cCount; cc++)
                if (types[cc].GetCustomAttribute<ContextAttribute>(true) != null)
                    contexts.Add(types[cc]);
                else if (types[cc].GetCustomAttribute<BehaviorAttribute>(true) != null)
                    behaviors.Add(types[cc]);
        }

        for (int c = 0, count = contexts.Count; c < count; c++)
            RegisterContext(contexts[c]);

        for (int c = 0, count = behaviors.Count; c < count; c++)
            RegisterBehavior(behaviors[c]);
    }


    /// <summary>
    /// Provides the first found context of the specified type.
    /// </summary>
    /// <typeparam name="T">The type of context to be retrieved.</typeparam>
    /// <returns>The first found context of the specified type, 
    /// or null if there is no such context.</returns>
    public static T? GetContext<T>() where T : class
    {
        if (Contexts.ContainsKey(typeof(T)))
            return Contexts[typeof(T)].FirstOrDefault() as T;
        
        return null;
    }

    /// <summary>
    /// Provides all contexts of the specified type.
    /// </summary>
    /// <typeparam name="T">The type of contexts to be retrieved.</typeparam>
    /// <returns>All contexts of the specified type, or an empty array if 
    /// there are no such contexts.</returns>
    public static T[] GetContexts<T>() where T : class
    {
        if (Contexts.ContainsKey(typeof(T)))
            return Contexts[typeof(T)].Cast<T>().ToArray();

        return Array.Empty<T>();
    }


    /// <summary>
    /// Contextualizes a new context, including it as part of the app's contextual state.
    /// </summary>
    /// <typeparam name="T">The type of the context.</typeparam>
    /// <param name="context">The context to be registered.</param>
    /// <exception cref="ArgumentNullException">Thrown if the provided context 
    /// instance is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown if the provided instance is 
    /// not a context instance.</exception>
    public static void Contextualize<T>(T context) where T : class
    {
        if (context == null)
            throw new ArgumentNullException(nameof(context));

        Type type = typeof(T);
        if (!ContextInfos.Contains(type))
            throw new InvalidOperationException($"The provided instance, {context}, " +
                $"of type {type.FullName} cannot be contextualized as it is not a Context.");

        if (!Contexts.ContainsKey(type))
            Contexts.Add(type, new());
        Contexts[type].Add(context);

        // TODO :: Fulfill behavior dependencies when possible.
    }

    /// <inheritdoc cref="Contextualize{T}(T)"/>
    private static void Contextualize(object context)
    {
        if (context == null)
            throw new ArgumentNullException(nameof(context));

        Type type = context.GetType();
        if (!ContextInfos.Contains(type))
            throw new InvalidOperationException($"The provided instance, {context}, " +
                $"of type {type.FullName} cannot be contextualized as it is not a Context.");

        if (!Contexts.ContainsKey(type))
            Contexts.Add(type, new());
        Contexts[type].Add(context);

        // TODO :: Fulfill behavior dependencies when possible.
    }

    /// <summary>
    /// Decontextualizes the provided context, removing it from the app's contextual state.
    /// </summary>
    /// <typeparam name="T">The type of the context.</typeparam>
    /// <param name="context">The context to be deregistered.</param>
    public static void Decontextualize<T>(T context)
    {
        if (context == null)
            throw new ArgumentNullException(nameof(context));

        if (!Contexts.ContainsKey(typeof(T)))
            return;

        Contexts[typeof(T)].Remove(context);
        ContextBehaviors.Remove(context);
    }


    /// <summary>
    /// Registers a type as a behavior type with its dependency information.
    /// </summary>
    /// <param name="type">The type to be registered as a behavior.</param>
    /// <exception cref="InvalidOperationException">Thrown if a dependency of 
    /// the behavior is not a context.</exception>
    private static void RegisterBehavior(Type type)
    {
        IEnumerable<DependencyAttribute> attrs =
            type.GetCustomAttributes<DependencyAttribute>(true);
        Type[] depTypes = new Type[attrs.Count()];
        Dictionary<string, int> selfCreatedDeps = new();

        int depIndex = 0;
        foreach (DependencyAttribute attr in attrs)
        {
            Type t = attr.Type;
            if (!ContextInfos.Contains(t))
                throw new InvalidOperationException($"The dependency named {attr.Name} of type" +
                    $"{t} for the behavior of type {type.FullName} is not defined as a context.");

            depTypes[depIndex] = t;
            switch (attr.Fulfillment)
            {
                case Fulfillment.SelfCreated:
                    selfCreatedDeps.Add(attr.Name, depIndex);
                    break;
                default:
                    break;
            }

            List<Type> dependents = ContextDependents.GetValueOrDefault(t, new());
            if (!dependents.Contains(type))
                dependents.Add(type);

            depIndex++;
        }

        BehDepTypes.Add(type, depTypes);
        BehSelfCreatedDeps.Add(type, selfCreatedDeps);

        // Only self-created dependencies are currently supported, so the Behavior can be created.
        ConstructorInfo[] constructors = type.GetConstructors(
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | 
            BindingFlags.FlattenHierarchy);
        if (constructors.Length == 0)
            throw new InvalidOperationException($"Behavior of type {type.FullName} does not " +
                $"have a valid constructor.");

        ConstructorInfo constructor = constructors[0]; // Assume one constructor for now.
        ParameterInfo[] parameters = constructor.GetParameters();
        if (parameters.Length != depTypes.Length)
            throw new InvalidOperationException($"The default constructor for the behavior " +
                $"of type {type.FullName} is not valid for its dependencies.");

        Type[] orderedDepTypes = new Type[depTypes.Length];
        for (int pc = 0, pCount = parameters.Length; pc < pCount; pc++)
        {
            ParameterInfo parameterInfo = parameters[pc];
            string? parameterName = parameterInfo.Name;

            if (parameterName == null)
                throw new InvalidOperationException($"The default constructor for the behavior " +
                    $"of type {type.FullName} is not valid for its dependencies.");

            if (!selfCreatedDeps.ContainsKey(parameterName))
                throw new InvalidOperationException($"The default constructor for the behavior " +
                    $"of type {type.FullName} is not valid for its dependencies.");

            if (depTypes[selfCreatedDeps[parameterName]].Equals(parameterInfo.ParameterType))
                throw new InvalidOperationException($"The default constructor for the behavior " +
                    $"of type {type.FullName} is not valid for its dependencies.");

            if (!parameterInfo.IsOut)
                throw new InvalidOperationException($"The default constructor for the behavior " +
                    $"of type {type.FullName} is not valid for its dependencies.");

            orderedDepTypes[pc] = parameterInfo.ParameterType;
        }

        object[] arguments = new object[depTypes.Length];
        object behavior = constructor.Invoke(arguments);

        for (int c = 0, count = arguments.Length; c < count; c++)
        {
            try
            {
                Contextualize(arguments[c]);
            }
            catch (ArgumentNullException)
            {
                throw new InvalidOperationException($"The default constructor for the behavior " +
                    $"of type {type.FullName} did not construct an expected dependency of " +
                    $"type {orderedDepTypes[c]}.");
            }

            ContextBehaviors.Add(arguments[c], behavior);
        }
    }

    /// <summary>
    /// Registers a type as a context with its relevant details.
    /// </summary>
    /// <param name="type">The type to be registered as a context.</param>
    private static void RegisterContext(Type type)
    {
        ContextInfos.Add(type);
        // TODO :: Collect relevant details from the context, likely context value information.
    }
}
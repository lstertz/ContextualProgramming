using System.Reflection;

namespace ContextualProgramming;

/// <summary>
/// Handles the state and behavior of the running application by managing 
/// registered Contexts (<see cref="ContextAttribute"/>) and the resulting behaviors 
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
    /// A mapping of contexts instances to the behavior instances that created them, 
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
    /// Contextualizes a new context, including it as part of the app's contextual state.
    /// </summary>
    /// <typeparam name="T">The type of the context.</typeparam>
    /// <param name="context">The context to be registered.</param>
    /// <exception cref="ArgumentNullException">Thrown if the provided context 
    /// instance is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown if the provided instance is 
    /// not a context instance.</exception>
    public static void Contextualize<T>(T context)
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
    }

    /// <summary>
    /// Registers a type as a context with its relevant details.
    /// </summary>
    /// <param name="type">The type to be registered as a context.</param>
    private static void RegisterContext(Type type)
    {
        ContextInfos.Add(type);
        // TODO :: Collect relevant details from the context, likely property info.
    }
}
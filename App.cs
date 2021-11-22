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
    /// Details of each registered context type.
    /// </summary>
    private static readonly HashSet<Type> ContextInfos = new();

    /// <summary>
    /// All currently contextualized context instances, keyed by their class type.
    /// </summary>
    private static readonly Dictionary<Type, HashSet<object>> Contexts = new();

    /// <summary>
    /// A mapping of contexts instances to the behavior instances that created them, 
    /// as self-fulfilled dependencies.
    /// </summary>
    private static readonly Dictionary<object, object> ContextBehaviors = new();

    /// <summary>
    /// A mapping of context types to the behavior types that depend upon them.
    /// </summary>
    private static readonly Dictionary<Type, List<Type>> ContextDependents = new();

    private static readonly Dictionary<Type, PropertyInfo[]> BehaviorSelfDepProperties = new();
    private static readonly Dictionary<Type, Type[]> BehaviorSharedDependencies = new();
    private static readonly Dictionary<Type, Type[]> BehaviorUniqueDependencies = new();


    /// <summary>
    /// Initializes the contextual execution of the application by registering all 
    /// declared bontexts (<see cref="ContextAttribute"/>) and all 
    /// declared Behaviors (<see cref="BehaviorAttribute"/>).
    /// </summary>
    /// <remarks>
    /// Behaviors without any dependencies will be instantiated.
    /// </remarks>
    public static void Initialize()
    {
        List<PropertyInfo> selfDependencyProperties = new();
        List<Type> sharedDependencies = new();
        List<Type> uniqueDependencies = new();

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
            RegisterBehavior(behaviors[c], ref selfDependencyProperties, 
                ref sharedDependencies, ref uniqueDependencies);
    }


    // Future reference, for creating instances with dependencies:
    // object o = FormatterServices.GetUninitializedObject(types[c]);
    // Set properties using o as instance.
    // Run o's constructor through constructor info.


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
    /// <param name="selfDependencyProperties">A list of property info for holding the behavior's 
    /// self-fulfilled dependency property information.</param>
    /// <param name="sharedDependencies">A list of the context types that this behavior 
    /// has as shared dependencies.</param>
    /// <param name="uniqueDependencies">A list of the context types that this behavior 
    /// has as unique dependencies.</param>
    /// <exception cref="InvalidOperationException">Thrown if the behavior 
    /// does not have a parameterless constructor or declares dependencies 
    /// that aren't contexts.</exception>
    private static void RegisterBehavior(Type type,
        ref List<PropertyInfo> selfDependencyProperties,
        ref List<Type> sharedDependencies,
        ref List<Type> uniqueDependencies)
    {
        static void RegisterDependent(Type context, Type dependentBehavior)
        {
            if (!ContextDependents.ContainsKey(context))
                ContextDependents.Add(context, new());

            ContextDependents[context].Add(dependentBehavior);
        }

        selfDependencyProperties.Clear();
        sharedDependencies.Clear();
        uniqueDependencies.Clear();

        PropertyInfo[] properties = type.GetProperties(BindingFlags.Instance |
            BindingFlags.Public | BindingFlags.NonPublic);
        for (int p = 0, pCount = properties.Length; p < pCount; p++)
        {
            PropertyInfo property = properties[p];
            DependencyAttribute? depAttr = property
                .GetCustomAttribute<DependencyAttribute>(true);
            if (depAttr == null)
                continue;

            Type pType = properties[p].PropertyType;
            switch (depAttr.Source)
            {
                case DependencySource.Self:
                    selfDependencyProperties.Add(property);
                    break;
                case DependencySource.Shared:
                    sharedDependencies.Add(pType);
                    break;
                case DependencySource.Unique:
                    uniqueDependencies.Add(pType);
                    break;
                default:
                    break;
            }
        }

        if (selfDependencyProperties.Count > 0)
            BehaviorSelfDepProperties.Add(type, selfDependencyProperties.ToArray());
        if (sharedDependencies.Count > 0)
        {
            BehaviorSharedDependencies.Add(type, sharedDependencies.ToArray());
            for (int sd = 0, sdCount = sharedDependencies.Count; sd < sdCount; sd++)
                RegisterDependent(sharedDependencies[sd], type);
        }
        if (uniqueDependencies.Count > 0)
        {
            BehaviorUniqueDependencies.Add(type, uniqueDependencies.ToArray());
            for (int ud = 0, udCount = uniqueDependencies.Count; ud < udCount; ud++)
                RegisterDependent(uniqueDependencies[ud], type);
        }

        if (sharedDependencies.Count > 0 || uniqueDependencies.Count > 0)
            return;  // TODO :: Check to see if the required dependencies exist.

        object? behavior = Activator.CreateInstance(type, true);
        if (behavior == null)
            throw new InvalidOperationException($"The Behavior {type.FullName} " +
                $"could not be constructed as there was no parameterless constructor, " +
                $"as is expected of a Behavior.");

        for (int sdi = 0, sdiCount = selfDependencyProperties.Count; sdi < sdiCount; sdi++)
        {
            object? context = selfDependencyProperties[sdi].GetValue(behavior);
            if (context == null)
                throw new InvalidOperationException($"The property " +
                    $"{selfDependencyProperties[sdi].Name} of the Behavior {type.FullName} " +
                    $"was not assigned a Context after construction, " +
                    $"as is expected of a self-fulfilled dependency.");

            ContextBehaviors.Add(context, behavior);
        }
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
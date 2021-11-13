using System.Reflection;

namespace ContextualProgramming
{
    /// <summary>
    /// Handles the state and behavior of the running application by managing 
    /// registered <see cref="Context"/>s and the resulting behaviors 
    /// (<see cref="BehaviorAttribute"/>).
    /// </summary>
    public static class App
    {
        private static readonly Dictionary<Type, HashSet<Context>> Contexts = new();
        private static readonly Dictionary<Context, object> ContextBehaviors = new();
        private static readonly Dictionary<Type, List<Type>> ContextDependents = new();

        private static readonly Dictionary<Type, PropertyInfo[]> BehaviorSelfDepProperties = new();
        private static readonly Dictionary<Type, Type[]> BehaviorSharedDependencies = new();
        private static readonly Dictionary<Type, Type[]> BehaviorUniqueDependencies = new();


        /// <summary>
        /// Initializes the contextual execution of the application by registering all 
        /// declared behaviors (<see cref="BehaviorAttribute"/>).
        /// </summary>
        /// <remarks>
        /// Behaviors without any dependencies will be instantiated.
        /// </remarks>
        public static void Initialize()
        {
            static void RegisterDependent(Type context, Type dependentBehavior)
            {
                if (!ContextDependents.ContainsKey(context))
                    ContextDependents.Add(context, new());

                ContextDependents[context].Add(dependentBehavior);
            }

            List<PropertyInfo> selfDependencyProperties = new();
            List<Type> sharedDependencies = new();
            List<Type> uniqueDependencies = new();

            Type[] types = Assembly.GetExecutingAssembly().GetTypes();
            for (int c = 0, count = types.Length; c < count; c++)
            {
                if (types[c].GetCustomAttribute<BehaviorAttribute>(true) == null)
                    continue;

                Type bType = types[c];
                selfDependencyProperties.Clear();
                sharedDependencies.Clear();
                uniqueDependencies.Clear();

                PropertyInfo[] properties = types[c].GetProperties(BindingFlags.Instance | 
                    BindingFlags.Public | BindingFlags.NonPublic);
                for (int p = 0, pCount = properties.Length; p <pCount; p++)
                {
                    PropertyInfo property = properties[p];
                    DependencyAttribute depAttr = property
                        .GetCustomAttribute<DependencyAttribute>(true);
                    if (depAttr == null)
                        continue;

                    Type pType = properties[p].PropertyType;
                    if (!pType.IsInterface && !pType.IsAssignableTo(typeof(Context)))
                        throw new InvalidOperationException($"The property {property.Name} " +
                            $"with the type {pType.FullName}, which is neither an interface nor " +
                            $"a Context, has been marked as a dependency of the Behavior " +
                            $"{bType.FullName}.");

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
                    BehaviorSelfDepProperties.Add(bType, selfDependencyProperties.ToArray());
                if (sharedDependencies.Count > 0)
                {
                    BehaviorSharedDependencies.Add(bType, sharedDependencies.ToArray());
                    for (int sd = 0, sdCount = sharedDependencies.Count; sd < sdCount; sd++)
                        RegisterDependent(sharedDependencies[sd], bType);
                }
                if (uniqueDependencies.Count > 0)
                {
                    BehaviorUniqueDependencies.Add(bType, uniqueDependencies.ToArray());
                    for (int ud = 0, udCount = uniqueDependencies.Count; ud < udCount; ud++)
                        RegisterDependent(uniqueDependencies[ud], bType);
                }

                if (sharedDependencies.Count > 0 || uniqueDependencies.Count > 0)
                    continue;  // TODO :: Check to see if the required dependencies exist.

                object behavior = Activator.CreateInstance(bType, true);

                for (int sdi = 0, sdiCount = selfDependencyProperties.Count; sdi < sdiCount; sdi++)
                {
                    Context context = selfDependencyProperties[sdi].GetValue(behavior) as Context;
                    if (context == null)
                        throw new InvalidOperationException($"The property " +
                            $"{selfDependencyProperties[sdi].Name} of the Behavior {bType.FullName} " +
                            $"was not assigned a Context after construction, " +
                            $"as is expected of a self-fulfilled dependency.");

                    ContextBehaviors.Add(context, behavior);
                }
            }
        }

        // Future reference, for creating instances with dependencies:
        // object o = FormatterServices.GetUninitializedObject(types[c]);
        // Set properties using o as instance.
        // Run o's constructor through constructor info.

        /// <summary>
        /// Registers a new context for the execution context of the application.
        /// </summary>
        /// <param name="context">The context to be registered.</param>
        public static void Register(Context context)
        {
            Type type = context.GetType();

            Type[] interfaces = type.GetInterfaces();
            for (int c = 0, count = interfaces.Length; c < count; c++)
                Register(type, context);

            while (!type.IsAbstract && type != typeof(Context))
            {
                Register(type, context);
                type = type.BaseType;
            }
        }


        /// <summary>
        /// Registers a context of the specified type for the execution context of the application.
        /// </summary>
        /// <param name="type">The type identifying the context.</param>
        /// <param name="context">The context to be registered.</param>
        private static void Register(Type type, Context context)
        {
            if (!Contexts.ContainsKey(type))
                Contexts.Add(type, new());

            Contexts[type].Add(context);

            // TODO :: Fulfill behavior dependencies when possible.
        }
    }
}

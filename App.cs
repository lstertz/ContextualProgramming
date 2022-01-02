using ContextualProgramming.Internal;
using System.Reflection;

namespace ContextualProgramming;

/// <summary>
/// Handles the state and behavior of the running application by managing 
/// registered contexts and the resulting behaviors within an encapsulated environment.
/// </summary>
public class App
{
    /// <summary>
    /// Encapsulates an instance of a behavior with its contexts.
    /// </summary>
    private class BehaviorInstance
    {
        /// <summary>
        /// The encapsulated behavior instance.
        /// </summary>
        public object Behavior { get; set; }

        /// <summary>
        /// The contexts (dependencies) of the encapsulated behavior keyed by to their names.
        /// </summary>
        public Dictionary<string, object> Contexts { get; set; }

        /// <summary>
        /// The contexts (dependencies) of the encapsulated behavior mapped to their names.
        /// </summary>
        public Dictionary<object, string> ContextNames { get; set; }


        /// <summary>
        /// Constructs a new behavior instance to link an instance 
        /// of a behavior with its contexts.
        /// </summary>
        /// <param name="behavior"><see cref="Behavior"/></param>
        /// <param name="contexts"><see cref="Contexts"/></param>
        public BehaviorInstance(object behavior, Dictionary<string, object> contexts) =>
            (Behavior, Contexts, ContextNames) =
            (behavior.EnsureNonNullable(), contexts, contexts.Flip());
    }

    /// <summary>
    /// Represents a record of a change for a property of a context.
    /// </summary>
    private class ContextChange
    {
        /// <summary>
        /// The context that was changed.
        /// </summary>
        public object Context { get; set; }

        /// <summary>
        /// The name of the property that was changed.
        /// </summary>
        public string PropertyName { get; set; }


        /// <summary>
        /// Constructs a new context change to hold details of a changed context.
        /// </summary>
        /// <param name="context"><see cref="Context"/></param>
        /// <param name="propertyName"><see cref="PropertyName"/></param>
        public ContextChange(object context, string propertyName) => (Context, PropertyName) =
            (context.EnsureNonNullable(), propertyName.EnsureNonNullable());
    }


    /// <summary>
    /// The evaluator that finds and validates the contexts and behaviors of this environment.
    /// </summary>
    public Evaluator Evaluator { get; private set; }


    /// <summary>
    /// All currently contextualized context instances, keyed by their class type.
    /// </summary>
    private readonly Dictionary<Type, HashSet<object>> _contexts = new();

    /// <summary>
    /// A mapping of context instances to the behavior instances that created them 
    /// as self-created dependencies.
    /// </summary>
    private readonly Dictionary<object, BehaviorInstance> _contextBehaviors = new();

    /// <summary>
    /// A record of context changes that have occurred since the last evaluation.
    /// </summary>
    private readonly List<ContextChange> _contextChanges = new();

    private bool _isInitialized = false;


    /// <summary>
    /// Constructs a new app with the default evaluator, 
    /// <see cref="Evaluator{TContextAttribute, TBehaviorAttribute, TBaseDependencyAttribute}"/>.
    /// </summary>
    public App()
    {
        Evaluator = new Evaluator<ContextAttribute, BehaviorAttribute,
            DependencyAttribute, OperationAttribute>();
    }

    /// <summary>
    /// Constructs a new app with the provided evaluator.
    /// </summary>
    /// <param name="evaluator">The evaluator to be used by the new app.</param>
    public App(Evaluator evaluator)
    {
        Evaluator = evaluator;
    }

    /// <summary>
    /// Initializes the contextual execution within the environment by registering all 
    /// contexts and behaviors found by the default <see cref="Evaluator"/>.
    /// </summary>
    /// <remarks>
    /// Behaviors without any dependencies will be instantiated.
    /// </remarks>
    public void Initialize()
    {
        Evaluator.Initialize();

        Type[] initializationBehaviors = Evaluator.GetInitializationBehaviorTypes();
        for (int c = 0, count = initializationBehaviors.Length; c < count; c++)
            InstantiateBehavior(initializationBehaviors[c]);

        _isInitialized = true;
    }

    /// <summary>
    /// Validates that the app has been initialized.
    /// </summary>
    /// <exception cref="InvalidOperationException">The exception thrown if the 
    /// app has not been initialized.</exception>
    private void ValidateInitialization()
    {
        if (!_isInitialized)
            throw new InvalidOperationException($"This app has not yet been initialized. " +
                $"Initialize with {nameof(App.Initialize)} prior to using the app.");
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
    public void Contextualize<T>(T context) where T : class
    {
        ValidateInitialization();

        Contextualize(context as object);
    }

    /// <inheritdoc cref="Contextualize{T}(T)"/>
    private void Contextualize(object context)
    {
        if (context == null)
            throw new ArgumentNullException(nameof(context));

        Type type = context.GetType();
        if (!Evaluator.IsContextType(type))
            throw new InvalidOperationException($"The provided instance, {context}, " +
                $"of type {type.FullName} cannot be contextualized as it is not a Context.");

        if (!_contexts.ContainsKey(type))
            _contexts.Add(type, new());
        _contexts[type].Add(context);

        PropertyInfo[] bindableProperties = Evaluator.GetBindableStateInfos(type);
        for (int c = 0, count = bindableProperties.Length; c < count; c++)
        {
            int contextIndex = c;
            (bindableProperties[c].GetValue(context) as IBindableState)?.Bind(() =>
                _contextChanges.Add(new(context, bindableProperties[contextIndex].Name)));
        }

        // TODO :: Fulfill behavior dependencies when possible.
    }

    /// <summary>
    /// Decontextualizes the provided context, removing it from the app's contextual state.
    /// </summary>
    /// <typeparam name="T">The type of the context.</typeparam>
    /// <param name="context">The context to be deregistered.</param>
    public void Decontextualize<T>(T context)
    {
        ValidateInitialization();

        if (context == null)
            throw new ArgumentNullException(nameof(context));

        if (!_contexts.ContainsKey(typeof(T)))
            return;

        PropertyInfo[] bindableProperties = Evaluator.GetBindableStateInfos(context.GetType());
        for (int c = 0, count = bindableProperties.Length; c < count; c++)
            (bindableProperties[c].GetValue(context) as IBindableState)?.Unbind();

        _contexts[typeof(T)].Remove(context);
        if (_contexts[typeof(T)].Count == 0)
            _contexts.Remove(typeof(T));

        if (_contextBehaviors.ContainsKey(context))
        {
            BehaviorInstance behaviorInstance = _contextBehaviors[context];
            foreach (object c in behaviorInstance.Contexts.Values)
                if (_contextBehaviors.ContainsKey(c) && _contextBehaviors[c] == behaviorInstance)
                    _contextBehaviors.Remove(c);
        }
    }

    /// <summary>
    /// Provides the first found context of the specified type.
    /// </summary>
    /// <typeparam name="T">The type of context to be retrieved.</typeparam>
    /// <returns>The first found context of the specified type, 
    /// or null if there is no such context.</returns>
    public T? GetContext<T>() where T : class
    {
        ValidateInitialization();

        if (!Evaluator.IsContextType(typeof(T)))
            throw new InvalidOperationException($"Type {typeof(T).FullName} is not a context.");

        if (_contexts.ContainsKey(typeof(T)))
            return _contexts[typeof(T)].First() as T;

        return null;
    }

    /// <summary>
    /// Provides all contexts of the specified type.
    /// </summary>
    /// <typeparam name="T">The type of contexts to be retrieved.</typeparam>
    /// <returns>All contexts of the specified type, or an empty array if 
    /// there are no such contexts.</returns>
    public T[] GetContexts<T>() where T : class
    {
        ValidateInitialization();

        if (!Evaluator.IsContextType(typeof(T)))
            throw new InvalidOperationException($"Type {typeof(T).FullName} is not a context.");

        if (_contexts.ContainsKey(typeof(T)))
            return _contexts[typeof(T)].Cast<T>().ToArray();

        return Array.Empty<T>();
    }

    /// <summary>
    /// Evaluates any changes to the contextual state of the app since the last update 
    /// and invokes any appropriate operations to respond to those changes.
    /// </summary>
    /// <returns>True if there were changes evaluated, false otherwise.</returns>
    public bool Update()
    {
        if (_contextChanges.Count == 0)
            return false;

        ContextChange[] contextChanges = _contextChanges.ToArray();
        _contextChanges.Clear();

        for (int c = 0, count = contextChanges.Length; c < count; c++)
        {
            ContextChange change = contextChanges[c];
            object context = change.Context;
            if (!_contextBehaviors.ContainsKey(context))
                continue;

            BehaviorInstance bInstance = _contextBehaviors[context];
            Type behaviorType = bInstance.Behavior.GetType();
            string contextName = bInstance.ContextNames[context];

            MethodInfo[] contextOperations = Evaluator.GetOnChangeOperations(
                behaviorType, contextName);
            for (int co = 0, coCount = contextOperations.Length; co < coCount; co++)
                InvokeOperation(bInstance, contextOperations[co]);

            MethodInfo[] stateOperations = Evaluator.GetOnChangeOperations(
                behaviorType, contextName, change.PropertyName);
            for (int so = 0, soCount = stateOperations.Length; so < soCount; so++)
                InvokeOperation(bInstance, stateOperations[so]);
        }

        return true;
    }

    /// <summary>
    /// Invokes the provided operation for the provided behavior instance.
    /// </summary>
    /// <param name="bInstance">The behavior instance for which the 
    /// operation is being invoked.</param>
    /// <param name="operation">The operation to invoke.</param>
    private static void InvokeOperation(BehaviorInstance bInstance, MethodInfo operation)
    {
        ParameterInfo[] parameters = operation.GetParameters();
        object[] arguments = new object[parameters.Length];

        for (int pco = 0, pcoCount = parameters.Length; pco < pcoCount; pco++)
            arguments[pco] = bInstance.Contexts[parameters[pco].Name.EnsureNonNullable()];

        operation.Invoke(bInstance.Behavior, arguments);
    }


    /// <summary>
    /// Instantiates an instance of the behavior specified by the provided type.
    /// </summary>
    /// <param name="behaviorType">The type of behavior to be instantiated.</param>
    /// <exception cref="InvalidOperationException">Thrown if the behavior does not 
    /// construct and expected self-created dependency during its instantiation.</exception>
    private void InstantiateBehavior(Type behaviorType)
    {
        ConstructorInfo constructor = Evaluator.GetBehaviorConstructor(behaviorType);
        ParameterInfo[] parameters = constructor.GetParameters();


        object[] arguments = new object[parameters.Length];
        object behavior = constructor.Invoke(arguments);

        Dictionary<string, object> contexts = new();
        for (int c = 0, count = parameters.Length; c < count; c++)
        {
            try
            {
                contexts.Add(parameters[c].Name.EnsureNonNullable(),
                    arguments[c].EnsureNonNullable());
            }
            catch (ArgumentNullException)
            {
                throw new InvalidOperationException($"The default constructor for the behavior " +
                    $"of type {behaviorType.FullName} did not construct an expected dependency " +
                    $"of type {parameters[c].ParameterType.FullName}.");
            }
        }

        BehaviorInstance behaviorInstance = new(behavior, contexts);
        for (int c = 0, count = arguments.Length; c < count; c++)
        {
            Contextualize(arguments[c]);
            _contextBehaviors.Add(arguments[c], behaviorInstance);
        }
    }
}
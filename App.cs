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
            (context.EnsureNotNull(), propertyName.EnsureNotNull());
    }


    /// <summary>
    /// The evaluator that finds and validates the contexts and behaviors of this environment.
    /// </summary>
    public Evaluator Evaluator { get; private set; }


    /// <summary>
    /// A mapping of behavior types to their behavior factories.
    /// </summary>
    private readonly Dictionary<Type, IBehaviorFactory> _behaviorFactories = new();

    /// <summary>
    /// All currently contextualized context instances, keyed by their class type.
    /// </summary>
    private readonly Dictionary<Type, HashSet<object>> _contexts = new();

    /// <summary>
    /// A mapping of context types to the behavior factories that require them 
    /// to be fulfill behavior instances.
    /// </summary>
    private readonly Dictionary<Type, List<IBehaviorFactory>> _contextBehaviorFactories = new();

    /// <summary>
    /// A mapping of context instances to the behavior instances that depend upon them.
    /// </summary>
    private readonly Dictionary<object, HashSet<BehaviorInstance>> _contextBehaviors = new();

    /// <summary>
    /// A record of context changes that have occurred since the last evaluation.
    /// </summary>
    private readonly List<ContextChange> _contextChanges = new();

    /// <summary>
    /// A queue of behavior factories that can (and should) be processed to 
    /// instantiate new behaviors.
    /// </summary>
    private readonly Queue<IBehaviorFactory> _pendingFactories = new();

    private bool _isInitialized = false;


    /// <summary>
    /// Constructs a new app with the default evaluator, 
    /// <see cref="Evaluator{TContextAttribute, TBehaviorAttribute, 
    /// TBaseDependencyAttribute, TOperationAttribute}"/>.
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

        Type[] behaviorTypes = Evaluator.GetBehaviorTypes();
        for (int c = 0, count = behaviorTypes.Length; c < count; c++)
            RegisterBehaviorFactory(behaviorTypes[c]);

        _isInitialized = true;

        ProcessPendingFactories();
    }

    /// <summary>
    /// Registers the factory for the provided type of behavior by mapping the behavior's 
    /// dependencies to the factory and queueing any initialization behavior factories 
    /// for instantiation.
    /// </summary>
    /// <param name="behaviorType">The type of the behavior whose factory 
    /// is being registered.</param>
    private void RegisterBehaviorFactory(Type behaviorType)
    {
        IBehaviorFactory factory = Evaluator.BuildBehaviorFactory(behaviorType);
        _behaviorFactories.Add(behaviorType, factory);

        Type[] dependencies = Evaluator.GetBehaviorRequiredDependencies(behaviorType);
        for (int c = 0, count = dependencies.Length; c < count; c++)
        {
            if (!_contextBehaviorFactories.ContainsKey(dependencies[c]))
                _contextBehaviorFactories.Add(dependencies[c], new());

            _contextBehaviorFactories[dependencies[c]].Add(factory);
        }

        if (factory.CanInstantiate)
            _pendingFactories.Enqueue(factory);
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
                $"of type {type.FullName} cannot be contextualized as it is not a context.");

        if (!_contexts.ContainsKey(type))
            _contexts.Add(type, new());

        if (!_contexts[type].Add(context))
            return;

        BindContext(context, type);
        AddContextToBehaviorFactories(context, type);
        ProcessPendingFactories();
    }


    /// <summary>
    /// Decontextualizes the provided context, removing it from the app's contextual state.
    /// </summary>
    /// <typeparam name="T">The type of the context.</typeparam>
    /// <param name="context">The context to be deregistered.</param>
    public void Decontextualize<T>(T context) where T : notnull
    {
        ValidateInitialization();

        if (context == null)
            throw new ArgumentNullException(nameof(context));

        Type type = typeof(T);
        if (!_contexts.ContainsKey(typeof(T)))
            throw new InvalidOperationException($"The provided instance, {context}, " +
                $"of type {type.FullName} cannot be decontextualized as it is not a context.");

        UnbindContext(context);
        RemoveContext(context);
        DeregisterContextBehaviorInstances(context);
        RemoveContextFromFactories(context);
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
        bool hadChanges = false;
        if (_contextChanges.Count == 0)
            return hadChanges;

        ContextChange[] contextChanges = _contextChanges.ToArray();
        _contextChanges.Clear();

        for (int c = 0, count = contextChanges.Length; c < count; c++)
        {
            ContextChange change = contextChanges[c];
            object context = change.Context;

            Type contextType = context.GetType();
            if (!_contexts.ContainsKey(contextType) || !_contexts[contextType].Contains(context))
                continue;

            hadChanges = true;

            if (!_contextBehaviors.ContainsKey(context))
                continue;

            foreach (BehaviorInstance bInstance in _contextBehaviors[context])
            {
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
        }

        return hadChanges;
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
            arguments[pco] = bInstance.Contexts[parameters[pco].Name.EnsureNotNull()];

        operation.Invoke(bInstance.Behavior, arguments);
    }

    /// <summary>
    /// Processes the app's pending factories until there are no more remaining.
    /// </summary>
    private void ProcessPendingFactories()
    {
        while (_pendingFactories.Count > 0)
        {
            BehaviorInstance[] newInstances = _pendingFactories.Dequeue().Process();
            for (int c = 0, count = newInstances.Length; c < count; c++)
                RegisterBehaviorInstance(newInstances[c]);
        }
    }

    /// <summary>
    /// Registers the provided behavior instance by contextualizing its self created contexts 
    /// and associating the instance's contexts with the instance.
    /// </summary>
    /// <param name="instance">The instance to be registered.</param>
    private void RegisterBehaviorInstance(BehaviorInstance instance)
    {
        for (int c = 0, count = instance.SelfCreatedContexts.Length; c < count; c++)
            Contextualize(instance.SelfCreatedContexts[c]);

        foreach (object context in instance.Contexts.Values)
        {
            if (!_contextBehaviors.ContainsKey(context))
                _contextBehaviors.Add(context, new());

            _contextBehaviors[context].Add(instance);
        }
    }

    #region Contextualization Functions
    /// <summary>
    /// Adds the provided context to its dependent behavior factories.
    /// </summary>
    /// <param name="context">The context to be added.</param>
    /// <param name="type">The type of the context.</param>
    private void AddContextToBehaviorFactories(object context, Type type)
    {
        if (_contextBehaviorFactories.ContainsKey(type))
            for (int c = 0, count = _contextBehaviorFactories[type].Count; c < count; c++)
                if (_contextBehaviorFactories[type][c].AddAvailableDependency(context))
                    _pendingFactories.Enqueue(_contextBehaviorFactories[type][c]);
    }

    /// <summary>
    /// Binds the provided context for state changes.
    /// </summary>
    /// <param name="context">The context to be bound.</param>
    /// <param name="type">The type of the context.</param>
    private void BindContext(object context, Type type)
    {
        PropertyInfo[] bindableProperties = Evaluator.GetBindableStateInfos(type);
        for (int c = 0, count = bindableProperties.Length; c < count; c++)
        {
            int contextIndex = c;
            (bindableProperties[c].GetValue(context) as IBindableState)?.Bind(() =>
                _contextChanges.Add(new(context, bindableProperties[contextIndex].Name)));
        }
    }
    #endregion

    #region Decontextualization Functions
    /// <summary>
    /// Deregisters all behaviors dependent upon the provided context.
    /// </summary>
    /// <typeparam name="T">The type of the context.</typeparam>
    /// <param name="context">The context whose behaviors are to be deregistered.</param>
    private void DeregisterContextBehaviorInstances<T>(T context) where T : notnull
    {
        if (_contextBehaviors.ContainsKey(context))
        {
            foreach (BehaviorInstance behaviorInstance in _contextBehaviors[context])
                foreach (object dependency in behaviorInstance.Contexts.Values)
                    if (!dependency.Equals(context))
                        _behaviorFactories[behaviorInstance.Behavior.GetType()]
                            .AddAvailableDependency(dependency);

            _contextBehaviors.Remove(context);
        }
    }

    /// <summary>
    /// Removes the provided context from the scope of the App.
    /// </summary>
    /// <typeparam name="T">The type of the context.</typeparam>
    /// <param name="context">The context to be removed.</param>
    private void RemoveContext<T>(T context) where T : notnull
    {
        _contexts[typeof(T)].Remove(context);
        if (_contexts[typeof(T)].Count == 0)
            _contexts.Remove(typeof(T));
    }

    /// <summary>
    /// Removes the provided context from all of its dependent behavior factories.
    /// </summary>
    /// <typeparam name="T">The type of the context.</typeparam>
    /// <param name="context">The context to be removed.</param>
    private void RemoveContextFromFactories<T>(T context) where T : notnull
    {
        if (_contextBehaviorFactories.ContainsKey(typeof(T)))
            for (int c = 0, count = _contextBehaviorFactories[typeof(T)].Count; c < count; c++)
                _contextBehaviorFactories[typeof(T)][c].RemoveAvailableDependency(context);
    }

    /// <summary>
    /// Unbinds the provided context from state changes.
    /// </summary>
    /// <typeparam name="T">The type of the context.</typeparam>
    /// <param name="context">The context being unbound.</param>
    private void UnbindContext<T>(T context) where T : notnull
    {
        PropertyInfo[] bindableProperties = Evaluator.GetBindableStateInfos(typeof(T));
        for (int c = 0, count = bindableProperties.Length; c < count; c++)
            (bindableProperties[c].GetValue(context) as IBindableState)?.Unbind();
    }
    #endregion
}
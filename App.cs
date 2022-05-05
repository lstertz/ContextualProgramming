using ContextualProgramming.Internal;
using System.Reflection;

namespace ContextualProgramming;


/// <summary>
/// Handles contextualization and decontextualization of contexts 
/// managed by behaviors.
/// </summary>
public interface IBehaviorApp
{
    /// <summary>
    /// Contextualizes a new context, including it as part of the app's contextual state.
    /// </summary>
    /// <typeparam name="T">The type of the context.</typeparam>
    /// <param name="context">The context to be registered.</param>
    /// <exception cref="ArgumentNullException">Thrown if the provided context 
    /// instance is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown if the provided instance is 
    /// not a context instance.</exception>
    public void Contextualize<T>(T context) where T : class;

    /// <summary>
    /// Decontextualizes the provided context, removing it from the app's contextual state.
    /// </summary>
    /// <typeparam name="T">The type of the context.</typeparam>
    /// <param name="context">The context to be deregistered.</param>
    public void Decontextualize<T>(T context) where T : notnull;
}

/// <summary>
/// Handles the state and behavior of the running application by managing 
/// registered contexts and the resulting behaviors within an encapsulated environment.
/// </summary>
public class App : IBehaviorApp
{
    /// <summary>
    /// Represents a record of a change for a state of a context.
    /// </summary>
    private class ContextChange
    {
        /// <summary>
        /// The context that was changed.
        /// </summary>
        public object Context { get; private set; }

        /// <summary>
        /// The name of the state that was changed.
        /// </summary>
        public string StateName { get; private set; }


        /// <summary>
        /// Constructs a new context change to hold details of a changed context.
        /// </summary>
        /// <param name="context"><see cref="Context"/></param>
        /// <param name="stateName"><see cref="StateName"/></param>
        public ContextChange(object context, string stateName) => (Context, StateName) =
            (context.EnsureNotNull(), stateName.EnsureNotNull());


        /// <inheritdoc/>
        public override bool Equals(object? obj)
        {
            if (obj is ContextChange other)
                return Equals(other);
            return false;
        }

        /// <summary>
        /// Verifies whether the provided context change is equal to this context change.
        /// </summary>
        /// <param name="other">The other context change.</param>
        /// <returns>Whether the other context change is equal to this context change.</returns>
        public bool Equals(ContextChange other)
        {
            if (other == null)
                return false;

            return Context == other.Context && StateName == other.StateName;
        }

        /// <inheritdoc/>
        public override int GetHashCode() => base.GetHashCode();
    }


    /// <summary>
    /// Mapping of behavior instances to their encapsulating apps.
    /// </summary>
    private static readonly Dictionary<object, IBehaviorApp> _behaviorApps = new();

    /// <summary>
    /// Provides the app that the specified behavior belongs to.
    /// </summary>
    /// <param name="behavior">The behavior whose encapsulating app is to be provided.</param>
    /// <returns>The app that encapsulates the specified behavior.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the provided 
    /// object is not an instance of a behavior known to any running app.</exception>
    public static IBehaviorApp Of(object behavior)
    {
        if (_behaviorApps.TryGetValue(behavior, out IBehaviorApp? app))
            return app;

        throw new InvalidOperationException($"The provided behavior does not " +
            $"belong to any running app.");
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
    /// to fulfill behavior instances.
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
    /// A mapping of mutualist context instances to their host context instances.
    /// </summary>
    private readonly Dictionary<object, HashSet<object>> _contextHosts = new();

    /// <summary>
    /// A mapping of host context instances to their mutualist context instances, 
    /// keyed by their mutualist names.
    /// </summary>
    private readonly Dictionary<object, Dictionary<string, object>> _contextMutualists = new();

    /// <summary>
    /// A mapping of context types to the mutualism fulfillers that will fulfill any 
    /// mutualistic relationships of their context type. A fulfiller will be null if 
    /// the context type has no mutualistic relationships.
    /// </summary>
    private readonly Dictionary<Type, IMutualismFulfiller?> _contextMutualismFulfillers = new();

    /// <summary>
    /// A mapping of context instances to a mapping of the context's state properties to 
    /// the context changes that should be evaluated when the state changes.
    /// </summary>
    private Dictionary<IBindableState, List<ContextChange>>
        _contextStatePotentialChanges = new();

    /// <summary>
    /// The contexts that have been decontextualized since the deregistration of 
    /// behaviors in the last update.
    /// </summary>
    private readonly HashSet<object> _decontextualizedContexts = new();

    /// <summary>
    /// A queue of behavior factories that can (and should) be processed to 
    /// instantiate new behaviors.
    /// </summary>
    private readonly List<IBehaviorFactory> _pendingFactories = new();

    private bool _isInitialized = false;


    /// <summary>
    /// Constructs a new app with the default evaluator, 
    /// <see cref="Evaluator{TContextAttribute, TMutualismAttribute, TMutualAttribute, 
    /// TBehaviorAttribute, TBaseDependencyAttribute, TOperationAttribute}"/>.
    /// </summary>
    public App()
    {
        Evaluator = new Evaluator<ContextAttribute, MutualismAttribute, MutualAttribute,
            BehaviorAttribute, DependencyAttribute, OperationAttribute>();
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
            _pendingFactories.Add(factory);
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


        FulfillMutualisms(context, type);
        BindContext(context, type);
        ContextualizeMutualists(context);

        if (!_decontextualizedContexts.Remove(context))
            AddContextToBehaviorFactories(context, type);
    }

    /// <summary>
    /// Decontextualizes the provided context, removing it from the app's contextual state.
    /// </summary>
    /// <typeparam name="T">The type of the context.</typeparam>
    /// <param name="context">The context to be deregistered.</param>
    public void Decontextualize<T>(T context) where T : notnull
    {
        ValidateInitialization();

        Decontextualize(context as object);
    }

    /// <inheritdoc cref="Decontextualize{T}(T)"/>
    private void Decontextualize(object context)
    {
        if (context == null)
            throw new ArgumentNullException(nameof(context));

        Type type = context.GetType();
        if (!_contexts.ContainsKey(type))
            throw new InvalidOperationException($"The provided instance, {context}, " +
                $"of type {type.FullName} cannot be decontextualized as it is not a context.");

        UnbindContext(context, type);
        BreakMutualismsForHost(context);
        BreakMutualismsForMutualist(context);
        RemoveContext(context, type);
        _decontextualizedContexts.Add(context);
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
        if (_contextChanges.Count == 0 && _pendingFactories.Count == 0 &&
            _decontextualizedContexts.Count == 0)
            return false;

        bool hadChanges = ProcessDecontextualizations();
        hadChanges = ProcessPendingFactories() || hadChanges;
        hadChanges = ProcessContextChanges() || hadChanges;

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
    /// Processes any context changes that have occurred since the start of the last 
    /// time context changes were processed.
    /// </summary>
    /// <returns>Whether any context changes were processed.</returns>
    private bool ProcessContextChanges()
    {
        if (_contextChanges.Count == 0)
            return false;

        ContextChange[] contextChanges = _contextChanges.ToArray();
        _contextChanges.Clear();

        bool hadChanges = false;
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
                    behaviorType, contextName, change.StateName);
                for (int so = 0, soCount = stateOperations.Length; so < soCount; so++)
                    InvokeOperation(bInstance, stateOperations[so]);
            }
        }

        return hadChanges;
    }

    /// <summary>
    /// Processes the decontextualizations that have occurred since the last 
    /// time decontextualizations started to be processed.
    /// </summary>
    /// <returns>Whether any decontextualizations were processed.</returns>
    private bool ProcessDecontextualizations()
    {
        if (_decontextualizedContexts.Count == 0)
            return false;

        object[] decontextualizedContexts = _decontextualizedContexts.ToArray();
        _decontextualizedContexts.Clear();

        foreach (object context in decontextualizedContexts)
        {
            RemoveContextFromFactories(context, context.GetType());
            DeregisterContextBehaviorInstances(context);
        }

        return true;
    }

    /// <summary>
    /// Processes the app's pending factories until there are no more remaining.
    /// </summary>
    /// <returns>Whether any instances were made from the pending factories.</returns>
    private bool ProcessPendingFactories()
    {
        bool hadNewInstances = false;

        IBehaviorFactory[] pendingFactories = _pendingFactories.ToArray();
        _pendingFactories.Clear();

        for (int c = 0, count = pendingFactories.Length; c < count; c++)
        {
            BehaviorInstance[] newInstances = pendingFactories[c].Process();
            if (newInstances.Length > 0)
            {
                hadNewInstances = true;

                for (int cc = 0, cCount = newInstances.Length; cc < cCount; cc++)
                    RegisterBehaviorInstance(newInstances[cc]);
            }
        }

        return hadNewInstances;
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

        _behaviorApps.Add(instance.Behavior, this);
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
                    _pendingFactories.Add(_contextBehaviorFactories[type][c]);
    }

    /// <summary>
    /// Binds the provided context for state changes.
    /// </summary>
    /// <param name="context">The context to be bound.</param>
    /// <param name="type">The type of the context.</param>
    private void BindContext(object context, Type type)
    {
        Tuple<PropertyInfo, Tuple<string, PropertyInfo>>[] mutualStateInfos =
            Evaluator.GetMutualStateInfos(type);
        for (int c = 0, count = mutualStateInfos.Length; c < count; c++)
        {
            // TODO : 29 :: Simplify with value tuple.

            PropertyInfo hostPropertyInfo = mutualStateInfos[c].Item1;
            string mutualistName = mutualStateInfos[c].Item2.Item1;
            PropertyInfo mutualistPropertyInfo = mutualStateInfos[c].Item2.Item2;

            mutualistPropertyInfo.SetValue(_contextMutualists[context][mutualistName],
                hostPropertyInfo.GetValue(context));
        }

        PropertyInfo[] bindableProperties = Evaluator.GetBindableStateInfos(type);
        for (int c = 0, count = bindableProperties.Length; c < count; c++)
            BindState(context, bindableProperties[c]);
    }

    /// <summary>
    /// Binds the context state, determined by the provided context instance and 
    /// property info, to an action that tracks context changes.
    /// </summary>
    /// <param name="context">The context whose state is to be bound.</param>
    /// <param name="propertyInfo">The property info of the state to be bound.</param>
    private void BindState(object context, PropertyInfo propertyInfo)
    {
        if (propertyInfo.GetValue(context) is not IBindableState state)
            return;

        if (!state.IsBound)
        {
            List<ContextChange> changes = new();
            state.Bind(() => _contextChanges.AddRange(changes));
            _contextStatePotentialChanges.Add(state, changes);
        }

        _contextStatePotentialChanges[state].Add(new(context, propertyInfo.Name));
    }

    /// <summary>
    /// Contextualizes the mutualists of the provided context.
    /// </summary>
    /// <param name="context">The host context whose mutualists should be contextualized.</param>
    private void ContextualizeMutualists(object context)
    {
        if (!_contextMutualists.ContainsKey(context))
            return;

        foreach (object mutualist in _contextMutualists[context].Values)
            Contextualize(mutualist);
    }

    /// <summary>
    /// Fulfills the mutualistic relationships of the provided context.
    /// </summary>
    /// <param name="context">The context whose mutualistic relationships 
    /// should be fulfilled.</param>
    /// <param name="type">The type of the provided context.</param>
    private void FulfillMutualisms(object context, Type type)
    {
        if (!_contextMutualismFulfillers.ContainsKey(type))
            _contextMutualismFulfillers.Add(type, Evaluator.BuildMutualismFulfiller(type));

        IMutualismFulfiller? fulfiller = _contextMutualismFulfillers[type];
        if (fulfiller == null)
            return;

        Tuple<string, object>[] mutualists = fulfiller.Fulfill(context);
        _contextMutualists.Add(context, mutualists
            .ToDictionary(mutualist => mutualist.Item1, mutualist => mutualist.Item2));

        for (int c = 0, count = mutualists.Length; c < count; c++)
            _contextHosts.Add(mutualists[c].Item2, new() { context });
    }
    #endregion

    #region Decontextualization Functions
    /// <summary>
    /// Breaks the mutualistic relationships between the provided host and any 
    /// of its mutualists. Mutualists that no longer have a host are 
    /// also decontextualized.
    /// </summary>
    /// <param name="host">The context whose mutualistic relationships 
    /// are to be broken.</param>
    private void BreakMutualismsForHost(object host)
    {
        if (!_contextMutualists.ContainsKey(host))
            return;

        Dictionary<string, object> mutualists = _contextMutualists[host];
        _contextMutualists.Remove(host);

        foreach (object mutualist in mutualists.Values)
        {
            if (!_contextHosts.ContainsKey(mutualist))
                continue;

            _contextHosts[mutualist].Remove(host);
            if (_contextHosts[mutualist].Count == 0)
                Decontextualize(mutualist);
        }
    }


    /// <summary>
    /// Breaks the mutualistic relationships between the provided mutualist and any 
    /// of its hosts. All hosts are decontextualized.
    /// </summary>
    /// <param name="mutualist">The context whose mutualistic relationships 
    /// are to be broken.</param>
    private void BreakMutualismsForMutualist(object mutualist)
    {
        if (!_contextHosts.ContainsKey(mutualist))
            return;

        object[] hosts = _contextHosts[mutualist].ToArray();
        _contextHosts.Remove(mutualist);

        for (int c = 0, count = hosts.Length; c < count; c++)
            Decontextualize(hosts[c]);
    }

    /// <summary>
    /// Deregisters all behaviors dependent upon the provided context.
    /// </summary>
    /// <param name="context">The context whose behaviors are to be deregistered.</param>
    private void DeregisterContextBehaviorInstances(object context)
    {
        if (_contextBehaviors.ContainsKey(context))
        {
            foreach (BehaviorInstance behaviorInstance in _contextBehaviors[context])
            {
                Type behaviorType = behaviorInstance.Behavior.GetType();
                foreach (object dependency in behaviorInstance.Contexts.Values)
                    if (!dependency.Equals(context))
                    {
                        IBehaviorFactory factory = _behaviorFactories[behaviorType];
                        if (factory.AddAvailableDependency(dependency))
                            _pendingFactories.Add(factory);

                        // TODO :: If a context isn't used by any factories after all of its 
                        //          behavior instances are removed, the context 
                        //          should be removed, since it won't ever be used.

                        _contextBehaviors[dependency].Remove(behaviorInstance);
                    }

                MethodInfo[] teardownOperations = Evaluator.GetOnTeardownOperations(behaviorType);
                for (int c = 0, count = teardownOperations.Length; c < count; c++)
                    InvokeOperation(behaviorInstance, teardownOperations[c]);

                _behaviorApps.Remove(behaviorInstance.Behavior);
            }

            _contextBehaviors.Remove(context);
        }
    }

    /// <summary>
    /// Removes the provided context from the scope of the App.
    /// </summary>
    /// <param name="context">The context to be removed.</param>
    /// <param name="type">The type of the context.</param>
    private void RemoveContext(object context, Type type)
    {
        _contexts[type].Remove(context);
        if (_contexts[type].Count == 0)
            _contexts.Remove(type);
    }

    /// <summary>
    /// Removes the provided context from all of its dependent behavior factories.
    /// </summary>
    /// <param name="context">The context to be removed.</param>
    /// <param name="type">The type of the context.</param>
    private void RemoveContextFromFactories(object context, Type type)
    {
        if (_contextBehaviorFactories.ContainsKey(type))
            for (int c = 0, count = _contextBehaviorFactories[type].Count; c < count; c++)
                _contextBehaviorFactories[type][c].RemoveAvailableDependency(context);
    }

    /// <summary>
    /// Unbinds the provided context from state changes.
    /// </summary>
    /// <param name="context">The context being unbound.</param>
    /// <param name="type">The type of the context.</param>
    private void UnbindContext(object context, Type type)
    {
        PropertyInfo[] bindableProperties = Evaluator.GetBindableStateInfos(type);
        for (int c = 0, count = bindableProperties.Length; c < count; c++)
            UnbindState(context, bindableProperties[c]);
    }

    /// <summary>
    /// Unbinds the context state determined by the provided context instance and 
    /// property info.
    /// </summary>
    /// <param name="context">The context whose state is to be unbound.</param>
    /// <param name="propertyInfo">The property info of the state to be unbound.</param>
    private void UnbindState(object context, PropertyInfo propertyInfo)
    {
        if (propertyInfo.GetValue(context) is not IBindableState state)
            return;

        _contextStatePotentialChanges[state].Remove(new(context, propertyInfo.Name));
        if (_contextStatePotentialChanges[state].Count == 0)
        {
            _contextStatePotentialChanges.Remove(state);
            state.Unbind();
        }
    }
    #endregion
}
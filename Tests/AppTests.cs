using ContextualProgramming.Internal;
using NSubstitute;
using NUnit.Framework;
using System.Reflection;

namespace AppTests
{
    public static class SetUp
    {
        public static App BehaviorAndContextApp<TBehavior, TContext>(
            IBehaviorFactory factory, Type[]? behaviorUnfulfilledDependencies = null)
        {
            Evaluator evaluator = Substitute.For<Evaluator>();
            App app = new(evaluator);

            PrimeEvaluatorForBehavior<TBehavior>(evaluator, factory,
                behaviorUnfulfilledDependencies);
            PrimeEvaluatorForContext<TContext>(evaluator);
            app.Initialize();

            return app;
        }

        public static App BehaviorAndContextApp<TBehavior, TContextA, TContextB>(
            IBehaviorFactory factory, Type[]? behaviorUnfulfilledDependencies = null)
        {
            Evaluator evaluator = Substitute.For<Evaluator>();
            App app = new(evaluator);

            PrimeEvaluatorForBehavior<TBehavior>(evaluator, factory,
                behaviorUnfulfilledDependencies);
            PrimeEvaluatorForContext<TContextA>(evaluator);
            PrimeEvaluatorForContext<TContextB>(evaluator);
            app.Initialize();

            return app;
        }

        public static App BehaviorAndContextApp<TBehaviorA, TBehaviorB, TContextA, TContextB>(
            IBehaviorFactory factoryA, IBehaviorFactory factoryB,
            Type[]? behaviorUnfulfilledDependenciesA = null,
            Type[]? behaviorUnfulfilledDependenciesB = null)
        {
            Evaluator evaluator = Substitute.For<Evaluator>();
            App app = new(evaluator);

            PrimeEvaluatorForBehavior<TBehaviorA>(evaluator, factoryA,
                behaviorUnfulfilledDependenciesA);
            PrimeEvaluatorForBehavior<TBehaviorB>(evaluator, factoryB,
                behaviorUnfulfilledDependenciesB);
            PrimeEvaluatorForContext<TContextA>(evaluator);
            PrimeEvaluatorForContext<TContextB>(evaluator);
            app.Initialize();

            return app;
        }

        public static void BehaviorOperations<TBehavior>(Evaluator evaluator,
            string contextName, string stateName)
        {
            Type type = typeof(TBehavior);

            if (Operations.OnContextChange.ContainsKey(type))
                evaluator.GetOnChangeOperations(type,
                contextName).Returns(new MethodInfo[]
                {
                    Operations.OnContextChange[type]
                });

            if (Operations.OnContextStateChange.ContainsKey(type))
                evaluator.GetOnChangeOperations(type,
                    contextName, stateName).Returns(new MethodInfo[]
                    {
                    Operations.OnContextStateChange[type]
                    });
        }

        public static App ContextOnlyApp<TContext>()
        {
            Evaluator evaluator = Substitute.For<Evaluator>();
            App app = new(evaluator);

            evaluator.GetBehaviorTypes().Returns(Array.Empty<Type>());
            PrimeEvaluatorForContext<TContext>(evaluator);
            app.Initialize();

            return app;
        }

        public static void PrimeEvaluatorForBehavior<TBehavior>(Evaluator evaluator,
            IBehaviorFactory factory, Type[]? behaviorUnfulfilledDependencies = null)
        {
            Type type = typeof(TBehavior);
            Type[] unfulfilledDependencies = behaviorUnfulfilledDependencies ??
                Array.Empty<Type>();

            evaluator.GetBehaviorTypes().Returns(new Type[] { type });
            evaluator.GetBehaviorRequiredDependencies(type).Returns(unfulfilledDependencies);
            evaluator.BuildBehaviorFactory(type).Returns(factory);
        }

        public static void PrimeEvaluatorForContext<TContext>(Evaluator evaluator)
        {
            Type type = typeof(TContext);

            PropertyInfo[] properties = type.GetProperties(BindingFlags.Instance |
                BindingFlags.NonPublic | BindingFlags.Public);
            List<PropertyInfo> bindableProperties = new();
            for (int c = 0, count = properties.Length; c < count; c++)
                if (typeof(IBindableState).IsAssignableFrom(properties[c].PropertyType))
                    bindableProperties.Add(properties[c]);

            evaluator.GetBindableStateInfos(type).Returns(bindableProperties.ToArray());
            evaluator.IsContextType(type).ReturnsForAnyArgs(true);
        }
    }

    #region Shared Constructs
    public static class Operations
    {
        public static readonly Dictionary<Type, MethodInfo> OnContextChange = new();
        public static readonly Dictionary<Type, MethodInfo> OnContextStateChange = new();

        static Operations()
        {
            MethodInfo? mi = typeof(TestBehaviorA).GetMethod("OnContextAChange");
            if (mi != null)
                OnContextChange.Add(typeof(TestBehaviorA), mi);

            mi = typeof(TestBehaviorA).GetMethod("OnContextAIntChange");
            if (mi != null)
                OnContextStateChange.Add(typeof(TestBehaviorA), mi);

            mi = typeof(TestBehaviorB).GetMethod("OnContextAChange");
            if (mi != null)
                OnContextChange.Add(typeof(TestBehaviorB), mi);

            mi = typeof(TestBehaviorB).GetMethod("OnContextAIntChange");
            if (mi != null)
                OnContextStateChange.Add(typeof(TestBehaviorB), mi);
        }
    }

    public static class Mappings
    {
        public static readonly Dictionary<Type, string> ContextNames = new()
        {
            { typeof(TestContextA), "contextA" },
            { typeof(TestContextB), "contextB" },
            { typeof(TestContextC), "contextC" }
        };
    }


    public class BehaviorFactoryDouble<TBehavior> : IBehaviorFactory
        where TBehavior : new()
    {
        public bool CanInstantiate => NumberOfPendingInstantiations > 0;

        public int NumberOfPendingInstantiations =>
            _availableDependencies.Count == RequiredDependencyTypes.Length ? 1 : 0;

        public Type[] RequiredDependencyTypes { get; init; }


        private readonly Dictionary<Type, List<object>> _availableDependencies = new();
        private readonly Dictionary<string, object> _existingContexts;
        private readonly Dictionary<string, object> _selfCreatedContexts;


        public BehaviorFactoryDouble(Dictionary<string, object> selfCreatedContexts,
            Dictionary<string, object>? existingContexts = null,
            Type[]? behaviorUnfulfilledDependencies = null)
        {
            RequiredDependencyTypes = behaviorUnfulfilledDependencies ?? Array.Empty<Type>();

            _existingContexts = existingContexts ?? new();
            _selfCreatedContexts = selfCreatedContexts;
        }

        public bool AddAvailableDependency(object dependency)
        {
            Type type = dependency.GetType();

            if (!_availableDependencies.ContainsKey(type))
                _availableDependencies.Add(type, new());
            _availableDependencies[type].Add(dependency);

            return CanInstantiate;
        }

        public BehaviorInstance[] Process()
        {
            if (!CanInstantiate)
                return Array.Empty<BehaviorInstance>();

            Dictionary<string, object> contexts = new();
            foreach (var kvp in _selfCreatedContexts)
                contexts.Add(kvp.Key, kvp.Value);
            foreach (var kvp in _existingContexts)
                contexts.Add(kvp.Key, kvp.Value);
            foreach (var kvp in _availableDependencies)
            {
                contexts.Add(Mappings.ContextNames[kvp.Key], kvp.Value[0]);
                _availableDependencies[kvp.Key].RemoveAt(0);
            }

            BehaviorInstance instance = new(new TBehavior(), contexts,
                _selfCreatedContexts.Values.ToArray());
            return new BehaviorInstance[] { instance };
        }

        public bool RemoveAvailableDependency(object dependency)
        {
            Type type = dependency.GetType();

            if (_availableDependencies.ContainsKey(type))
            {
                _availableDependencies[type].Remove(dependency);
                if (_availableDependencies[type].Count == 0)
                    _availableDependencies.Remove(type);
            }

            return CanInstantiate;
        }
    }

    public class TestBehaviorA
    {
        public static readonly string ContextAName = Mappings.ContextNames[typeof(TestContextA)];
        public static readonly string ContextBName = Mappings.ContextNames[typeof(TestContextB)];

        public void OnContextAChange(TestContextA contextA)
        {
            contextA.OnContextChangeIntValue = contextA.Int;
        }

        public void OnContextAIntChange(TestContextA contextA, TestContextB contextB)
        {
            contextA.OnStateChangeIntValue = contextA.Int;
            contextB.Int.Value = contextA.Int;
        }

        public void OnContextBIntChange(TestContextB contextB)
        {
            contextB.OnContextChangeIntValue = contextB.Int;
        }
    }

    public class TestBehaviorB
    {
        public static readonly string ContextAName = Mappings.ContextNames[typeof(TestContextA)];
        public static readonly string ContextCName = Mappings.ContextNames[typeof(TestContextC)];

        public void OnContextAChange(TestContextA contextA)
        {
            contextA.OnContextChangeIntValue = contextA.Int;
        }

        public void OnContextAIntChange(TestContextA contextA)
        {
            contextA.OnStateChangeIntValue = contextA.Int;
        }
    }


    public class TestContextA
    {
        public int OnStateChangeIntValue { get; set; } = 0;
        public int OnContextChangeIntValue { get; set; } = 0;

        public ContextState<int> Int { get; init; } = 0;
    }

    public class TestContextB
    {
        public int OnContextChangeIntValue { get; set; } = 0;

        public ContextState<int> Int { get; init; } = 0;
    }

    public class TestContextC { }

    public class TestNonContext { }
    #endregion

    public static class Validate
    {
        public static void TestBehaviorADoesNotExist(App app, TestContextA contextA)
        {
            TestBehaviorAOnChangeOperationsNotInvoked(app, contextA);
        }

        public static void TestBehaviorAExists(App app, TestContextA contextA)
        {
            TestBehaviorAOnChangeOperationsInvoked(app, contextA);
        }

        public static void TestBehaviorAOnChangeOperationsInvoked(App app, TestContextA contextA,
            int? existingValueChange = null)
        {
            int expectedValue = TestBehaviorAChangeOperationDependentValue(
                app, contextA, existingValueChange);

            Assert.AreEqual(expectedValue, contextA.OnContextChangeIntValue);
            Assert.AreEqual(expectedValue, contextA.OnStateChangeIntValue);
        }

        public static void TestBehaviorAOnChangeOperationsNotInvoked(App app,
            TestContextA contextA, int? existingValueChange = null)
        {
            int expectedValue = TestBehaviorAChangeOperationDependentValue(
                app, contextA, existingValueChange);

            Assert.AreNotEqual(expectedValue, contextA.OnContextChangeIntValue);
            Assert.AreNotEqual(expectedValue, contextA.OnStateChangeIntValue);
        }


        private static int TestBehaviorAChangeOperationDependentValue(App app,
            TestContextA contextA, int? existingValueChange = null)
        {
            SetUp.BehaviorOperations<TestBehaviorA>(app.Evaluator,
                TestBehaviorA.ContextAName, nameof(TestContextA.Int));

            int expectedValue;
            if (!existingValueChange.HasValue)
            {
                expectedValue = 11;
                contextA.Int.Value = expectedValue;
            }
            else
                expectedValue = existingValueChange.Value;

            app.Update(); // Behavior A updates Context A's context change value.

            return expectedValue;
        }
    }

    public class Construction
    {
        [Test]
        public void DefaultEvaluator()
        {
            Type expectedEvaluator = typeof(Evaluator<ContextAttribute, BehaviorAttribute,
                DependencyAttribute, OperationAttribute>);

            App app = new();

            Assert.AreEqual(expectedEvaluator, app.Evaluator.GetType());
        }

        [Test]
        public void OverriddenEvaluator()
        {
            Evaluator evaluator = Substitute.For<Evaluator>();

            App app = new(evaluator);

            Assert.AreEqual(evaluator.GetType(), app.Evaluator.GetType());
        }
    }

    public class Contextualization
    {
        [Test]
        public void BindsForChanges()
        {
            IBehaviorFactory factory = new BehaviorFactoryDouble<TestBehaviorA>(new());
            App app = SetUp.BehaviorAndContextApp<TestBehaviorA, TestContextA,
                TestContextB>(factory);

            TestContextA context = new();
            context.Int.Value = 10;

            Assert.IsFalse(app.Update()); // Not yet bound, so changes aren't detected.

            app.Contextualize(context);

            context.Int.Value = 11;

            Assert.IsTrue(app.Update());
        }

        [Test]
        public void Contextualizes()
        {
            IBehaviorFactory factory = new BehaviorFactoryDouble<TestBehaviorA>(new());
            App app = SetUp.BehaviorAndContextApp<TestBehaviorA, TestContextA,
                TestContextB>(factory);

            TestContextA expectedContext = new();
            app.Contextualize(expectedContext);

            Assert.AreEqual(expectedContext, app.GetContext<TestContextA>());
        }

        [Test]
        public void DependentFulfilledBehavior_ContextualizesSelfCreatedDependencies()
        {
            TestContextA expectedContextA = new();
            Type[] requiredDependencies = new Type[]
            {
                typeof(TestContextB)
            };

            IBehaviorFactory factory = new BehaviorFactoryDouble<TestBehaviorA>(new()
            {
                { TestBehaviorA.ContextAName, expectedContextA }
            }, new(), requiredDependencies);
            App app = SetUp.BehaviorAndContextApp<TestBehaviorA, TestContextA,
                TestContextB>(factory, requiredDependencies);

            Assert.IsNull(app.GetContext<TestContextA>());

            TestContextB contextB = new();
            app.Contextualize(contextB);

            Assert.AreEqual(expectedContextA, app.GetContext<TestContextA>());
        }

        [Test]
        public void DependentFulfilledBehavior_ExistingChanges_InvokesOnChangeChangeOperations()
        {
            Type[] requiredDependencies = new Type[]
            {
                typeof(TestContextA),
                typeof(TestContextB)
            };

            IBehaviorFactory factory = new BehaviorFactoryDouble<TestBehaviorA>(new(), new(),
                requiredDependencies);
            App app = SetUp.BehaviorAndContextApp<TestBehaviorA, TestContextA,
                TestContextB>(factory, requiredDependencies);

            TestContextA contextA = new();
            app.Contextualize(contextA);
            int expectedValue = 11;
            contextA.Int.Value = expectedValue;

            TestContextB contextB = new();
            app.Contextualize(contextB);

            Validate.TestBehaviorAOnChangeOperationsInvoked(app, contextA, expectedValue);
        }

        [Test]
        public void DependentFulfilledBehavior_NewChanges_InvokesOnContextChangeOperations()
        {
            Type[] requiredDependencies = new Type[]
            {
                typeof(TestContextA),
                typeof(TestContextB)
            };

            IBehaviorFactory factory = new BehaviorFactoryDouble<TestBehaviorA>(new(), new(),
                requiredDependencies);
            App app = SetUp.BehaviorAndContextApp<TestBehaviorA, TestContextA,
                TestContextB>(factory, requiredDependencies);

            TestContextA contextA = new();
            app.Contextualize(contextA);

            TestContextB contextB = new();
            app.Contextualize(contextB);

            Validate.TestBehaviorAOnChangeOperationsInvoked(app, contextA);
        }

        [Test]
        public void DependentFulfilledBehaviors_ExistingChanges_InvokesOnChangeChangeOperations()
        {
            // TODO :: Update for multiple behaviors.
            Assert.Ignore();

            Type[] requiredDependencies = new Type[]
            {
                typeof(TestContextA),
                typeof(TestContextB)
            };

            IBehaviorFactory factory = new BehaviorFactoryDouble<TestBehaviorA>(new(), new(),
                requiredDependencies);
            App app = SetUp.BehaviorAndContextApp<TestBehaviorA, TestContextA,
                TestContextB>(factory, requiredDependencies);

            TestContextA contextA = new();
            app.Contextualize(contextA);
            int expectedValue = 11;
            contextA.Int.Value = expectedValue;

            TestContextB contextB = new();
            app.Contextualize(contextB);

            Validate.TestBehaviorAOnChangeOperationsInvoked(app, contextA, expectedValue);
        }

        [Test]
        public void DependentFulfilledBehaviors_NewChanges_InvokesOnContextChangeOperations()
        {
            // TODO :: Update for multiple behaviors.
            Assert.Ignore();

            Type[] requiredDependencies = new Type[]
            {
                typeof(TestContextA),
                typeof(TestContextB)
            };

            IBehaviorFactory factory = new BehaviorFactoryDouble<TestBehaviorA>(new(), new(),
                requiredDependencies);
            App app = SetUp.BehaviorAndContextApp<TestBehaviorA, TestContextA,
                TestContextB>(factory, requiredDependencies);

            TestContextA contextA = new();
            app.Contextualize(contextA);

            TestContextB contextB = new();
            app.Contextualize(contextB);

            Validate.TestBehaviorAOnChangeOperationsInvoked(app, contextA);
        }

        [Test]
        public void NonContextTypeThrowsException()
        {
            Evaluator evaluator = Substitute.For<Evaluator>();
            App app = new(evaluator);

            evaluator.GetBehaviorTypes().Returns(Array.Empty<Type>());
            evaluator.IsContextType(typeof(TestNonContext)).Returns(false);
            app.Initialize();

            Assert.Throws<InvalidOperationException>(() =>
                app.Contextualize<TestNonContext>(new()));
        }

        [Test]
        public void NullThrowsException()
        {
            App app = SetUp.ContextOnlyApp<TestContextA>();

#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            Assert.Throws<ArgumentNullException>(() =>
                app.Contextualize<TestContextA>(null));
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
        }

        [Test]
        public void UninitializedThrowsException()
        {
            Evaluator evaluator = Substitute.For<Evaluator>();
            App app = new(evaluator);

            evaluator.IsContextType(typeof(TestContextA)).Returns(true);

            Assert.Throws<InvalidOperationException>(() =>
                app.Contextualize<TestContextA>(new()));
        }
    }

    public class Decontextualization
    {
#pragma warning disable CS8618
        private App _app;
#pragma warning restore CS8618

        [SetUp]
        public void SetUp()
        {
            IBehaviorFactory factory = new BehaviorFactoryDouble<TestBehaviorA>(
                new()
                {
                    { TestBehaviorA.ContextAName, new TestContextA() },
                    { TestBehaviorA.ContextBName, new TestContextB() }
                });
            _app = AppTests.SetUp.BehaviorAndContextApp<TestBehaviorA, TestContextA,
                TestContextB>(factory);
        }

        [Test]
        public void Decontextualizes()
        {
            TestContextA? contextA = _app.GetContext<TestContextA>() ??
                throw new NullReferenceException(); ;

            _app.Decontextualize(contextA);

            Assert.IsNull(_app.GetContext<TestContextA>());
        }

        [Test]
        public void DependentBehavior_PreventsFulfillment()
        {
            Type[] requiredDependencies = new Type[]
            {
                typeof(TestContextA),
                typeof(TestContextB)
            };

            IBehaviorFactory factory = new BehaviorFactoryDouble<TestBehaviorA>(
                new(), new(), requiredDependencies);
            App app = AppTests.SetUp.BehaviorAndContextApp<TestBehaviorA, TestContextA,
                TestContextB>(factory, requiredDependencies);

            TestContextB contextB = new();
            app.Contextualize(contextB);

            app.Decontextualize(contextB);

            TestContextA contextA = new();
            app.Contextualize(contextA);

            Validate.TestBehaviorAOnChangeOperationsNotInvoked(app, contextA);
        }

        [Test]
        public void DependentBehavior_RemainingContextsReusedForNewBehaviorInstance()
        {
            Type[] requiredDependencies = new Type[]
            {
                typeof(TestContextA),
                typeof(TestContextB)
            };

            IBehaviorFactory factory = new BehaviorFactoryDouble<TestBehaviorA>(
                new(), new(), requiredDependencies);
            App app = AppTests.SetUp.BehaviorAndContextApp<TestBehaviorA, TestContextA,
                TestContextB>(factory, requiredDependencies);

            TestContextB contextB = new();
            app.Contextualize(contextB);

            TestContextA contextA = new();
            app.Contextualize(contextA);

            app.Decontextualize(contextB);

            app.Contextualize(new TestContextB());
            Validate.TestBehaviorAOnChangeOperationsInvoked(app, contextA);
        }

        [Test]
        public void DependentBehaviors_ExistingChangesIgnoredForOnContextChange()
        {
            // TODO :: Update for multiple behaviors.
            Assert.Ignore();

            TestContextA? contextA = _app.GetContext<TestContextA>() ??
                throw new NullReferenceException();

            int expectedValue = 11;
            contextA.Int.Value = expectedValue;
            _app.Decontextualize(contextA);

            Validate.TestBehaviorAOnChangeOperationsNotInvoked(_app, contextA, expectedValue);
        }

        [Test]
        public void DependentBehaviors_NewChangesIgnoredForOnContextChange()
        {
            // TODO :: Update for multiple behaviors.
            Assert.Ignore();

            TestContextA? contextA = _app.GetContext<TestContextA>();
            if (contextA == null)
                throw new NullReferenceException();

            _app.Decontextualize(contextA);
            Validate.TestBehaviorAOnChangeOperationsNotInvoked(_app, contextA);
        }

        [Test]
        public void DependentBehaviors_PreventsFulfillment()
        {
            // TODO :: Update for multiple behaviors.
            Assert.Ignore();

            Type[] requiredDependencies = new Type[]
            {
                typeof(TestContextA),
                typeof(TestContextB)
            };

            IBehaviorFactory factory = new BehaviorFactoryDouble<TestBehaviorA>(
                new(), new(), requiredDependencies);
            App app = AppTests.SetUp.BehaviorAndContextApp<TestBehaviorA, TestContextA,
                TestContextB>(factory, requiredDependencies);

            TestContextB contextB = new();
            app.Contextualize(contextB);

            app.Decontextualize(contextB);

            TestContextA contextA = new();
            app.Contextualize(contextA);

            Validate.TestBehaviorAOnChangeOperationsNotInvoked(app, contextA);
        }

        [Test]
        public void DependentBehaviors_RemainingContextsReusedForNewBehaviorInstances()
        {
            // TODO :: Update for multiple behaviors.
            Assert.Ignore();
        }

        [Test]
        public void DependentInitializationBehavior_ExistingChangesIgnoredForOnContextChange()
        {
            TestContextA? contextA = _app.GetContext<TestContextA>() ??
                throw new NullReferenceException();

            int expectedValue = 11;
            contextA.Int.Value = expectedValue;
            _app.Decontextualize(contextA);

            Validate.TestBehaviorAOnChangeOperationsNotInvoked(_app, contextA, expectedValue);
        }

        [Test]
        public void DependentInitializationBehavior_NewChangesIgnoredForOnContextChange()
        {
            TestContextA? contextA = _app.GetContext<TestContextA>();
            if (contextA == null)
                throw new NullReferenceException();

            _app.Decontextualize(contextA);
            Validate.TestBehaviorAOnChangeOperationsNotInvoked(_app, contextA);
        }

        [Test]
        public void NonContextTypeThrowsException()
        {
            Evaluator evaluator = Substitute.For<Evaluator>();
            App? app = new(evaluator);

            evaluator.GetBehaviorTypes().Returns(Array.Empty<Type>());
            evaluator.IsContextType(typeof(TestNonContext)).Returns(false);
            app.Initialize();

            Assert.Throws<InvalidOperationException>(() =>
                app.Decontextualize<TestNonContext>(new()));
        }

        [Test]
        public void NullThrowsException()
        {
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            Assert.Throws<ArgumentNullException>(() =>
                _app.Decontextualize<TestContextA>(null));
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
        }

        [Test]
        public void UnbindsForChanges()
        {
            TestContextA context = new();
            _app.Contextualize(context);

            _app.Decontextualize(context);
            context.Int.Value = 10;

            Assert.IsFalse(_app.Update());
        }

        [Test]
        public void UninitializedThrowsException()
        {
            Evaluator evaluator = Substitute.For<Evaluator>();
            App app = new(evaluator);

            Assert.Throws<InvalidOperationException>(() =>
                app.Decontextualize<TestContextA>(new()));
        }

        [Test]
        public void UpdateIgnoresExistingChanges()
        {
            TestContextA context = new();
            _app.Contextualize(context);
            context.Int.Value = 10;

            _app.Decontextualize(context);

            Assert.IsFalse(_app.Update());
        }
    }

    public class GetContext
    {
        [Test]
        public void NonContextThrowsException()
        {
            Evaluator evaluator = Substitute.For<Evaluator>();
            App app = new(evaluator);

            evaluator.GetBehaviorTypes().Returns(Array.Empty<Type>());
            evaluator.IsContextType(typeof(TestNonContext)).Returns(false);
            app.Initialize();

            Assert.Throws<InvalidOperationException>(() =>
                app.GetContext<TestNonContext>());
        }

        [Test]
        public void NullWhenNoneAvailable()
        {
            App app = SetUp.ContextOnlyApp<TestContextA>();

            Assert.IsNull(app.GetContext<TestContextA>());
        }

        [Test]
        public void ProvidesFirstWhenMultipleAreAvailable()
        {
            App app = SetUp.ContextOnlyApp<TestContextA>();

            TestContextA firstContext = new();
            app.Contextualize(firstContext);
            app.Contextualize(new TestContextA());

            Assert.AreEqual(firstContext, app.GetContext<TestContextA>());
        }

        [Test]
        public void ProvidesWhenOneIsAvailable()
        {
            App app = SetUp.ContextOnlyApp<TestContextA>();

            TestContextA firstContext = new();
            app.Contextualize(firstContext);

            Assert.AreEqual(firstContext, app.GetContext<TestContextA>());
        }

        [Test]
        public void UninitializedThrowsException()
        {
            Evaluator evaluator = Substitute.For<Evaluator>();
            App app = new(evaluator);

            Assert.Throws<InvalidOperationException>(() => app.GetContext<TestContextA>());
        }
    }

    public class GetContexts
    {
        [Test]
        public void EmptyWhenNonAvailable()
        {
            App app = SetUp.ContextOnlyApp<TestContextA>();

            Assert.IsEmpty(app.GetContexts<TestContextA>());
        }

        [Test]
        public void NonContextThrowsException()
        {
            Evaluator evaluator = Substitute.For<Evaluator>();
            App app = new(evaluator);

            evaluator.GetBehaviorTypes().Returns(Array.Empty<Type>());
            evaluator.IsContextType(typeof(TestNonContext)).Returns(false);
            app.Initialize();

            Assert.Throws<InvalidOperationException>(() =>
                app.GetContexts<TestNonContext>());
        }

        [Test]
        public void ProvidesWhenAvailable()
        {
            App app = SetUp.ContextOnlyApp<TestContextA>();

            TestContextA firstContext = new();
            TestContextA secondContext = new();
            app.Contextualize(firstContext);

            Assert.AreEqual(1, app.GetContexts<TestContextA>().Length);
            Assert.AreEqual(firstContext, app.GetContexts<TestContextA>()[0]);

            app.Contextualize(secondContext);

            Assert.AreEqual(2, app.GetContexts<TestContextA>().Length);
            Assert.Contains(firstContext, app.GetContexts<TestContextA>());
            Assert.Contains(secondContext, app.GetContexts<TestContextA>());
        }

        [Test]
        public void UninitializedThrowsException()
        {
            Evaluator evaluator = Substitute.For<Evaluator>();
            App app = new(evaluator);

            Assert.Throws<InvalidOperationException>(() => app.GetContexts<TestContextA>());
        }
    }

    public class Initialization
    {
#pragma warning disable CS8618
        private App _app;
#pragma warning restore CS8618

        [SetUp]
        public void SetUp()
        {
            // TODO :: Update for multiple behaviors.

            IBehaviorFactory factory = new BehaviorFactoryDouble<TestBehaviorA>(
                new()
                {
                    { TestBehaviorA.ContextAName, new TestContextA() },
                    { TestBehaviorA.ContextBName, new TestContextB() }
                });
            _app = AppTests.SetUp.BehaviorAndContextApp<TestBehaviorA, TestContextA,
                TestContextB>(factory);
        }

        [Test]
        public void FulfilledBehaviors_ContextualizeSelfCreatedContexts()
        {
            Assert.Ignore();
        }

        [Test]
        public void InitializationBehavior_ContextualizesSelfCreatedContexts()
        {
            Assert.NotNull(_app.GetContext<TestContextA>());
        }

        [Test]
        public void InitializationBehavior_SelfCreatedContexts_FulfillDependentBehaviors()
        {
            Assert.Ignore();
        }

        [Test]
        public void InitializationBehavior_NewChanges_InvokeOnContextChangeOperations()
        {
            TestContextA? contextA = _app.GetContext<TestContextA>();
            if (contextA == null)
                throw new NullReferenceException();

            Validate.TestBehaviorAOnChangeOperationsInvoked(_app, contextA);
        }
    }

    public class Updating
    {
#pragma warning disable CS8618
        private App _app;
#pragma warning restore CS8618

        [SetUp]
        public void SetUp()
        {
            // TODO :: Update for multiple behaviors.

            IBehaviorFactory factory = new BehaviorFactoryDouble<TestBehaviorA>(
                new()
                {
                    { TestBehaviorA.ContextAName, new TestContextA() },
                    { TestBehaviorA.ContextBName, new TestContextB() }
                });
            _app = AppTests.SetUp.BehaviorAndContextApp<TestBehaviorA, TestContextA,
                TestContextB>(factory);
        }

        [Test]
        public void NoChanges_DoesNotInvokeContextChangeOperations()
        {
            TestContextA? contextA = _app.GetContext<TestContextA>();
            if (contextA == null)
                throw new NullReferenceException();

            _app.Update();

            Assert.AreEqual(0, contextA.OnContextChangeIntValue);
        }
        [Test]
        public void NoChanges_DoesNotInvokeStateChangeOperations()
        {
            TestContextA? contextA = _app.GetContext<TestContextA>();
            if (contextA == null)
                throw new NullReferenceException();

            _app.Update();

            Assert.AreEqual(0, contextA.OnStateChangeIntValue);
        }

        [Test]
        public void NoChanges_ReturnsFalse()
        {
            Assert.IsFalse(_app.Update());
        }

        [Test]
        public void WithChanges_ClearsPreviousChanges()
        {
            TestContextA? contextA = _app.GetContext<TestContextA>();
            if (contextA == null)
                throw new NullReferenceException();

            contextA.Int.Value = 11;

            _app.Update(); // Evaluate Context A's changes, which change Context C.
            _app.Update(); // Evaluate Context C's changes, which result in no new changes.

            Assert.IsFalse(_app.Update());
        }

        [Test]
        public void WithChanges_InvokesOnContextChangeOperations()
        {
            // TODO :: Update for multiple behaviors.
            Assert.Ignore();

            TestContextA? contextA = _app.GetContext<TestContextA>();
            if (contextA == null)
                throw new NullReferenceException();

            Validate.TestBehaviorAOnChangeOperationsInvoked(_app, contextA);
        }

        [Test]
        public void WithChanges_RecordsAndInvokesForSubsequentChanges()
        {
            TestContextA? contextA = _app.GetContext<TestContextA>();
            if (contextA == null)
                throw new NullReferenceException();

            TestContextB? contextB = _app.GetContext<TestContextB>();
            if (contextB == null)
                throw new NullReferenceException();

            MethodInfo? mi = typeof(TestBehaviorA).GetMethod("OnContextAIntChange");
            if (mi == null)
                throw new NullReferenceException();

            _app.Evaluator.GetOnChangeOperations(typeof(TestBehaviorA),
                TestBehaviorA.ContextAName, nameof(TestContextA.Int))
                .Returns(new MethodInfo[] { mi });

            mi = typeof(TestBehaviorA).GetMethod("OnContextBIntChange");
            if (mi == null)
                throw new NullReferenceException();

            _app.Evaluator.GetOnChangeOperations(typeof(TestBehaviorA),
                TestBehaviorA.ContextBName, nameof(TestContextB.Int))
                .Returns(new MethodInfo[] { mi });

            int expectedValue = 11;
            contextA.Int.Value = expectedValue;

            _app.Update(); // Behavior A propagates Context A's value to Context C.
            _app.Update(); // Behavior A updates Context C's context change value.

            Assert.AreEqual(expectedValue, contextB.OnContextChangeIntValue);
        }

        [Test]
        public void WithChanges_ReturnsTrue()
        {
            TestContextA? contextA = _app.GetContext<TestContextA>();
            if (contextA == null)
                throw new NullReferenceException();

            contextA.Int.Value = 11;

            Assert.IsTrue(_app.Update());
        }
    }
}
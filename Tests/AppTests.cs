using ContextualProgramming.Internal;
using NSubstitute;
using NUnit.Framework;
using System.Reflection;

namespace AppTests
{
    public static class SetUp
    {
        public static App BehaviorAndContextApp<TBehavior, TContext>(
            IBehaviorFactory factory, Type[]? behaviorUnfulfilledDependencies = null,
            bool initializeApp = true)
        {
            Evaluator evaluator = Substitute.For<Evaluator>();
            App app = new(evaluator);

            evaluator.GetBehaviorTypes().Returns(new Type[]
            {
                typeof(TBehavior)
            });

            PrimeEvaluatorForBehavior<TBehavior>(evaluator, factory,
                behaviorUnfulfilledDependencies);
            PrimeEvaluatorForContext<TContext>(evaluator);

            if (initializeApp)
                app.Initialize();

            return app;
        }

        public static App BehaviorAndContextApp<TBehavior, TContextA, TContextB>(
            IBehaviorFactory factory, Type[]? behaviorUnfulfilledDependencies = null)
        {
            Evaluator evaluator = Substitute.For<Evaluator>();
            App app = new(evaluator);

            evaluator.GetBehaviorTypes().Returns(new Type[]
            {
                typeof(TBehavior)
            });

            PrimeEvaluatorForBehavior<TBehavior>(evaluator, factory,
                behaviorUnfulfilledDependencies);
            PrimeEvaluatorForContext<TContextA>(evaluator);
            PrimeEvaluatorForContext<TContextB>(evaluator);
            app.Initialize();

            return app;
        }

        public static App BehaviorAndContextApp<TBehaviorA, TBehaviorB, TContextA,
            TContextB, TContextC>(
            IBehaviorFactory factoryA, IBehaviorFactory factoryB,
            Type[]? behaviorUnfulfilledDependenciesA = null,
            Type[]? behaviorUnfulfilledDependenciesB = null)
        {
            Evaluator evaluator = Substitute.For<Evaluator>();
            App app = new(evaluator);

            evaluator.GetBehaviorTypes().Returns(new Type[]
            {
                typeof(TBehaviorA),
                typeof(TBehaviorB)
            });

            PrimeEvaluatorForBehavior<TBehaviorA>(evaluator, factoryA,
                behaviorUnfulfilledDependenciesA);
            PrimeEvaluatorForBehavior<TBehaviorB>(evaluator, factoryB,
                behaviorUnfulfilledDependenciesB);
            PrimeEvaluatorForContext<TContextA>(evaluator);
            PrimeEvaluatorForContext<TContextB>(evaluator);
            PrimeEvaluatorForContext<TContextC>(evaluator);
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

            if (Operations.OnTeardown.ContainsKey(type))
                evaluator.GetOnTeardownOperations(type).Returns(new MethodInfo[]
                {
                    Operations.OnTeardown[type]
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

        public static App ContextOnlyApp<TContextA, TContextB>()
        {
            Evaluator evaluator = Substitute.For<Evaluator>();
            App app = new(evaluator);

            evaluator.GetBehaviorTypes().Returns(Array.Empty<Type>());
            PrimeEvaluatorForContext<TContextA>(evaluator);
            PrimeEvaluatorForContext<TContextB>(evaluator);
            app.Initialize();

            return app;
        }

        public static App FullSuiteDependentApp()
        {
            Type[] aDependencies = new Type[]
            {
                    typeof(TestContextA),
                    typeof(TestContextB)
            };
            Type[] bDependencies = new Type[]
            {
                    typeof(TestContextA),
                    typeof(TestContextC)
            };

            IBehaviorFactory factoryA = new BehaviorFactoryDouble<TestBehaviorA>(
                new(), null, aDependencies);
            IBehaviorFactory factoryB = new BehaviorFactoryDouble<TestBehaviorB>(
                new(), null, bDependencies);

            return BehaviorAndContextApp<TestBehaviorA, TestBehaviorB, TestContextA,
                TestContextB, TestContextC>(factoryA, factoryB, aDependencies, bDependencies);
        }

        public static App FullSuiteIndependentApp()
        {
            IBehaviorFactory factoryA = new BehaviorFactoryDouble<TestBehaviorA>(
                new()
                {
                    { TestBehaviorA.ContextAName, new TestContextA() },
                    { TestBehaviorA.ContextBName, new TestContextB() }
                });
            IBehaviorFactory factoryB = new BehaviorFactoryDouble<TestBehaviorB>(
                new()
                {
                    { TestBehaviorB.ContextCName, new TestContextC() }
                },
                null,
                new Type[]
                {
                    typeof(TestContextA)
                });

            return BehaviorAndContextApp<TestBehaviorA, TestBehaviorB, TestContextA,
                TestContextB, TestContextC>(factoryA, factoryB, null,
                new Type[]
                {
                    typeof(TestContextA)
                });
        }

        public static void PrimeEvaluatorForBehavior<TBehavior>(Evaluator evaluator,
            IBehaviorFactory factory, Type[]? behaviorUnfulfilledDependencies = null)
        {
            Type type = typeof(TBehavior);
            Type[] unfulfilledDependencies = behaviorUnfulfilledDependencies ??
                Array.Empty<Type>();

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

        public static readonly Dictionary<Type, MethodInfo> OnTeardown = new();

        static Operations()
        {
            MethodInfo? mi = null;
            mi = typeof(TestBehaviorA).GetMethod(nameof(TestBehaviorA.OnContextAChange));
            if (mi != null)
                OnContextChange.Add(typeof(TestBehaviorA), mi);

            mi = typeof(TestBehaviorA).GetMethod(nameof(TestBehaviorA.OnContextAIntChange));
            if (mi != null)
                OnContextStateChange.Add(typeof(TestBehaviorA), mi);

            mi = typeof(TestBehaviorB).GetMethod(nameof(TestBehaviorB.OnContextAChange));
            if (mi != null)
                OnContextChange.Add(typeof(TestBehaviorB), mi);

            mi = typeof(TestBehaviorB).GetMethod(nameof(TestBehaviorB.OnContextAIntChange));
            if (mi != null)
                OnContextStateChange.Add(typeof(TestBehaviorB), mi);

            mi = typeof(TestBehaviorA).GetMethod(nameof(TestBehaviorA.OnTeardown));
            if (mi != null)
                OnTeardown.Add(typeof(TestBehaviorA), mi);

            mi = typeof(TestBehaviorB).GetMethod(nameof(TestBehaviorB.OnTeardown));
            if (mi != null)
                OnTeardown.Add(typeof(TestBehaviorB), mi);
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
        public List<TBehavior> CreatedInstances { get; init; } = new();

        public bool CanInstantiate => NumberOfPendingInstantiations > 0;

        public int NumberOfPendingInstantiations
        {
            get
            {
                for (int c = 0, count = RequiredDependencyTypes.Count(); c < count; c++)
                {
                    if (!_availableDependencies.ContainsKey(RequiredDependencyTypes[c]))
                        return 0;
                    if (_availableDependencies[RequiredDependencyTypes[c]].Count == 0)
                        return 0;
                }

                return 1;
            }
        }

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
            if (!RequiredDependencyTypes.Contains(type))
                return false;

            if (!_availableDependencies.ContainsKey(type))
                _availableDependencies.Add(type, new());
            _availableDependencies[type].Add(dependency);

            return CanInstantiate;
        }

        public bool HasAvailableDependency(Type type) => _availableDependencies[type].Count > 0;

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

            TBehavior behavior = new();
            CreatedInstances.Add(behavior);

            BehaviorInstance instance = new(behavior, contexts,
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
            contextA.OnContextChangeIntValueFromBehaviorA = contextA.Int;
            contextA.OnContextAChangeFromTestBehaviorACallCount++;
        }

        public void OnContextAIntChange(TestContextA contextA, TestContextB contextB)
        {
            contextA.OnStateChangeIntValueFromBehaviorA = contextA.Int;
            contextB.Int.Value = contextA.Int;
        }

        public void OnContextBIntChange(TestContextB contextB)
        {
            contextB.OnContextChangeIntValue = contextB.Int;
        }

        public void OnTeardown(TestContextA contextA, TestContextB contextB)
        {
            contextB.HasTornDown = true;
            contextA.App?.Decontextualize(contextA);
        }

        public void OnUpdate(TestContextA contextA)
        {
            contextA.UpdateCount++;
        }
    }

    public class TestBehaviorB
    {
        public static readonly string ContextAName = Mappings.ContextNames[typeof(TestContextA)];
        public static readonly string ContextCName = Mappings.ContextNames[typeof(TestContextC)];

        public void OnContextAChange(TestContextA contextA)
        {
            contextA.OnContextChangeIntValueFromBehaviorB = contextA.Int;
        }

        public void OnContextAIntChange(TestContextA contextA)
        {
            contextA.OnStateChangeIntValueFromBehaviorB = contextA.Int;
        }

        public void OnTeardown(TestContextC contextC)
        {
            contextC.HasTornDown = true;
        }
    }


    public class TestContextA
    {
        public App? App { get; set; }

        public int UpdateCount { get; set; } = 0;

        public int OnContextAChangeFromTestBehaviorACallCount { get; set; } = 0;

        public int OnStateChangeIntValueFromBehaviorA { get; set; } = 0;
        public int OnContextChangeIntValueFromBehaviorA { get; set; } = 0;

        public int OnStateChangeIntValueFromBehaviorB { get; set; } = 0;
        public int OnContextChangeIntValueFromBehaviorB { get; set; } = 0;

        public ContextState<int> Int { get; init; } = 0;
    }

    public class TestContextB
    {
        public bool HasTornDown { get; set; } = false;

        public int OnContextChangeIntValue { get; set; } = 0;

        public ContextState<int> Int { get; init; } = 0;
    }

    public class TestContextC
    {
        public bool HasTornDown { get; set; } = false;
    }

    public class TestNonContext { }
    #endregion

    public static class Validate
    {
        public static void TestBehaviorADoesNotExist(App app, TestContextA contextA)
        {
            // Existence is determined by operation invocation.
            TestBehaviorAOnChangeOperationsNotInvoked(app, contextA);
        }

        public static void TestBehaviorAExists(App app, TestContextA contextA)
        {
            // Existence is determined by operation invocation.
            TestBehaviorAOnChangeOperationsInvoked(app, contextA);
        }

        public static void TestBehaviorAOnChangeOperationsInvoked(App app, TestContextA contextA,
            int? existingValueChange = null)
        {
            int expectedValue = TestBehaviorAChangeOperationDependentValue(
                app, contextA, existingValueChange);

            Assert.AreEqual(expectedValue, contextA.OnContextChangeIntValueFromBehaviorA);
            Assert.AreEqual(expectedValue, contextA.OnStateChangeIntValueFromBehaviorA);
        }

        public static void TestBehaviorAOnChangeOperationsNotInvoked(App app,
            TestContextA contextA, int? existingValueChange = null)
        {
            int expectedValue = TestBehaviorAChangeOperationDependentValue(
                app, contextA, existingValueChange);

            Assert.AreNotEqual(expectedValue, contextA.OnContextChangeIntValueFromBehaviorA);
            Assert.AreNotEqual(expectedValue, contextA.OnStateChangeIntValueFromBehaviorA);
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

            app.Update(); // Behavior A updates Context A's context change values.

            return expectedValue;
        }

        public static void TestBehaviorsABDoNotExist(App app, TestContextA contextA)
        {
            // Existence is determined by operation invocation.
            TestBehaviorsABOnChangeOperationsNotInvoked(app, contextA);
        }

        public static void TestBehaviorsABExist(App app, TestContextA contextA)
        {
            // Existence is determined by operation invocation.
            TestBehaviorsABOnChangeOperationsInvoked(app, contextA);
        }

        public static void TestBehaviorsABOnChangeOperationsInvoked(App app, TestContextA contextA,
            int? existingValueChange = null)
        {
            int expectedValue = TestBehaviorsABChangeOperationDependentValue(
                app, contextA, existingValueChange);

            Assert.AreEqual(expectedValue, contextA.OnContextChangeIntValueFromBehaviorA);
            Assert.AreEqual(expectedValue, contextA.OnStateChangeIntValueFromBehaviorA);

            Assert.AreEqual(expectedValue, contextA.OnContextChangeIntValueFromBehaviorB);
            Assert.AreEqual(expectedValue, contextA.OnStateChangeIntValueFromBehaviorB);
        }

        public static void TestBehaviorsABOnChangeOperationsNotInvoked(App app,
            TestContextA contextA, int? existingValueChange = null)
        {
            int expectedValue = TestBehaviorsABChangeOperationDependentValue(
                app, contextA, existingValueChange);

            Assert.AreNotEqual(expectedValue, contextA.OnContextChangeIntValueFromBehaviorA);
            Assert.AreNotEqual(expectedValue, contextA.OnStateChangeIntValueFromBehaviorA);

            Assert.AreNotEqual(expectedValue, contextA.OnContextChangeIntValueFromBehaviorB);
            Assert.AreNotEqual(expectedValue, contextA.OnStateChangeIntValueFromBehaviorB);
        }

        private static int TestBehaviorsABChangeOperationDependentValue(App app,
            TestContextA contextA, int? existingValueChange = null)
        {
            SetUp.BehaviorOperations<TestBehaviorA>(app.Evaluator,
                TestBehaviorA.ContextAName, nameof(TestContextA.Int));
            SetUp.BehaviorOperations<TestBehaviorB>(app.Evaluator,
                TestBehaviorA.ContextAName, nameof(TestContextA.Int));

            int expectedValue;
            if (!existingValueChange.HasValue)
            {
                expectedValue = 11;
                contextA.Int.Value = expectedValue;
            }
            else
                expectedValue = existingValueChange.Value;

            app.Update(); // Behavior A and B update Context A's context change values.

            return expectedValue;
        }


        public static void TestBehaviorBDoesNotExist(App app, TestContextA contextA)
        {
            // Existence is determined by operation invocation.
            TestBehaviorBOnChangeOperationsNotInvoked(app, contextA);
        }

        public static void TestBehaviorBExists(App app, TestContextA contextA)
        {
            // Existence is determined by operation invocation.
            TestBehaviorBOnChangeOperationsInvoked(app, contextA);
        }

        public static void TestBehaviorBOnChangeOperationsInvoked(App app, TestContextA contextA,
            int? existingValueChange = null)
        {
            int expectedValue = TestBehaviorBChangeOperationDependentValue(
                app, contextA, existingValueChange);

            Assert.AreEqual(expectedValue, contextA.OnContextChangeIntValueFromBehaviorB);
            Assert.AreEqual(expectedValue, contextA.OnStateChangeIntValueFromBehaviorB);
        }

        public static void TestBehaviorBOnChangeOperationsNotInvoked(App app,
            TestContextA contextA, int? existingValueChange = null)
        {
            int expectedValue = TestBehaviorBChangeOperationDependentValue(
                app, contextA, existingValueChange);

            Assert.AreNotEqual(expectedValue, contextA.OnContextChangeIntValueFromBehaviorB);
            Assert.AreNotEqual(expectedValue, contextA.OnStateChangeIntValueFromBehaviorB);
        }

        private static int TestBehaviorBChangeOperationDependentValue(App app,
            TestContextA contextA, int? existingValueChange = null)
        {
            SetUp.BehaviorOperations<TestBehaviorB>(app.Evaluator,
                TestBehaviorA.ContextAName, nameof(TestContextA.Int));

            int expectedValue;
            if (!existingValueChange.HasValue)
            {
                expectedValue = 11;
                contextA.Int.Value = expectedValue;
            }
            else
                expectedValue = existingValueChange.Value;

            app.Update(); // Behavior B update Context A's context change values.

            return expectedValue;
        }
    }

    public class AppRetrieval
    {
        [Test]
        public void WithBehaviorInstance_ReturnsApp()
        {
            BehaviorFactoryDouble<TestBehaviorA> factoryA = new(new());
            App expectedAppA = SetUp.BehaviorAndContextApp<TestBehaviorA, TestContextA,
                TestContextB>(factoryA);

            BehaviorFactoryDouble<TestBehaviorA> factoryB = new(new());
            App expectedAppB = SetUp.BehaviorAndContextApp<TestBehaviorA, TestContextA,
                TestContextB>(factoryB);

            IBehaviorApp appA = App.Of(factoryA.CreatedInstances[0]);
            Assert.AreEqual(expectedAppA, appA);

            IBehaviorApp appB = App.Of(factoryB.CreatedInstances[0]);
            Assert.AreEqual(expectedAppB, appB);
        }

        [Test]
        public void WithFormerBehaviorInstance_ThrowsException()
        {
            TestContextA contextA = new();

            BehaviorFactoryDouble<TestBehaviorA> factory = new(new()
            {
                { TestBehaviorA.ContextAName, contextA }
            });
            App app = SetUp.BehaviorAndContextApp<TestBehaviorA, TestContextA,
                TestContextB>(factory);

            app.Decontextualize(contextA);
            app.Update();

            Assert.Throws<InvalidOperationException>(() =>
                App.Of(factory.CreatedInstances[0]));
        }

        [Test]
        public void WithNonBehaviorInstance_ThrowsException()
        {
            BehaviorFactoryDouble<TestBehaviorA> factory = new(new());
            App app = SetUp.BehaviorAndContextApp<TestBehaviorA, TestContextA,
                TestContextB>(factory);

            Assert.Throws<InvalidOperationException>(() =>
                App.Of(new TestBehaviorA()));
        }
    }

    public class Construction
    {
        [Test]
        public void DefaultEvaluator()
        {
            Type expectedEvaluator = typeof(Evaluator<ContextAttribute, MutualismAttribute,
                MutualAttribute, BehaviorAttribute, DependencyAttribute, OperationAttribute>);

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
            app.Update();

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
            App app = SetUp.FullSuiteDependentApp();

            TestContextA contextA = new();
            app.Contextualize(contextA);
            int expectedValue = 11;
            contextA.Int.Value = expectedValue;

            app.Contextualize(new TestContextB());
            app.Contextualize(new TestContextC());

            Validate.TestBehaviorsABOnChangeOperationsInvoked(app, contextA, expectedValue);
        }

        [Test]
        public void DependentFulfilledBehaviors_NewChanges_InvokesOnContextChangeOperations()
        {
            App app = SetUp.FullSuiteDependentApp();

            TestContextA contextA = new();
            app.Contextualize(contextA);

            app.Contextualize(new TestContextB());
            app.Contextualize(new TestContextC());

            Validate.TestBehaviorsABOnChangeOperationsInvoked(app, contextA);
        }

        [Test]
        public void HasMutualism_ConnectsMutualStates()
        {
            string mutualistName = "B";

            IMutualismFulfiller fulfiller = Substitute.For<IMutualismFulfiller>();
            App app = SetUp.ContextOnlyApp<TestContextA, TestContextB>();

            PropertyInfo aInt = typeof(TestContextA).GetProperty(nameof(TestContextA.Int)) ??
                throw new NullReferenceException();
            PropertyInfo bInt = typeof(TestContextB).GetProperty(nameof(TestContextB.Int)) ??
                throw new NullReferenceException();

            TestContextA contextA = new();
            TestContextB contextB = new();
            app.Evaluator.BuildMutualismFulfiller(typeof(TestContextA)).Returns(fulfiller);
            app.Evaluator.GetMutualStateInfos(typeof(TestContextA)).Returns(
                new MutualStateInfo[]
                {
                    new(aInt, mutualistName, bInt)
                });
            fulfiller.Fulfill(contextA).Returns(new Tuple<string, object>[]
            {
                new(mutualistName, contextB)
            });

            app.Contextualize(contextA);

            int expected = 1;
            contextA.Int.Value = expected;

            Assert.AreEqual(expected, contextB.Int.Value);
        }

        [Test]
        public void HasMutualism_ContextualizesMutualistContext()
        {
            IMutualismFulfiller fulfiller = Substitute.For<IMutualismFulfiller>();
            App app = SetUp.ContextOnlyApp<TestContextA, TestContextB>();

            TestContextA contextA = new();
            TestContextB expectedMutualistContext = new();
            app.Evaluator.BuildMutualismFulfiller(typeof(TestContextA)).Returns(fulfiller);
            fulfiller.Fulfill(contextA).Returns(new Tuple<string, object>[]
            {
                new("B", expectedMutualistContext)
            });

            app.Contextualize(contextA);

            Assert.AreEqual(expectedMutualistContext, app.GetContext<TestContextB>());
        }

        [Test]
        public void HasMutualism_OverridesMutualistMutualState()
        {
            string mutualistName = "B";

            IMutualismFulfiller fulfiller = Substitute.For<IMutualismFulfiller>();
            App app = SetUp.ContextOnlyApp<TestContextA, TestContextB>();

            PropertyInfo aInt = typeof(TestContextA).GetProperty(nameof(TestContextA.Int)) ??
                throw new NullReferenceException();
            PropertyInfo bInt = typeof(TestContextB).GetProperty(nameof(TestContextB.Int)) ??
                throw new NullReferenceException();

            TestContextA contextA = new();
            TestContextB contextB = new();
            app.Evaluator.BuildMutualismFulfiller(typeof(TestContextA)).Returns(fulfiller);
            app.Evaluator.GetMutualStateInfos(typeof(TestContextA)).Returns(
                new MutualStateInfo[]
                {
                    new(aInt, mutualistName, bInt)
                });
            fulfiller.Fulfill(contextA).Returns(new Tuple<string, object>[]
            {
                new(mutualistName, contextB)
            });

            int expected = 1;
            contextA.Int.Value = expected;

            app.Contextualize(contextA);

            Assert.AreEqual(expected, contextB.Int.Value);
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
            _app = AppTests.SetUp.FullSuiteIndependentApp();
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

            Validate.TestBehaviorADoesNotExist(app, contextA);
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

            app.Update();

            app.Decontextualize(contextB);

            app.Contextualize(new TestContextB());
            app.Update();

            Validate.TestBehaviorAExists(app, contextA);
        }

        [Test]
        public void DependentBehaviors_DecontextualizeA_ExistingChangesIgnoredForOnContextChange()
        {
            TestContextA? contextA = _app.GetContext<TestContextA>() ??
                throw new NullReferenceException();

            int expectedValue = 11;
            contextA.Int.Value = expectedValue;
            _app.Decontextualize(contextA);

            Validate.TestBehaviorAOnChangeOperationsNotInvoked(_app, contextA, expectedValue);
        }

        [Test]
        public void DependentBehaviors_DecontextualizeA_NewChangesIgnoredForOnContextChange()
        {
            TestContextA? contextA = _app.GetContext<TestContextA>() ??
                throw new NullReferenceException();

            _app.Decontextualize(contextA);
            Validate.TestBehaviorAOnChangeOperationsNotInvoked(_app, contextA);
        }

        [Test]
        public void DependentBehaviors_DecontextualizeB_ExistingChangesIgnoredForOnContextChange()
        {
            TestContextA? contextA = _app.GetContext<TestContextA>() ??
                throw new NullReferenceException();
            TestContextB? contextB = _app.GetContext<TestContextB>() ??
                throw new NullReferenceException();

            int expectedValue = 11;
            contextA.Int.Value = expectedValue;
            _app.Decontextualize(contextB);

            Validate.TestBehaviorAOnChangeOperationsNotInvoked(_app, contextA, expectedValue);
        }

        [Test]
        public void DependentBehaviors_DecontextualizeB_NewChangesIgnoredForOnContextChange()
        {
            TestContextA? contextA = _app.GetContext<TestContextA>() ??
                throw new NullReferenceException();
            TestContextB? contextB = _app.GetContext<TestContextB>() ??
                throw new NullReferenceException();

            _app.Decontextualize(contextB);
            Validate.TestBehaviorAOnChangeOperationsNotInvoked(_app, contextA);
        }

        [Test]
        public void DependentBehaviors_PreventsFulfillment()
        {
            App app = AppTests.SetUp.FullSuiteDependentApp();

            TestContextB contextB = new();
            app.Contextualize(contextB);

            app.Decontextualize(contextB);

            TestContextC contextC = new();
            app.Contextualize(contextC);

            app.Decontextualize(contextC);

            TestContextA contextA = new();
            app.Contextualize(contextA);

            Validate.TestBehaviorsABDoNotExist(app, contextA);
        }

        [Test]
        public void DependentBehaviors_Recontextualized_DoesNotInvokeOperationsMultipleTimes()
        {
            AppTests.SetUp.BehaviorOperations<TestBehaviorA>(_app.Evaluator,
                TestBehaviorA.ContextAName, nameof(TestContextA.Int));
            AppTests.SetUp.BehaviorOperations<TestBehaviorB>(_app.Evaluator,
                TestBehaviorA.ContextAName, nameof(TestContextA.Int));

            TestContextA? contextA = _app.GetContext<TestContextA>() ??
                throw new NullReferenceException();

            _app.Decontextualize(contextA);
            _app.Contextualize(contextA);

            contextA.Int.Value = 1;
            _app.Update();

            Assert.AreEqual(1, contextA.OnContextAChangeFromTestBehaviorACallCount);
        }

        [Test]
        public void DependentBehaviors_RecontextualizedBeforeUpdate_BehaviorsAreMaintained()
        {
            TestContextA? contextA = _app.GetContext<TestContextA>() ??
                throw new NullReferenceException();

            _app.Decontextualize(contextA);
            _app.Contextualize(contextA);

            Validate.TestBehaviorsABExist(_app, contextA);
        }

        [Test]
        public void DependentBehaviors_RecontextualizedBeforeUpdate_FactoryHasNoExtraContext()
        {
            Type[] requiredDependencies = new Type[]
            {
                typeof(TestContextA),
                typeof(TestContextB)
            };

            var factory = new BehaviorFactoryDouble<TestBehaviorA>(
                new(), new(), requiredDependencies);
            App app = AppTests.SetUp.BehaviorAndContextApp<TestBehaviorA, TestContextA,
                TestContextB>(factory, requiredDependencies);

            TestContextA contextA = new();
            app.Contextualize(contextA);
            app.Contextualize(new TestContextB());
            app.Update();

            app.Decontextualize(contextA);
            app.Contextualize(contextA);

            Assert.IsFalse(factory.HasAvailableDependency(typeof(TestContextA)));
        }

        [Test]
        public void DependentBehaviors_RemainingContextsReusedForNewBehaviorInstancesWithExisting()
        {
            App app = AppTests.SetUp.FullSuiteDependentApp();

            TestContextA originalContextA = new();
            app.Contextualize(originalContextA);

            TestContextB contextB = new();
            app.Contextualize(contextB);

            TestContextC contextC = new();
            app.Contextualize(contextC);

            app.Update();

            TestContextA finalContextA = new();
            app.Contextualize(finalContextA);

            app.Decontextualize(originalContextA);

            app.Update();

            Validate.TestBehaviorsABExist(app, finalContextA);
        }

        [Test]
        public void DependentBehaviors_RemainingContextsReusedForNewBehaviorInstances_WithNew()
        {
            App app = AppTests.SetUp.FullSuiteDependentApp();

            TestContextA originalContextA = new();
            app.Contextualize(originalContextA);

            TestContextB contextB = new();
            app.Contextualize(contextB);

            TestContextC contextC = new();
            app.Contextualize(contextC);

            app.Update();

            app.Decontextualize(originalContextA);

            TestContextA finalContextA = new();
            app.Contextualize(finalContextA);
            app.Update();

            Validate.TestBehaviorsABExist(app, finalContextA);
        }

        [Test]
        public void DependentInitializationBehavior_DecontextualizeA_NewChangesIgnoredForOnContextChange()
        {
            TestContextA? contextA = _app.GetContext<TestContextA>() ??
                throw new NullReferenceException();

            _app.Decontextualize(contextA);
            Validate.TestBehaviorAOnChangeOperationsNotInvoked(_app, contextA);
        }

        [Test]
        public void DependentInitializationBehavior_DecontextualizeB_NewChangesIgnoredForOnContextChange()
        {
            TestContextA? contextA = _app.GetContext<TestContextA>() ??
                throw new NullReferenceException();

            TestContextB? contextB = _app.GetContext<TestContextB>() ??
                throw new NullReferenceException();

            _app.Decontextualize(contextB);
            Validate.TestBehaviorAOnChangeOperationsNotInvoked(_app, contextA);
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
        public void HasMutualism_DecontextualizesMutualistContext()
        {
            IMutualismFulfiller fulfiller = Substitute.For<IMutualismFulfiller>();
            App app = AppTests.SetUp.ContextOnlyApp<TestContextA, TestContextB>();

            TestContextA contextA = new();
            app.Evaluator.BuildMutualismFulfiller(typeof(TestContextA)).Returns(fulfiller);
            fulfiller.Fulfill(contextA).Returns(new Tuple<string, object>[]
            {
                new("B", new TestContextB())
            });

            app.Contextualize(contextA);
            app.Decontextualize(contextA);

            Assert.IsNull(app.GetContext<TestContextB>());
        }

        [Test]
        public void IsMutualistContext_DecontextualizesHostContext()
        {
            IMutualismFulfiller fulfiller = Substitute.For<IMutualismFulfiller>();
            App app = AppTests.SetUp.ContextOnlyApp<TestContextA, TestContextB>();

            TestContextA contextA = new();
            TestContextB contextB = new();
            app.Evaluator.BuildMutualismFulfiller(typeof(TestContextA)).Returns(fulfiller);
            fulfiller.Fulfill(contextA).Returns(new Tuple<string, object>[]
            {
                new("B", contextB)
            });

            app.Contextualize(contextA);
            app.Decontextualize(contextB);

            Assert.IsNull(app.GetContext<TestContextA>());
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
            _app.Update();

            _app.Decontextualize(context);
            _app.Update();

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
        public class InitializationBehavior_CallsAppInConstructor
        {
            public static App? App { get; set; }

            public InitializationBehavior_CallsAppInConstructor()
            {
                App?.GetContext<TestContextA>();
            }
        }

        [Test]
        public void FulfilledBehaviors_ContextualizeSelfCreatedContextsAfterUpdate()
        {
            App app = SetUp.FullSuiteIndependentApp();

            app.Update();

            Assert.NotNull(app.GetContext<TestContextC>());
        }

        [Test]
        public void InitializationBehavior_CallsAppInConstructor_DoesNotThrowException()
        {
            BehaviorFactoryDouble<InitializationBehavior_CallsAppInConstructor> factory = new(
                new()
                {
                    { "contextA", new TestContextA() }
                });
            App app = SetUp.BehaviorAndContextApp<InitializationBehavior_CallsAppInConstructor,
                TestContextA>(factory, null, false);

            InitializationBehavior_CallsAppInConstructor.App = app;

            Assert.DoesNotThrow(() => app.Initialize());

            InitializationBehavior_CallsAppInConstructor.App = null;
        }

        [Test]
        public void InitializationBehavior_ContextualizesSelfCreatedContexts()
        {
            App app = SetUp.FullSuiteIndependentApp();

            Assert.NotNull(app.GetContext<TestContextA>());
        }

        [Test]
        public void InitializationBehavior_NewChanges_InvokeOnContextChangeOperations()
        {
            App app = SetUp.FullSuiteIndependentApp();

            TestContextA? contextA = app.GetContext<TestContextA>();
            if (contextA == null)
                throw new NullReferenceException();

            Validate.TestBehaviorAOnChangeOperationsInvoked(app, contextA);
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
            _app = AppTests.SetUp.FullSuiteIndependentApp();
        }

        [Test]
        public void FulfilledBehaviors_InstantiatedAfterUpdate()
        {
            TestContextA? contextA = _app.GetContext<TestContextA>() ??
                throw new NullReferenceException();

            Assert.IsNull(_app.GetContext<TestContextC>());

            _app.Update();

            Assert.IsNotNull(_app.GetContext<TestContextC>());
            Assert.AreEqual(0, contextA.OnContextChangeIntValueFromBehaviorA);
        }

        [Test]
        public void NoChanges_DoesNotInvokeContextChangeOperations()
        {
            TestContextA? contextA = _app.GetContext<TestContextA>() ??
                throw new NullReferenceException();

            _app.Update();

            Assert.AreEqual(0, contextA.OnContextChangeIntValueFromBehaviorA);
        }

        [Test]
        public void NoChanges_DoesNotInvokeStateChangeOperations()
        {
            TestContextA? contextA = _app.GetContext<TestContextA>() ??
                throw new NullReferenceException();

            _app.Update();

            Assert.AreEqual(0, contextA.OnStateChangeIntValueFromBehaviorA);
        }

        [Test]
        public void NoChangesAndNoPendingFactoriesAndNoDeregisteringBehaviors_ReturnsFalse()
        {
            _app.Update();

            Assert.IsFalse(_app.Update());
        }

        [Test]
        public void WithChanges_ClearsPreviousChanges()
        {
            TestContextA? contextA = _app.GetContext<TestContextA>() ??
                throw new NullReferenceException();

            contextA.Int.Value = 11;

            _app.Update(); // Evaluate Context A's changes, which change Context C.
            _app.Update(); // Evaluate Context C's changes, which result in no new changes.

            Assert.IsFalse(_app.Update());
        }

        [Test]
        public void WithChanges_InvokesOnContextChangeOperations()
        {
            TestContextA? contextA = _app.GetContext<TestContextA>() ??
                throw new NullReferenceException();

            Validate.TestBehaviorsABOnChangeOperationsInvoked(_app, contextA);
        }

        [Test]
        public void WithChanges_RecordsAndInvokesForSubsequentChanges()
        {
            TestContextA? contextA = _app.GetContext<TestContextA>() ??
                throw new NullReferenceException();

            TestContextB? contextB = _app.GetContext<TestContextB>() ??
                throw new NullReferenceException();

            MethodInfo? mi = typeof(TestBehaviorA).GetMethod(
                nameof(TestBehaviorA.OnContextAIntChange)) ?? throw new NullReferenceException();

            _app.Evaluator.GetOnChangeOperations(typeof(TestBehaviorA),
                TestBehaviorA.ContextAName, nameof(TestContextA.Int))
                .Returns(new MethodInfo[] { mi });

            mi = typeof(TestBehaviorA).GetMethod(
                nameof(TestBehaviorA.OnContextBIntChange)) ?? throw new NullReferenceException();

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
            TestContextA? contextA = _app.GetContext<TestContextA>() ??
                throw new NullReferenceException();

            _app.Update();

            contextA.Int.Value = 11;

            Assert.IsTrue(_app.Update());
        }

        [Test]
        public void WithDeregisteringBehaviors_AfterUpdate_ReturnsFalse()
        {
            TestContextA? contextA = _app.GetContext<TestContextA>() ??
                throw new NullReferenceException();

            _app.Update();

            _app.Decontextualize(contextA);

            _app.Update();

            Assert.IsFalse(_app.Update());
        }

        [Test]
        public void WithDeregisteringBehaviors_BeforeUpdate_ReturnsTrue()
        {
            TestContextA? contextA = _app.GetContext<TestContextA>() ??
                throw new NullReferenceException();

            _app.Update();

            _app.Decontextualize(contextA);

            Assert.IsTrue(_app.Update());
        }

        [Test]
        public void WithDeregisteringBehaviors_InvokesTeardownOperations()
        {
            _app.Update();

            TestContextA? contextA = _app.GetContext<TestContextA>() ??
                throw new NullReferenceException();
            TestContextB? contextB = _app.GetContext<TestContextB>() ??
                throw new NullReferenceException();
            TestContextC? contextC = _app.GetContext<TestContextC>() ??
                throw new NullReferenceException();

            AppTests.SetUp.BehaviorOperations<TestBehaviorA>(_app.Evaluator,
                TestBehaviorA.ContextAName, nameof(TestContextA.Int));
            AppTests.SetUp.BehaviorOperations<TestBehaviorB>(_app.Evaluator,
                TestBehaviorA.ContextAName, nameof(TestContextA.Int));

            _app.Decontextualize(contextA);

            _app.Update();

            Assert.IsTrue(contextB.HasTornDown);
            Assert.IsTrue(contextC.HasTornDown);
        }

        [Test]
        public void WithDeregisteringBehaviors_RecordsAndInvokesForSubsequentDeregistrations()
        {
            _app.Update();

            TestContextA? contextA = _app.GetContext<TestContextA>() ??
                throw new NullReferenceException();
            TestContextB? contextB = _app.GetContext<TestContextB>() ??
                throw new NullReferenceException();
            TestContextC? contextC = _app.GetContext<TestContextC>() ??
                throw new NullReferenceException();

            AppTests.SetUp.BehaviorOperations<TestBehaviorA>(_app.Evaluator,
                TestBehaviorA.ContextAName, nameof(TestContextA.Int));
            AppTests.SetUp.BehaviorOperations<TestBehaviorB>(_app.Evaluator,
                TestBehaviorA.ContextAName, nameof(TestContextA.Int));

            contextA.App = _app; // Set app so TestBehaviorA teardown decontextualizes contextA.

            _app.Decontextualize(contextB);
            _app.Update(); // Deregisters only TestBehaviorA for contextB decontextualization.

            Assert.IsTrue(contextB.HasTornDown);
            Assert.IsFalse(contextC.HasTornDown);

            _app.Update(); // Deregisters TestBehaviorB for contextA decontextualization.

            Assert.IsTrue(contextB.HasTornDown);
            Assert.IsTrue(contextC.HasTornDown);
        }

        [Test]
        public void WithMutualChanges_InvokesOnContextChangeOperations()
        {
            string mutualistName = "B";

            TestContextA contextA = new();
            TestContextB contextB = new();
            Type[] requiredContextTypes = new Type[]
            {
                typeof(TestContextA),
                typeof(TestContextB)
            };

            IMutualismFulfiller fulfiller = Substitute.For<IMutualismFulfiller>();
            BehaviorFactoryDouble<TestBehaviorA> factoryA = new(new(), new(), 
                requiredContextTypes);
            App app = AppTests.SetUp.BehaviorAndContextApp<TestBehaviorA,
                TestContextA, TestContextB>(factoryA, requiredContextTypes);

            PropertyInfo aInt = typeof(TestContextA).GetProperty(nameof(TestContextA.Int)) ??
                throw new NullReferenceException();
            PropertyInfo bInt = typeof(TestContextB).GetProperty(nameof(TestContextB.Int)) ??
                throw new NullReferenceException();

            app.Evaluator.BuildMutualismFulfiller(typeof(TestContextA)).Returns(fulfiller);
            app.Evaluator.GetMutualStateInfos(typeof(TestContextA)).Returns(
                new MutualStateInfo[]
                {
                    new(aInt, mutualistName, bInt)
                });
            fulfiller.Fulfill(contextA).Returns(new Tuple<string, object>[]
            {
                new(mutualistName, contextB)
            });

            app.Contextualize(contextA); // Results in contextualization of contextB.
            app.Update(); // Results in creation of TestBehaviorA.

            MethodInfo? mi = typeof(TestBehaviorA).GetMethod(
                nameof(TestBehaviorA.OnContextAIntChange)) ??
                throw new NullReferenceException();

            app.Evaluator.GetOnChangeOperations(typeof(TestBehaviorA),
                TestBehaviorA.ContextAName, nameof(TestContextA.Int))
                .Returns(new MethodInfo[] { mi });

            mi = typeof(TestBehaviorA).GetMethod(nameof(TestBehaviorA.OnContextBIntChange)) ??
                throw new NullReferenceException();

            app.Evaluator.GetOnChangeOperations(typeof(TestBehaviorA),
                TestBehaviorA.ContextBName, nameof(TestContextB.Int))
                .Returns(new MethodInfo[] { mi });

            int expectedValue = 11;
            contextA.Int.Value = expectedValue;

            app.Update(); // Behavior A updates for the mutual change to both contexts.

            Assert.AreEqual(expectedValue, contextA.OnStateChangeIntValueFromBehaviorA);
            Assert.AreEqual(expectedValue, contextB.OnContextChangeIntValue);
        }

        [Test]
        public void WithPendingFactories_ReturnsTrue()
        {
            Assert.IsTrue(_app.Update());
        }

        [Test]
        public void WithUpdateOperations_AfterTeardown_DoesNotInvokeUpdateOperations()
        {
            App app = AppTests.SetUp.FullSuiteDependentApp();
            TestContextA contextA = new();
            TestContextB contextB = new();

            MethodInfo? mi = typeof(TestBehaviorA).GetMethod(
                nameof(TestBehaviorA.OnUpdate)) ?? throw new NullReferenceException();
            app.Evaluator.GetOnUpdateOperations(typeof(TestBehaviorA))
                .Returns(new MethodInfo[] { mi });

            app.Contextualize(contextA);
            app.Contextualize(contextB);

            app.Update();  // Instantiate the behaviors after contextualization.

            app.Decontextualize(contextB);

            app.Update();  // Should run no update operations.

            Assert.AreEqual(0, contextA.UpdateCount);
        }

        [Test]
        public void WithUpdateOperations_BeforeTeardown_InvokesUpdateOperations()
        {
            App app = AppTests.SetUp.FullSuiteDependentApp();
            TestContextA contextA = new();
            TestContextB contextB = new();

            MethodInfo? mi = typeof(TestBehaviorA).GetMethod(
                nameof(TestBehaviorA.OnUpdate)) ?? throw new NullReferenceException();
            app.Evaluator.GetOnUpdateOperations(typeof(TestBehaviorA))
                .Returns(new MethodInfo[] { mi });

            app.Contextualize(contextA);
            app.Contextualize(contextB);

            app.Update();  // Instantiate the behaviors after contextualization.

            app.Update();  // First run of update operations.

            Assert.AreEqual(1, contextA.UpdateCount);

            app.Update();  // Second run of update operations.

            Assert.AreEqual(2, contextA.UpdateCount);
        }
    }
}
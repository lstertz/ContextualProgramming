using ContextualProgramming.Internal;
using NSubstitute;
using NUnit.Framework;
using System.Reflection;

namespace AppTests
{
    public static class Setup
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

        public static App ContextOnlyApp<TContext>()
        {
            Evaluator evaluator = Substitute.For<Evaluator>();
            App app = new(evaluator);

            evaluator.GetBehaviorTypes().Returns(Array.Empty<Type>());
            PrimeEvaluatorForContext<TContext>(evaluator);
            app.Initialize();

            return app;
        }

        public static IBehaviorFactory BuildFactorySubstitute<TBehavior>(
            Dictionary<string, object> selfCreatedContexts, 
            Dictionary<string, object>? existingContexts = null, 
            Type[]? behaviorUnfulfilledDependencies = null) where TBehavior : new()
        {
            Dictionary<string, object> allContexts = new();
            selfCreatedContexts.ToList().ForEach(kvp => allContexts.Add(kvp.Key, kvp.Value));
            if (existingContexts != null)
                existingContexts.ToList().ForEach(kvp => allContexts.Add(kvp.Key, kvp.Value));

            Type[] unfulfilledDependencies = behaviorUnfulfilledDependencies ?? 
                Array.Empty<Type>();

            IBehaviorFactory factory = Substitute.For<IBehaviorFactory>();
            factory.RequiredDependencyTypes.Returns(unfulfilledDependencies);
            factory.CanInstantiate.Returns(unfulfilledDependencies.Length == 0);
            factory.NumberOfPendingInstantiations.Returns(
                unfulfilledDependencies.Length > 0 ? 0 : -1);
            if (unfulfilledDependencies.Length == 0)
            {
                BehaviorInstance bi = new(
                    new TBehavior(),
                    allContexts, 
                    selfCreatedContexts.Values.ToArray());
                factory.Process().Returns(new BehaviorInstance[] { bi });
            }
            else
                factory.Process().Returns(Array.Empty<BehaviorInstance>());

            return factory;
        }

        public static void PrimeEvaluatorForBehavior<TBehavior>(Evaluator evaluator,
            IBehaviorFactory factory, Type[]? behaviorUnfulfilledDependencies = null)
        {
            Type type = typeof(TBehavior);
            Type[] unfulfilledDependencies = behaviorUnfulfilledDependencies == null ?
                Array.Empty<Type>() : behaviorUnfulfilledDependencies;


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
    public class TestBehaviorA
    {
        public const string ContextAName = "contextA";
        public const string ContextBName = "contextB";

        private void OnContextAIntChange(TestContextA contextA, TestContextB contextB)
        {
            contextA.OnStateChangeIntValue = contextA.Int;
            contextB.Int.Value = contextA.Int;
        }

        public void OnContextAChange(TestContextA contextA)
        {
            contextA.OnContextChangeIntValue = contextA.Int;
        }

        public void OnContextBIntChange(TestContextB contextB)
        {
            contextB.OnContextChangeIntValue = contextB.Int;
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

    public class TestNonContext { }
    #endregion

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

    public class BehaviorHandling
    {
        [Test]
        public void FulfilledBehaviorContextualizesSelfCreatedDependencies()
        {
            Assert.Ignore();
        }

        [Test]
        public void DecontextualizationPreventsFulfillment()
        {
            Assert.Ignore();
        }

        [Test]
        public void DestroyedBehaviorsRemainingDependenciesUsedForInstantiation()
        {
            Assert.Ignore();
        }
    }

    public class Contextualization
    {
        [Test]
        public void BindsForChanges()
        {
            IBehaviorFactory factory = Setup.BuildFactorySubstitute<TestBehaviorA>(new());
            App app = Setup.BehaviorAndContextApp<TestBehaviorA, TestContextA,
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
            IBehaviorFactory factory = Setup.BuildFactorySubstitute<TestBehaviorA>(new());
            App app = Setup.BehaviorAndContextApp<TestBehaviorA, TestContextA,
                TestContextB>(factory);

            TestContextA expectedContext = new();
            app.Contextualize(expectedContext);

            Assert.AreEqual(expectedContext, app.GetContext<TestContextA>());
        }

        [Test]
        public void DependentFulfilledBehavior_NewChangesInvokeOnContextChange()
        {
            Assert.Ignore();

            IBehaviorFactory factory = Setup.BuildFactorySubstitute<TestBehaviorA>(
                new(), new(), new Type[] 
                {
                    typeof(TestContextA), 
                    typeof(TestContextB)
                });
            App app = Setup.BehaviorAndContextApp<TestBehaviorA, TestContextA,
                TestContextB>(factory);

            MethodInfo? mi = typeof(TestBehaviorA).GetMethod("OnContextAChange");
            if (mi == null)
                throw new NullReferenceException();

            app.Evaluator.GetOnChangeOperations(typeof(TestBehaviorA),
                TestBehaviorA.ContextAName).Returns(new MethodInfo[] { mi });

            TestContextA contextA = new();
            app.Contextualize(contextA);
            int expectedValue = 11;
            contextA.Int.Value = expectedValue;

            app.Update(); // Behavior A updates Context A's context change value.

            Assert.AreEqual(expectedValue, contextA.OnContextChangeIntValue);
        }

        [Test]
        public void DependentFulfilledBehavior_NewChangesInvokeOnStateChange()
        {
            Assert.Ignore();

            IBehaviorFactory factory = Setup.BuildFactorySubstitute<TestBehaviorA>(
                new(), new(), new Type[]
                {
                    typeof(TestContextA),
                    typeof(TestContextB)
                });
            App app = Setup.BehaviorAndContextApp<TestBehaviorA, TestContextA,
                TestContextB>(factory);

            MethodInfo? mi = typeof(TestBehaviorA).GetMethod("OnContextAIntChange",
                BindingFlags.Instance | BindingFlags.NonPublic);
            if (mi == null)
                throw new NullReferenceException();

            app.Evaluator.GetOnChangeOperations(typeof(TestBehaviorA),
                TestBehaviorA.ContextAName, nameof(TestContextA.Int))
                .Returns(new MethodInfo[] { mi });

            TestContextA contextA = new();
            app.Contextualize(contextA);
            int expectedValue = 11;
            contextA.Int.Value = expectedValue;

            app.Update(); // Behavior A updates Context A's context change value.

            Assert.AreEqual(expectedValue, contextA.OnStateChangeIntValue);
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
            App app = Setup.ContextOnlyApp<TestContextA>();

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
            IBehaviorFactory factory = Setup.BuildFactorySubstitute<TestBehaviorA>(
                new()
                {
                    { TestBehaviorA.ContextAName, new TestContextA() },
                    { TestBehaviorA.ContextBName, new TestContextB() }
                });
            _app = Setup.BehaviorAndContextApp<TestBehaviorA, TestContextA,
                TestContextB>(factory);
        }

        [Test]
        public void Decontextualizes()
        {
            _app.Decontextualize(_app.GetContext<TestContextA>());

            Assert.IsNull(_app.GetContext<TestContextA>());
        }

        [Test]
        public void DependentFulfilledBehavior_ExistingChangesIgnoredForOnContextChange()
        {
            Assert.Ignore();

            TestContextA? contextA = _app.GetContext<TestContextA>();
            if (contextA == null)
                throw new NullReferenceException();

            MethodInfo? mi = typeof(TestBehaviorA).GetMethod("OnContextAChange");
            if (mi == null)
                throw new NullReferenceException();

            _app.Evaluator.GetOnChangeOperations(typeof(TestBehaviorA),
                TestBehaviorA.ContextAName).Returns(new MethodInfo[] { mi });

            int expectedValue = 11;
            contextA.Int.Value = expectedValue;
            _app.Decontextualize(contextA);

            _app.Update(); // Behavior A would update Context A's context change value.

            Assert.AreNotEqual(expectedValue, contextA.OnContextChangeIntValue);
        }

        [Test]
        public void DependentFulfilledBehavior_ExistingChangesIgnoredForOnStateChange()
        {
            Assert.Ignore();

            TestContextA? contextA = _app.GetContext<TestContextA>();
            if (contextA == null)
                throw new NullReferenceException();

            MethodInfo? mi = typeof(TestBehaviorA).GetMethod("OnContextAIntChange",
                BindingFlags.Instance | BindingFlags.NonPublic);
            if (mi == null)
                throw new NullReferenceException();

            _app.Evaluator.GetOnChangeOperations(typeof(TestBehaviorA),
                TestBehaviorA.ContextAName, nameof(TestContextA.Int))
                .Returns(new MethodInfo[] { mi });

            int expectedValue = 11;
            contextA.Int.Value = expectedValue;
            _app.Decontextualize(contextA);

            _app.Update(); // Behavior A would update Context A's context change value.

            Assert.AreNotEqual(expectedValue, contextA.OnStateChangeIntValue);
        }

        [Test]
        public void DependentFulfilledBehavior_NewChangesIgnoredForOnContextChange()
        {
            Assert.Ignore();

            TestContextA? contextA = _app.GetContext<TestContextA>();
            if (contextA == null)
                throw new NullReferenceException();

            MethodInfo? mi = typeof(TestBehaviorA).GetMethod("OnContextAChange");
            if (mi == null)
                throw new NullReferenceException();

            _app.Evaluator.GetOnChangeOperations(typeof(TestBehaviorA),
                TestBehaviorA.ContextAName).Returns(new MethodInfo[] { mi });

            _app.Decontextualize(contextA);
            int expectedValue = 11;
            contextA.Int.Value = expectedValue;

            _app.Update(); // Behavior A would update Context A's context change value.

            Assert.AreNotEqual(expectedValue, contextA.OnContextChangeIntValue);
        }

        [Test]
        public void DependentFulfilledBehavior_NewChangesIgnoredForOnStateChange()
        {
            Assert.Ignore();

            TestContextA? contextA = _app.GetContext<TestContextA>();
            if (contextA == null)
                throw new NullReferenceException();

            MethodInfo? mi = typeof(TestBehaviorA).GetMethod("OnContextAIntChange",
                BindingFlags.Instance | BindingFlags.NonPublic);
            if (mi == null)
                throw new NullReferenceException();

            _app.Evaluator.GetOnChangeOperations(typeof(TestBehaviorA),
                TestBehaviorA.ContextAName, nameof(TestContextA.Int))
                .Returns(new MethodInfo[] { mi });

            _app.Decontextualize(contextA);
            int expectedValue = 11;
            contextA.Int.Value = expectedValue;

            _app.Update(); // Behavior A would update Context A's context change value.

            Assert.AreNotEqual(expectedValue, contextA.OnStateChangeIntValue);
        }

        [Test]
        public void DependentInitializationBehavior_ExistingChangesIgnoredForOnContextChange()
        {
            TestContextA? contextA = _app.GetContext<TestContextA>();
            if (contextA == null)
                throw new NullReferenceException();

            MethodInfo? mi = typeof(TestBehaviorA).GetMethod("OnContextAChange");
            if (mi == null)
                throw new NullReferenceException();

            _app.Evaluator.GetOnChangeOperations(typeof(TestBehaviorA),
                TestBehaviorA.ContextAName).Returns(new MethodInfo[] { mi });

            int expectedValue = 11;
            contextA.Int.Value = expectedValue;
            _app.Decontextualize(contextA);

            _app.Update(); // Behavior A would update Context A's context change value.

            Assert.AreNotEqual(expectedValue, contextA.OnContextChangeIntValue);
        }

        [Test]
        public void DependentInitializationBehavior_ExistingChangesIgnoredForOnStateChange()
        {
            TestContextA? contextA = _app.GetContext<TestContextA>();
            if (contextA == null)
                throw new NullReferenceException();

            MethodInfo? mi = typeof(TestBehaviorA).GetMethod("OnContextAIntChange",
                BindingFlags.Instance | BindingFlags.NonPublic);
            if (mi == null)
                throw new NullReferenceException();

            _app.Evaluator.GetOnChangeOperations(typeof(TestBehaviorA),
                TestBehaviorA.ContextAName, nameof(TestContextA.Int))
                .Returns(new MethodInfo[] { mi });

            int expectedValue = 11;
            contextA.Int.Value = expectedValue;
            _app.Decontextualize(contextA);

            _app.Update(); // Behavior A would update Context A's context change value.

            Assert.AreNotEqual(expectedValue, contextA.OnStateChangeIntValue);
        }

        [Test]
        public void DependentInitializationBehavior_NewChangesIgnoredForOnContextChange()
        {
            TestContextA? contextA = _app.GetContext<TestContextA>();
            if (contextA == null)
                throw new NullReferenceException();

            MethodInfo? mi = typeof(TestBehaviorA).GetMethod("OnContextAChange");
            if (mi == null)
                throw new NullReferenceException();

            _app.Evaluator.GetOnChangeOperations(typeof(TestBehaviorA),
                TestBehaviorA.ContextAName).Returns(new MethodInfo[] { mi });

            _app.Decontextualize(contextA);
            int expectedValue = 11;
            contextA.Int.Value = expectedValue;

            _app.Update(); // Behavior A would update Context A's context change value.

            Assert.AreNotEqual(expectedValue, contextA.OnContextChangeIntValue);
        }

        [Test]
        public void DependentInitializationBehavior_NewChangesIgnoredForOnStateChange()
        {
            TestContextA? contextA = _app.GetContext<TestContextA>();
            if (contextA == null)
                throw new NullReferenceException();

            MethodInfo? mi = typeof(TestBehaviorA).GetMethod("OnContextAIntChange",
                BindingFlags.Instance | BindingFlags.NonPublic);
            if (mi == null)
                throw new NullReferenceException();

            _app.Evaluator.GetOnChangeOperations(typeof(TestBehaviorA),
                TestBehaviorA.ContextAName, nameof(TestContextA.Int))
                .Returns(new MethodInfo[] { mi });

            _app.Decontextualize(contextA);
            int expectedValue = 11;
            contextA.Int.Value = expectedValue;

            _app.Update(); // Behavior A would update Context A's context change value.

            Assert.AreNotEqual(expectedValue, contextA.OnStateChangeIntValue);
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
            App app = Setup.ContextOnlyApp<TestContextA>();

            Assert.IsNull(app.GetContext<TestContextA>());
        }

        [Test]
        public void ProvidesFirstWhenMultipleAreAvailable()
        {
            App app = Setup.ContextOnlyApp<TestContextA>();

            TestContextA firstContext = new();
            app.Contextualize(firstContext);
            app.Contextualize(new TestContextA());

            Assert.AreEqual(firstContext, app.GetContext<TestContextA>());
        }

        [Test]
        public void ProvidesWhenOneIsAvailable()
        {
            App app = Setup.ContextOnlyApp<TestContextA>();

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
            App app = Setup.ContextOnlyApp<TestContextA>();

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
            App app = Setup.ContextOnlyApp<TestContextA>();

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
            IBehaviorFactory factory = Setup.BuildFactorySubstitute<TestBehaviorA>(
                new()
                {
                    { TestBehaviorA.ContextAName, new TestContextA() },
                    { TestBehaviorA.ContextBName, new TestContextB() }
                });
            _app = Setup.BehaviorAndContextApp<TestBehaviorA, TestContextA,
                TestContextB>(factory);
        }

        [Test]
        public void ContextualizesInitializationBehaviorContexts()
        {
            Assert.NotNull(_app.GetContext<TestContextA>());
        }

        [Test]
        public void InitializationBehavior_NewChangesInvokeOnContextChange()
        {
            TestContextA? contextA = _app.GetContext<TestContextA>();
            if (contextA == null)
                throw new NullReferenceException();

            MethodInfo? mi = typeof(TestBehaviorA).GetMethod("OnContextAChange");
            if (mi == null)
                throw new NullReferenceException();

            _app.Evaluator.GetOnChangeOperations(typeof(TestBehaviorA),
                TestBehaviorA.ContextAName).Returns(new MethodInfo[] { mi });

            int expectedValue = 11;
            contextA.Int.Value = expectedValue;

            _app.Update(); // Behavior A updates Context A's context change value.

            Assert.AreEqual(expectedValue, contextA.OnContextChangeIntValue);
        }

        [Test]
        public void InitializationBehavior_NewChangesInvokeOnStateChange()
        {
            TestContextA? contextA = _app.GetContext<TestContextA>();
            if (contextA == null)
                throw new NullReferenceException();

            MethodInfo? mi = typeof(TestBehaviorA).GetMethod("OnContextAIntChange",
                BindingFlags.Instance | BindingFlags.NonPublic);
            if (mi == null)
                throw new NullReferenceException();

            _app.Evaluator.GetOnChangeOperations(typeof(TestBehaviorA),
                TestBehaviorA.ContextAName, nameof(TestContextA.Int))
                .Returns(new MethodInfo[] { mi });

            int expectedValue = 11;
            contextA.Int.Value = expectedValue;

            _app.Update(); // Behavior A updates Context A's context change value.

            Assert.AreEqual(expectedValue, contextA.OnStateChangeIntValue);
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
            IBehaviorFactory factory = Setup.BuildFactorySubstitute<TestBehaviorA>(
                new()
                {
                    { TestBehaviorA.ContextAName, new TestContextA() },
                    { TestBehaviorA.ContextBName, new TestContextB() }
                });
            _app = Setup.BehaviorAndContextApp<TestBehaviorA, TestContextA,
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
            TestContextA? contextA = _app.GetContext<TestContextA>();
            if (contextA == null)
                throw new NullReferenceException();

            MethodInfo? mi = typeof(TestBehaviorA).GetMethod("OnContextAChange");
            if (mi == null)
                throw new NullReferenceException();

            _app.Evaluator.GetOnChangeOperations(typeof(TestBehaviorA),
                TestBehaviorA.ContextAName).Returns(new MethodInfo[] { mi });

            int expectedValue = 11;
            contextA.Int.Value = expectedValue;

            _app.Update(); // Behavior A updates Context A's context change value.

            Assert.AreEqual(expectedValue, contextA.OnContextChangeIntValue);
        }

        [Test]
        public void WithChanges_InvokesOnStateChangeOperations()
        {
            TestContextA? contextA = _app.GetContext<TestContextA>();
            if (contextA == null)
                throw new NullReferenceException();

            MethodInfo? mi = typeof(TestBehaviorA).GetMethod("OnContextAIntChange",
                BindingFlags.Instance | BindingFlags.NonPublic);
            if (mi == null)
                throw new NullReferenceException();

            _app.Evaluator.GetOnChangeOperations(typeof(TestBehaviorA),
                TestBehaviorA.ContextAName, nameof(TestContextA.Int))
                .Returns(new MethodInfo[] { mi });

            int expectedValue = 11;
            contextA.Int.Value = expectedValue;

            _app.Update(); // Behavior A updates Context A's state change value.

            Assert.AreEqual(expectedValue, contextA.OnStateChangeIntValue);
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

            MethodInfo? mi = typeof(TestBehaviorA).GetMethod("OnContextAIntChange",
                BindingFlags.Instance | BindingFlags.NonPublic);
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
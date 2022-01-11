using ContextualProgramming.Internal;
using NSubstitute;
using NUnit.Framework;
using System.Reflection;

namespace AppTests
{
    public static class Setup
    {
        public static void PrimeEvaluatorForBehavior<TBehavior>(Evaluator evaluator)
        {
            Type type = typeof(TBehavior);

            evaluator.GetInitializationBehaviorTypes().Returns(new Type[] { type });
            evaluator.GetBehaviorConstructor(type).Returns(type
                .GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic)[0]);
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

        public static App BehaviorAndContextApp<TBehavior, TContext>()
        {
            Evaluator evaluator = Substitute.For<Evaluator>();
            App app = new(evaluator);

            PrimeEvaluatorForBehavior<TBehavior>(evaluator);
            PrimeEvaluatorForContext<TContext>(evaluator);
            app.Initialize();

            return app;
        }

        public static App BehaviorAndContextApp<TBehavior, TContextA, TContextB>()
        {
            Evaluator evaluator = Substitute.For<Evaluator>();
            App app = new(evaluator);

            PrimeEvaluatorForBehavior<TBehavior>(evaluator);
            PrimeEvaluatorForContext<TContextA>(evaluator);
            PrimeEvaluatorForContext<TContextB>(evaluator);
            app.Initialize();

            return app;
        }

        public static App ContextOnlyApp<TContext>()
        {
            Evaluator evaluator = Substitute.For<Evaluator>();
            App app = new(evaluator);

            evaluator.GetInitializationBehaviorTypes().Returns(Array.Empty<Type>());
            PrimeEvaluatorForContext<TContext>(evaluator);
            app.Initialize();

            return app;
        }
    }

    #region Shared Constructs
    public abstract class TDAttribute : BaseDependencyAttribute
    {
        protected TDAttribute(Binding binding, Fulfillment fulfillment,
            string name, Type type) : base(binding, fulfillment, name, type) { }
    }
    public class TDAttribute<T> : TDAttribute
    {
        public TDAttribute(Binding binding, Fulfillment fulfillment,
            string name) : base(binding, fulfillment, name, typeof(T)) { }
    }

    public class TestNonContext { }
    #endregion

    public class Construction
    {
        [Test]
        public void Construction_DefaultEvaluator()
        {
            Type expectedEvaluator = typeof(Evaluator<ContextAttribute, BehaviorAttribute,
                DependencyAttribute, OperationAttribute>);

            App app = new();

            Assert.AreEqual(expectedEvaluator, app.Evaluator.GetType());
        }

        [Test]
        public void Construction_OverriddenEvaluator()
        {
            Evaluator evaluator = Substitute.For<Evaluator>();

            App app = new(evaluator);

            Assert.AreEqual(evaluator.GetType(), app.Evaluator.GetType());
        }
    }

    public class Contextualization
    {
        public class TCAttribute : BaseContextAttribute { }
        [TC]
        public class TestContextA
        {
            public ContextState<int> Int { get; init; } = 0;
        }

        [Test]
        public void Contextualization_BindsForChanges()
        {
            App app = Setup.ContextOnlyApp<TestContextA>();

            TestContextA context = new();
            context.Int.Value = 10;

            Assert.IsFalse(app.Update()); // Not yet bound, so changes aren't detected.

            app.Contextualize(context);

            context.Int.Value = 11;

            Assert.IsTrue(app.Update());
        }

        [Test]
        public void Contextualization_Contextualizes()
        {
            App app = Setup.ContextOnlyApp<TestContextA>();

            TestContextA expectedContext = new();
            app.Contextualize(expectedContext);

            Assert.AreEqual(expectedContext, app.GetContext<TestContextA>());
        }

        [Test]
        public void Contextualization_NonContextTypeThrowsException()
        {
            Evaluator evaluator = Substitute.For<Evaluator>();
            App app = new(evaluator);

            evaluator.GetInitializationBehaviorTypes().Returns(Array.Empty<Type>());
            evaluator.IsContextType(typeof(TestNonContext)).Returns(false);
            app.Initialize();

            Assert.Throws<InvalidOperationException>(() =>
                app.Contextualize<TestNonContext>(new()));
        }

        [Test]
        public void Contextualization_NullThrowsException()
        {
            App app = Setup.ContextOnlyApp<TestContextA>();

            Assert.Throws<ArgumentNullException>(() =>
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                app.Contextualize<TestContextA>(null));
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
        }

        [Test]
        public void Contextualization_UninitializedThrowsException()
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
        public class TBAttribute : BaseBehaviorAttribute { }
        [TB]
        [TD<TestContextA>(Binding.Unique, Fulfillment.SelfCreated, "contextA")]
        public class TestBehaviorA
        {
            public static int InstanceCount = 0;

            protected TestBehaviorA(out TestContextA contextA)
            {
                contextA = new();

                InstanceCount++;
            }

            ~TestBehaviorA()
            {
                InstanceCount--;
            }
        }

        public class TCAttribute : BaseContextAttribute { }
        [TC]
        public class TestContextA
        {
            public ContextState<int> Int { get; init; } = 0;
        }

        [Test]
        public void Decontextualization_Decontextualizes()
        {
            Evaluator evaluator = Substitute.For<Evaluator>();
            App app = new(evaluator);

            Setup.PrimeEvaluatorForContext<TestContextA>(evaluator);
            Setup.PrimeEvaluatorForBehavior<TestBehaviorA>(evaluator);

            app.Initialize();

            app.Decontextualize(app.GetContext<TestContextA>());

            Assert.IsNull(app.GetContext<TestContextA>());
        }

        [Test]
        public void Decontextualization_DestroysDependentBehaviors()
        {
            Evaluator evaluator = Substitute.For<Evaluator>();
            App app = new(evaluator);

            Setup.PrimeEvaluatorForContext<TestContextA>(evaluator);
            Setup.PrimeEvaluatorForBehavior<TestBehaviorA>(evaluator);

            app.Initialize();

            app.Decontextualize(app.GetContext<TestContextA>());

            GC.Collect();
            GC.WaitForPendingFinalizers();

            Assert.AreEqual(0, TestBehaviorA.InstanceCount);
        }

        [Test]
        public void Decontextualization_NonContextTypeThrowsException()
        {
            Evaluator evaluator = Substitute.For<Evaluator>();
            App? app = new(evaluator);

            evaluator.GetInitializationBehaviorTypes().Returns(Array.Empty<Type>());
            evaluator.IsContextType(typeof(TestNonContext)).Returns(false);
            app.Initialize();

            Assert.Throws<InvalidOperationException>(() =>
                app.Decontextualize<TestNonContext>(new()));
        }

        [Test]
        public void Decontextualization_NullThrowsException()
        {
            App app = Setup.ContextOnlyApp<TestContextA>();

#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            Assert.Throws<ArgumentNullException>(() =>
                app.Decontextualize<TestContextA>(null));
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
        }

        [Test]
        public void Decontextualization_UnbindsForChanges()
        {
            App app = Setup.ContextOnlyApp<TestContextA>();

            TestContextA context = new();
            app.Contextualize(context);
            app.Decontextualize(context);
            context.Int.Value = 10;

            Assert.IsFalse(app.Update());
        }

        [Test]
        public void Decontextualization_UninitializedThrowsException()
        {
            Evaluator evaluator = Substitute.For<Evaluator>();
            App app = new(evaluator);

            Assert.Throws<InvalidOperationException>(() =>
                app.Decontextualize<TestContextA>(new()));
        }
    }

    public class GetContext
    {
        public class TCAttribute : BaseContextAttribute { }
        [TC]
        public class TestContextA
        {
            public ContextState<int> Int { get; init; } = 0;
        }

        [Test]
        public void GetContext_NonContextThrowsException()
        {
            Evaluator evaluator = Substitute.For<Evaluator>();
            App app = new(evaluator);

            evaluator.GetInitializationBehaviorTypes().Returns(Array.Empty<Type>());
            evaluator.IsContextType(typeof(TestNonContext)).Returns(false);
            app.Initialize();

            Assert.Throws<InvalidOperationException>(() =>
                app.GetContext<TestNonContext>());
        }

        [Test]
        public void GetContext_NullWhenNoneAvailable()
        {
            App app = Setup.ContextOnlyApp<TestContextA>();

            Assert.IsNull(app.GetContext<TestContextA>());
        }

        [Test]
        public void GetContext_ProvidesFirstWhenMultipleAreAvailable()
        {
            App app = Setup.ContextOnlyApp<TestContextA>();

            TestContextA firstContext = new();
            app.Contextualize(firstContext);
            app.Contextualize(new TestContextA());

            Assert.AreEqual(firstContext, app.GetContext<TestContextA>());
        }

        [Test]
        public void GetContext_ProvidesWhenOneIsAvailable()
        {
            App app = Setup.ContextOnlyApp<TestContextA>();

            TestContextA firstContext = new();
            app.Contextualize(firstContext);

            Assert.AreEqual(firstContext, app.GetContext<TestContextA>());
        }

        [Test]
        public void GetContext_UninitializedThrowsException()
        {
            Evaluator evaluator = Substitute.For<Evaluator>();
            App app = new(evaluator);

            Assert.Throws<InvalidOperationException>(() => app.GetContext<TestContextA>());
        }
    }

    public class GetContexts
    {
        public class TCAttribute : BaseContextAttribute { }
        [TC]
        public class TestContextA
        {
            public ContextState<int> Int { get; init; } = 0;
        }

        [Test]
        public void GetContexts_EmptyWhenNonAvailable()
        {
            App app = Setup.ContextOnlyApp<TestContextA>();

            Assert.IsEmpty(app.GetContexts<TestContextA>());
        }

        [Test]
        public void GetContexts_NonContextThrowsException()
        {
            Evaluator evaluator = Substitute.For<Evaluator>();
            App app = new(evaluator);

            evaluator.GetInitializationBehaviorTypes().Returns(Array.Empty<Type>());
            evaluator.IsContextType(typeof(TestNonContext)).Returns(false);
            app.Initialize();

            Assert.Throws<InvalidOperationException>(() =>
                app.GetContexts<TestNonContext>());
        }

        [Test]
        public void GetContexts_ProvidesWhenAvailable()
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
        public void GetContexts_UninitializedThrowsException()
        {
            Evaluator evaluator = Substitute.For<Evaluator>();
            App app = new(evaluator);

            Assert.Throws<InvalidOperationException>(() => app.GetContexts<TestContextA>());
        }
    }

    public class Initialization
    {
        public class TBAttribute : BaseBehaviorAttribute { }
        [TB]
        [TD<TestContextA>(Binding.Unique, Fulfillment.SelfCreated, "contextA")]
        public class TestBehaviorA
        {
            public static int InstanceCount = 0;

            protected TestBehaviorA(out TestContextA contextA)
            {
                contextA = new();

                InstanceCount++;
            }

            ~TestBehaviorA()
            {
                InstanceCount--;
            }
        }

        public class TBNullDependencyConstructorBehaviorAttribute : BaseBehaviorAttribute { }
        [TBNullDependencyConstructorBehavior]
        [TDAttribute<TestContextA>(Binding.Unique, Fulfillment.SelfCreated, "contextA")]
        public class TestNullDependencyConstructorBehavior
        {
            protected TestNullDependencyConstructorBehavior(out TestContextA? contextA) =>
                contextA = null;
        }

        public class TCAttribute : BaseContextAttribute { }
        [TC]
        public class TestContextA
        {
            public ContextState<int> Int { get; init; } = 0;
        }

        [SetUp]
        public void SetupTests()
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        [Test]
        public void Initialization_ContextualizedInitializationBehaviorContexts()
        {
            App app = Setup.BehaviorAndContextApp<TestBehaviorA, TestContextA>();

            Assert.NotNull(app.GetContext<TestContextA>());
        }

        [Test]
        public void Initialization_InstantiatedInitializationBehaviors()
        {
            App _ = Setup.BehaviorAndContextApp<TestBehaviorA, TestContextA>();

            Assert.AreEqual(1, TestBehaviorA.InstanceCount);
        }

        [Test]
        public void Initialization_NullDependencyConstructorThrowsException()
        {
            Evaluator evaluator = Substitute.For<Evaluator>();
            App app = new(evaluator);

            Setup.PrimeEvaluatorForBehavior<TestNullDependencyConstructorBehavior>(evaluator);
            Setup.PrimeEvaluatorForContext<TestContextA>(evaluator);

            Assert.Throws<InvalidOperationException>(() => app.Initialize());
        }
    }

    public class Updating
    {
        public class TBAttribute : BaseBehaviorAttribute { }

        public class TCAttribute : BaseContextAttribute { }

        public class TOAttribute : BaseOperationAttribute { }

        [TB]
        [TD<TestContextA>(Binding.Unique, Fulfillment.SelfCreated, ContextAName)]
        [TD<TestContextB>(Binding.Unique, Fulfillment.SelfCreated, ContextBName)]
        public class TestBehaviorA
        {
            public const string ContextAName = "contextA";
            public const string ContextBName = "contextB";

            protected TestBehaviorA(out TestContextA contextA, out TestContextB contextB)
            {
                contextA = new();
                contextB = new();
            }


            [TO]
            [OnChange(ContextAName, nameof(TestContextA.Int))]
            private void OnContextAIntChange(TestContextA contextA, TestContextB contextB)
            {
                contextA.OnStateChangeIntValue = contextA.Int;
                contextB.Int.Value = contextA.Int;
            }

            [TO]
            [OnChange(ContextAName)]
            public void OnContextAChange(TestContextA contextA)
            {
                contextA.OnContextChangeIntValue = contextA.Int;
            }

            [TO]
            [OnChange(ContextBName, nameof(TestContextB.Int))]
            public void OnContextBIntChange(TestContextB contextB)
            {
                contextB.OnContextChangeIntValue = contextB.Int;
            }
        }


        [TC]
        public class TestContextA
        {
            public int OnStateChangeIntValue { get; set; } = 0;
            public int OnContextChangeIntValue { get; set; } = 0;

            public ContextState<int> Int { get; init; } = 0;
        }

        [TC]
        public class TestContextB
        {
            public int OnContextChangeIntValue { get; set; } = 0;

            public ContextState<int> Int { get; init; } = 0;
        }

        [Test]
        public void Updating_NoChanges_DoesNotInvokeContextChangeOperations()
        {
            App app = Setup.BehaviorAndContextApp<TestBehaviorA, TestContextA, TestContextB>();

            TestContextA? contextA = app.GetContext<TestContextA>();
            if (contextA == null)
                throw new NullReferenceException();

            app.Update();

            Assert.AreEqual(0, contextA.OnContextChangeIntValue);
        }
        [Test]
        public void Updating_NoChanges_DoesNotInvokeStateChangeOperations()
        {
            App app = Setup.BehaviorAndContextApp<TestBehaviorA, TestContextA, TestContextB>();

            TestContextA? contextA = app.GetContext<TestContextA>();
            if (contextA == null)
                throw new NullReferenceException();

            app.Update();

            Assert.AreEqual(0, contextA.OnStateChangeIntValue);
        }

        [Test]
        public void Updating_NoChanges_ReturnsFalse()
        {
            App app = Setup.BehaviorAndContextApp<TestBehaviorA, TestContextA, TestContextB>();

            Assert.IsFalse(app.Update());
        }

        [Test]
        public void Updating_WithChanges_ClearsPreviousChanges()
        {
            App app = Setup.BehaviorAndContextApp<TestBehaviorA, TestContextA, TestContextB>();

            TestContextA? contextA = app.GetContext<TestContextA>();
            if (contextA == null)
                throw new NullReferenceException();

            contextA.Int.Value = 11;

            app.Update(); // Evaluate Context A's changes, which change Context C.
            app.Update(); // Evaluate Context C's changes, which result in no new changes.

            Assert.IsFalse(app.Update());
        }

        [Test]
        public void Updating_WithChanges_InvokesOnContextChangeOperations()
        {
            App app = Setup.BehaviorAndContextApp<TestBehaviorA, TestContextA, TestContextB>();

            TestContextA? contextA = app.GetContext<TestContextA>();
            if (contextA == null)
                throw new NullReferenceException();

            MethodInfo? mi = typeof(TestBehaviorA).GetMethod("OnContextAChange");
            if (mi == null)
                throw new NullReferenceException();

            app.Evaluator.GetOnChangeOperations(typeof(TestBehaviorA),
                TestBehaviorA.ContextAName).Returns(new MethodInfo[] { mi });

            int expectedValue = 11;
            contextA.Int.Value = expectedValue;

            app.Update(); // Behavior A updates Context A's context change value.

            Assert.AreEqual(expectedValue, contextA.OnContextChangeIntValue);
        }

        [Test]
        public void Updating_WithChanges_InvokesOnStateChangeOperations()
        {
            App app = Setup.BehaviorAndContextApp<TestBehaviorA, TestContextA, TestContextB>();

            TestContextA? contextA = app.GetContext<TestContextA>();
            if (contextA == null)
                throw new NullReferenceException();

            MethodInfo? mi = typeof(TestBehaviorA).GetMethod("OnContextAIntChange",
                BindingFlags.Instance | BindingFlags.NonPublic);
            if (mi == null)
                throw new NullReferenceException();

            app.Evaluator.GetOnChangeOperations(typeof(TestBehaviorA),
                TestBehaviorA.ContextAName, nameof(TestContextA.Int))
                .Returns(new MethodInfo[] { mi });

            int expectedValue = 11;
            contextA.Int.Value = expectedValue;

            app.Update(); // Behavior A updates Context A's state change value.

            Assert.AreEqual(expectedValue, contextA.OnStateChangeIntValue);
        }

        [Test]
        public void Updating_WithChanges_RecordsAndInvokesForSubsequentChanges()
        {
            App app = Setup.BehaviorAndContextApp<TestBehaviorA, TestContextA, TestContextB>();

            TestContextA? contextA = app.GetContext<TestContextA>();
            if (contextA == null)
                throw new NullReferenceException();

            TestContextB? contextB = app.GetContext<TestContextB>();
            if (contextB == null)
                throw new NullReferenceException();

            MethodInfo? mi = typeof(TestBehaviorA).GetMethod("OnContextAIntChange",
                BindingFlags.Instance | BindingFlags.NonPublic);
            if (mi == null)
                throw new NullReferenceException();

            app.Evaluator.GetOnChangeOperations(typeof(TestBehaviorA),
                TestBehaviorA.ContextAName, nameof(TestContextA.Int))
                .Returns(new MethodInfo[] { mi });

            mi = typeof(TestBehaviorA).GetMethod("OnContextBIntChange");
            if (mi == null)
                throw new NullReferenceException();

            app.Evaluator.GetOnChangeOperations(typeof(TestBehaviorA),
                TestBehaviorA.ContextBName, nameof(TestContextB.Int))
                .Returns(new MethodInfo[] { mi });

            int expectedValue = 11;
            contextA.Int.Value = expectedValue;

            app.Update(); // Behavior A propagates Context A's value to Context C.
            app.Update(); // Behavior A updates Context C's context change value.

            Assert.AreEqual(expectedValue, contextB.OnContextChangeIntValue);
        }

        [Test]
        public void Updating_WithChanges_ReturnsTrue()
        {
            App app = Setup.BehaviorAndContextApp<TestBehaviorA, TestContextA, TestContextB>();

            TestContextA? contextA = app.GetContext<TestContextA>();
            if (contextA == null)
                throw new NullReferenceException();

            contextA.Int.Value = 11;

            Assert.IsTrue(app.Update());
        }
    }
}
using ContextualProgramming.Internal;
using NSubstitute;
using NUnit.Framework;
using System.Reflection;
using Tests.Constructs;

namespace Tests
{
    public class AppTests
    {
        [SetUp]
        public void Setup()
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        #region Construction
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
        #endregion

        #region Contextualization
        [Test]
        public void Contextualization_BindsForChanges()
        {
            Evaluator evaluator = Substitute.For<Evaluator>();
            App? app = new(evaluator);

            PrimeEvaluatorForContext<TestContextA>(evaluator);

            app.Initialize();

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
            Evaluator evaluator = Substitute.For<Evaluator>();
            App? app = new(evaluator);

            evaluator.GetInitializationBehaviorTypes().Returns(Array.Empty<Type>());
            evaluator.IsContextType(typeof(TestContextA)).Returns(true);
            app.Initialize();

            TestContextA expectedContext = new();
            app.Contextualize(expectedContext);

            Assert.AreEqual(expectedContext, app.GetContext<TestContextA>());
        }

        [Test]
        public void Contextualization_NonContextTypeThrowsException()
        {
            Evaluator evaluator = Substitute.For<Evaluator>();
            App? app = new(evaluator);

            evaluator.GetInitializationBehaviorTypes().Returns(Array.Empty<Type>());
            evaluator.IsContextType(typeof(TestNonContext)).Returns(false);
            app.Initialize();

            Assert.Throws<InvalidOperationException>(() =>
                app.Contextualize<TestNonContext>(new()));
        }

        [Test]
        public void Contextualization_NullThrowsException()
        {
            Evaluator evaluator = Substitute.For<Evaluator>();
            App? app = new(evaluator);

            evaluator.GetInitializationBehaviorTypes().Returns(Array.Empty<Type>());
            app.Initialize();

            Assert.Throws<ArgumentNullException>(() =>
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                app.Contextualize<TestContextA>(null));
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
        }

        [Test]
        public void Contextualization_UninitializedThrowsException()
        {
            Evaluator evaluator = Substitute.For<Evaluator>();
            App? app = new(evaluator);

            evaluator.IsContextType(typeof(TestContextA)).Returns(true);

            Assert.Throws<InvalidOperationException>(() =>
                app.Contextualize<TestContextA>(new()));
        }
        #endregion

        #region Decontextualization
        [Test]
        public void Decontextualization_Decontextualizes()
        {
            App app = SetupStandardApp();

            app.Decontextualize(app.GetContext<TestContextA>());

            Assert.IsNull(app.GetContext<TestContextA>());
        }

        [Test]
        public void Decontextualization_DestroysDependentBehaviors()
        {
            App app = SetupStandardApp();

            app.Decontextualize(app.GetContext<TestContextA>());

            GC.Collect();
            GC.WaitForPendingFinalizers();

            Assert.AreEqual(0, TestBehaviorA.InstanceCount);
        }

        [Test]
        public void Decontextualization_NullThrowsException()
        {
            Evaluator evaluator = Substitute.For<Evaluator>();
            App? app = new(evaluator);

            evaluator.GetInitializationBehaviorTypes().Returns(Array.Empty<Type>());
            app.Initialize();

            Assert.Throws<ArgumentNullException>(() =>
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                app.Contextualize<TestContextA>(null));
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
        }

        [Test]
        public void Decontextualization_UnbindsForChanges()
        {
            Evaluator evaluator = Substitute.For<Evaluator>();
            App? app = new(evaluator);

            PrimeEvaluatorForContext<TestContextA>(evaluator);

            app.Initialize();

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
            App? app = new(evaluator);

            Assert.Throws<InvalidOperationException>(() =>
                app.Decontextualize<TestContextA>(new()));
        }
        #endregion

        #region Get Context
        [Test]
        public void GetContext_NonContextThrowsException()
        {
            Evaluator evaluator = Substitute.For<Evaluator>();
            App? app = new(evaluator);

            evaluator.GetInitializationBehaviorTypes().Returns(Array.Empty<Type>());
            evaluator.IsContextType(typeof(TestNonContext)).Returns(false);
            app.Initialize();

            Assert.Throws<InvalidOperationException>(() =>
                app.GetContext<TestNonContext>());
        }

        [Test]
        public void GetContext_NullWhenNoneAvailable()
        {
            Evaluator evaluator = Substitute.For<Evaluator>();
            App? app = new(evaluator);

            evaluator.GetInitializationBehaviorTypes().Returns(Array.Empty<Type>());
            evaluator.IsContextType(typeof(TestContextA)).Returns(true);
            app.Initialize();

            Assert.IsNull(app.GetContext<TestContextA>());
        }

        [Test]
        public void GetContext_ProvidesFirstWhenMultipleAreAvailable()
        {
            App app = SetupStandardApp();

            TestContextA? firstContext = app.GetContext<TestContextA>();
            Assert.IsNotNull(firstContext);

            app.Contextualize(new TestContextA());

            Assert.AreEqual(firstContext, app.GetContext<TestContextA>());
        }

        [Test]
        public void GetContext_ProvidesWhenOneIsAvailable()
        {
            App app = SetupStandardApp();

            Assert.IsNotNull(app.GetContext<TestContextA>());
        }

        [Test]
        public void GetContext_UninitializedThrowsException()
        {
            Evaluator evaluator = Substitute.For<Evaluator>();
            App? app = new(evaluator);

            Assert.Throws<InvalidOperationException>(() => app.GetContext<TestContextA>());
        }
        #endregion

        #region Get Contexts
        [Test]
        public void GetContexts_EmptyWhenNonAvailable()
        {
            Evaluator evaluator = Substitute.For<Evaluator>();
            App? app = new(evaluator);

            evaluator.GetInitializationBehaviorTypes().Returns(Array.Empty<Type>());
            evaluator.IsContextType(typeof(TestContextA)).Returns(true);
            app.Initialize();

            Assert.IsEmpty(app.GetContexts<TestContextA>());
        }

        [Test]
        public void GetContexts_NonContextThrowsException()
        {
            Evaluator evaluator = Substitute.For<Evaluator>();
            App? app = new(evaluator);

            evaluator.GetInitializationBehaviorTypes().Returns(Array.Empty<Type>());
            evaluator.IsContextType(typeof(TestNonContext)).Returns(false);
            app.Initialize();

            Assert.Throws<InvalidOperationException>(() =>
                app.GetContexts<TestNonContext>());
        }

        [Test]
        public void GetContexts_ProvidesWhenAvailable()
        {
            App app = SetupStandardApp();

            Assert.AreEqual(1, app.GetContexts<TestContextA>().Length);

            app.Contextualize(new TestContextA());

            Assert.AreEqual(2, app.GetContexts<TestContextA>().Length);
        }

        [Test]
        public void GetContexts_UninitializedThrowsException()
        {
            Evaluator evaluator = Substitute.For<Evaluator>();
            App? app = new(evaluator);

            Assert.Throws<InvalidOperationException>(() => app.GetContexts<TestContextA>());
        }
        #endregion

        #region Initialization
        [Test]
        public void Initialization_ContextualizedInitializationBehaviorContexts()
        {
            App app = SetupStandardApp();

            Assert.NotNull(app.GetContext<TestContextA>());
        }

        [Test]
        public void Initialization_InstantiatedInitializationBehaviors()
        {
            App _ = SetupStandardApp();

            Assert.AreEqual(1, TestBehaviorA.InstanceCount);
        }

        [Test]
        public void Initialization_InvalidBehaviorContructorThrowsException()
        {
            Evaluator evaluator = Substitute.For<Evaluator>();
            App app = new(evaluator);

            PrimeEvaluatorForBehavior<TestNullDependencyConstructorBehavior>(evaluator);

            Assert.Throws<InvalidOperationException>(() => app.Initialize());
        }
        #endregion

        #region Updating
        [Test]
        public void Updating_NoChanges_DoesNotInvokeContextChangeOperations()
        {
            App app = SetupStandardApp();

            TestContextA? contextA = app.GetContext<TestContextA>();
            if (contextA == null)
                throw new NullReferenceException();

            app.Update();

            Assert.AreEqual(0, contextA.OnContextChangeIntValue);
        }
        [Test]
        public void Updating_NoChanges_DoesNotInvokeStateChangeOperations()
        {
            App app = SetupStandardApp();

            TestContextA? contextA = app.GetContext<TestContextA>();
            if (contextA == null)
                throw new NullReferenceException();

            app.Update();

            Assert.AreEqual(0, contextA.OnStateChangeIntValue);
        }

        [Test]
        public void Updating_NoChanges_ReturnsFalse()
        {
            App app = SetupStandardApp();

            Assert.IsFalse(app.Update());
        }

        [Test]
        public void Updating_WithChanges_ClearsPreviousChanges()
        {
            App app = SetupStandardApp();

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
            App app = SetupStandardApp();

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
            App app = SetupStandardApp();

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
            App app = SetupStandardApp();

            TestContextA? contextA = app.GetContext<TestContextA>();
            if (contextA == null)
                throw new NullReferenceException();

            TestContextC? contextC = app.GetContext<TestContextC>();
            if (contextC == null)
                throw new NullReferenceException();

            MethodInfo? mi = typeof(TestBehaviorA).GetMethod("OnContextAIntChange",
                BindingFlags.Instance | BindingFlags.NonPublic);
            if (mi == null)
                throw new NullReferenceException();

            app.Evaluator.GetOnChangeOperations(typeof(TestBehaviorA),
                TestBehaviorA.ContextAName, nameof(TestContextA.Int))
                .Returns(new MethodInfo[] { mi });

            mi = typeof(TestBehaviorA).GetMethod("OnContextCIntChange");
            if (mi == null)
                throw new NullReferenceException();

            app.Evaluator.GetOnChangeOperations(typeof(TestBehaviorA),
                TestBehaviorA.ContextCName, nameof(TestContextC.Int))
                .Returns(new MethodInfo[] { mi });

            int expectedValue = 11;
            contextA.Int.Value = expectedValue;

            app.Update(); // Behavior A propagates Context A's value to Context C.
            app.Update(); // Behavior A updates Context C's context change value.

            Assert.AreEqual(expectedValue, contextC.OnContextChangeIntValue);
        }

        [Test]
        public void Updating_WithChanges_ReturnsTrue()
        {
            App app = SetupStandardApp();

            TestContextA? contextA = app.GetContext<TestContextA>();
            if (contextA == null)
                throw new NullReferenceException();

            contextA.Int.Value = 11;

            Assert.IsTrue(app.Update());
        }
        #endregion


        private void PrimeEvaluatorForBehavior<TBehavior>(Evaluator evaluator)
        {
            Type type = typeof(TBehavior);

            evaluator.GetInitializationBehaviorTypes().Returns(new Type[] { type });
            evaluator.GetBehaviorConstructor(type).Returns(type
                .GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic)[0]);
        }

        private void PrimeEvaluatorForContext<TContext>(Evaluator evaluator)
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

        private App SetupStandardApp()
        {
            Evaluator evaluator = Substitute.For<Evaluator>();
            App? app = new(evaluator);

            PrimeEvaluatorForContext<TestContextA>(evaluator);
            PrimeEvaluatorForContext<TestContextC>(evaluator);
            PrimeEvaluatorForBehavior<TestBehaviorA>(evaluator);

            app.Initialize();
            return app;
        }
    }
}
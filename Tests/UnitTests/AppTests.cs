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
        //[Test]
        //public void Contextualization_BindsForChanges()
        //{
            // TODO :: Implement once able to be validated.
        //}

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

        //[Test]
        //public void Decontextualization_UnbindsForChanges()
        //{
        // TODO :: Implement once able to be validated.
        //}

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
            App app = SetupStandardApp();

            Assert.AreEqual(1, TestBehaviorA.InstanceCount);
        }

        [Test]
        public void Initialization_InvalidBehaviorContructorThrowsException()
        {
            Evaluator evaluator = Substitute.For<Evaluator>();
            App app = new(evaluator);

            Type bType = typeof(TestNullDependencyConstructorBehavior);
            evaluator.GetInitializationBehaviorTypes().Returns(new Type[] { bType });
            evaluator.GetBehaviorConstructor(bType).Returns(bType
                .GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic)[0]);

            Assert.Throws<InvalidOperationException>(() => app.Initialize());
        }
        #endregion


        private App SetupStandardApp()
        {
            Evaluator evaluator = Substitute.For<Evaluator>();
            App? app = new(evaluator);

            Type bType = typeof(TestBehaviorA);
            Type cType = typeof(TestContextA);

            evaluator.GetInitializationBehaviorTypes().Returns(new Type[] { bType });
            evaluator.GetBehaviorConstructor(bType).Returns(bType
                .GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic)[0]);
            evaluator.IsContextType(cType).ReturnsForAnyArgs(true);
            evaluator.GetBindableStateInfos(cType).Returns(new PropertyInfo[0]);

            app.Initialize();
            return app;
        }
    }
}
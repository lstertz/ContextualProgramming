using ContextualProgramming.Internal;
using NUnit.Framework;
using System.Reflection;

namespace Tests
{
    public class EvaluatorTests
    {
        #region Test Attributes
        private class TBAttribute : BehaviorAttribute { }
        private class TBNonContextDependencyAttribute : BehaviorAttribute { }
        private class TBInvalidDependencyConstructorAttribute : BehaviorAttribute { }
        private class TBInvalidParameterNameConstructorAttribute : BehaviorAttribute { }
        private class TBInvalidParameterCountConstructorAttribute : BehaviorAttribute { }
        private class TBInvalidParameterTypeConstructorAttribute : BehaviorAttribute { }
        private class TBMissingConstructorAttribute : BehaviorAttribute { }
        private class TBNonOutParameterConstructorAttribute : BehaviorAttribute { }
        private class UnusedTBAttribute : BehaviorAttribute { }

        private class TCAttribute : ContextAttribute { }
        private class UnusedTCAttribute : ContextAttribute { }

        private abstract class TDAttribute : DependencyAttribute
        {
            protected TDAttribute(Binding binding, Fulfillment fulfillment,
                string name, Type type) : base(binding, fulfillment, name, type) { }
        }
        private class TDAttribute<T> : TDAttribute
        {
            public TDAttribute(Binding binding, Fulfillment fulfillment,
                string name) : base(binding, fulfillment, name, typeof(T)) { }
        }
        #endregion

        #region Test Classes
        [TB]
        [TDAttribute<TestContextA>(Binding.Unique, Fulfillment.SelfCreated, "contextA")]
        private class TestBehaviorA
        {
            protected TestBehaviorA(out TestContextA contextA) => contextA = new();
        }

        [TB]
        private class TestBehaviorB { }


        [TBInvalidDependencyConstructor]
        [TDAttribute<TestContextA>(Binding.Unique, Fulfillment.SelfCreated, "contextA")]
        private class TestInvalidDependencyConstructorBehavior
        {
            protected TestInvalidDependencyConstructorBehavior(out TestContextB contextB) =>
                contextB = new();
        }

        [TBInvalidParameterCountConstructor]
        [TDAttribute<TestContextA>(Binding.Unique, Fulfillment.SelfCreated, "contextA")]
        private class TestInvalidParameterCountConstructorBehavior
        {
            protected TestInvalidParameterCountConstructorBehavior(
                out TestContextA contextA, out TestContextB contextB)
            {
                contextA = new();
                contextB = new();
            }
        }

        [TBInvalidParameterNameConstructor]
        [TDAttribute<TestContextA>(Binding.Unique, Fulfillment.SelfCreated, "contextA")]
        private class TBInvalidParameterNameConstructorBehavior
        {
            protected TBInvalidParameterNameConstructorBehavior(out TestContextA a) => a = new();
        }

        [TBInvalidParameterTypeConstructor]
        [TDAttribute<TestContextA>(Binding.Unique, Fulfillment.SelfCreated, "contextA")]
        private class TestInvalidParameterTypeConstructorBehavior
        {
            protected TestInvalidParameterTypeConstructorBehavior(out TestContextB contextA) => 
                contextA = new();
        }

        [TBMissingConstructor]
        [TDAttribute<TestContextA>(Binding.Unique, Fulfillment.SelfCreated, "contextA")]
        private class TestMissingConstructorBehavior { }

        [TBNonContextDependency]
        [TDAttribute<TestNonContext>(Binding.Unique, Fulfillment.SelfCreated, "invalidContext")]
        private class TestInvalidDependencyBehavior
        {
            protected TestInvalidDependencyBehavior(out TestNonContext invalidContext) =>
                invalidContext = new();
        }

        [TBNonOutParameterConstructor]
        [TDAttribute<TestContextA>(Binding.Unique, Fulfillment.SelfCreated, "contextA")]
        private class TestNonOutParameterConstructorBehavior
        {
            protected TestNonOutParameterConstructorBehavior(TestContextA contextA) { }
        }

        private class TestNonBehavior { }


        [TC]
        private class TestContextA
        {
            public ContextState<int> Int { get; set; } = 10;
        }

        [TC]
        private class TestContextB { }

        private class TestNonContext { }
        #endregion



        #region Get Behavior Constructor
        [Test]
        public void GetBehaviorConstructor_HasDefinedConstructor()
        {
            Evaluator<TCAttribute, TBAttribute, TDAttribute> evaluator = new();
            evaluator.Initialize();

            ConstructorInfo constructor = evaluator.GetBehaviorConstructor(typeof(TestBehaviorA));
            Assert.IsNotNull(constructor);
        }

        [Test]
        public void GetBehaviorConstructor_HasDefaultConstructor()
        {
            Evaluator<TCAttribute, TBAttribute, TDAttribute> evaluator = new();
            evaluator.Initialize();

            ConstructorInfo constructor = evaluator.GetBehaviorConstructor(typeof(TestBehaviorB));
            Assert.IsNotNull(constructor);
        }

        [Test]
        public void GetBehaviorConstructor_InvalidBehavior()
        {
            Evaluator<TCAttribute, UnusedTBAttribute, TDAttribute> evaluator = new();
            evaluator.Initialize();

            Assert.Throws<ArgumentException>(() =>
                evaluator.GetBehaviorConstructor(typeof(TestBehaviorA)));
        }

        [Test]
        public void GetBehaviorConstructor_UninitializedThrowsException()
        {
            Evaluator<TCAttribute, TBAttribute, TDAttribute> evaluator = new();

            Assert.Throws<InvalidOperationException>(() =>
                evaluator.GetBehaviorConstructor(typeof(TestBehaviorA)));
        }
        #endregion

        #region Get Behavior Types
        [Test]
        public void GetBehaviorTypes_HasBehaviorTypes()
        {
            Evaluator<TCAttribute, TBAttribute, TDAttribute> evaluator = new();
            evaluator.Initialize();

            Type[] behaviors = evaluator.GetBehaviorTypes();

            Assert.AreEqual(2, behaviors.Length);
            Assert.Contains(typeof(TestBehaviorA), behaviors);
            Assert.Contains(typeof(TestBehaviorB), behaviors);
        }

        [Test]
        public void GetBehaviorTypes_HasNoBehaviorTypes()
        {
            Evaluator<TCAttribute, UnusedTBAttribute, TDAttribute> evaluator = new();
            evaluator.Initialize();

            Assert.IsEmpty(evaluator.GetBehaviorTypes());
        }

        [Test]
        public void GetBehaviorTypes_UninitializedThrowsException()
        {
            Evaluator<TCAttribute, TBAttribute, TDAttribute> evaluator = new();

            Assert.Throws<InvalidOperationException>(() =>
                evaluator.GetBehaviorTypes());
        }
        #endregion

        #region Get Bindable State Infos
        [Test]
        public void GetBindableStateInfos_HasStates()
        {
            Evaluator<TCAttribute, TBAttribute, TDAttribute> evaluator = new();
            evaluator.Initialize();

            PropertyInfo[] infos = evaluator.GetBindableStateInfos(typeof(TestContextA));

            Assert.AreEqual(1, infos.Length);
            Assert.AreEqual(nameof(TestContextA.Int), infos[0].Name);
        }

        [Test]
        public void GetBindableStateInfos_HasNoStates()
        {
            Evaluator<TCAttribute, TBAttribute, TDAttribute> evaluator = new();
            evaluator.Initialize();

            PropertyInfo[] infos = evaluator.GetBindableStateInfos(typeof(TestContextB));

            Assert.IsEmpty(infos);
        }

        [Test]
        public void GetBindableStateInfos_NonContext()
        {
            Evaluator<TCAttribute, TBAttribute, TDAttribute> evaluator = new();
            evaluator.Initialize();

            Assert.Throws<ArgumentException>(() =>
                evaluator.GetBindableStateInfos(typeof(TestNonContext)));
        }

        [Test]
        public void GetBindableStateInfos_UninitializedThrowsException()
        {
            Evaluator<TCAttribute, TBAttribute, TDAttribute> evaluator = new();

            Assert.Throws<InvalidOperationException>(() =>
                evaluator.GetBindableStateInfos(typeof(TestContextB)));
        }
        #endregion

        #region Get Context Types
        [Test]
        public void GetContextTypes_HasContextTypes()
        {
            Evaluator<TCAttribute, TBAttribute, TDAttribute> evaluator = new();
            evaluator.Initialize();

            Type[] contexts = evaluator.GetContextTypes();

            Assert.AreEqual(2, contexts.Length);
            Assert.Contains(typeof(TestContextA), contexts);
            Assert.Contains(typeof(TestContextB), contexts);
        }

        [Test]
        public void GetContextTypes_HasNoContextTypes()
        {
            Evaluator<UnusedTCAttribute, UnusedTBAttribute, TDAttribute> evaluator = new();
            evaluator.Initialize();

            Assert.IsEmpty(evaluator.GetContextTypes());
        }

        [Test]
        public void GetContextTypes_UninitializedThrowsException()
        {
            Evaluator<TCAttribute, TBAttribute, TDAttribute> evaluator = new();

            Assert.Throws<InvalidOperationException>(() =>
                evaluator.GetContextTypes());
        }
        #endregion

        #region Get Initialization Behavior Types
        [Test]
        public void GetInitializationBehaviorTypes_HasBehaviors()
        {
            Evaluator<TCAttribute, TBAttribute, TDAttribute> evaluator = new();
            evaluator.Initialize();

            Type[] behaviors = evaluator.GetInitializationBehaviorTypes();

            Assert.AreEqual(2, behaviors.Length);
            Assert.Contains(typeof(TestBehaviorA), behaviors);
            Assert.Contains(typeof(TestBehaviorB), behaviors);
        }

        [Test]
        public void GetInitializationBehaviorTypes_HasNoBehaviors()
        {
            Evaluator<TCAttribute, UnusedTBAttribute, TDAttribute> evaluator = new();
            evaluator.Initialize();

            Assert.IsEmpty(evaluator.GetInitializationBehaviorTypes());
        }

        [Test]
        public void GetInitializationBehaviorTypes_UninitializedThrowsException()
        {
            Evaluator<TCAttribute, TBAttribute, TDAttribute> evaluator = new();

            Assert.Throws<InvalidOperationException>(() =>
                evaluator.GetInitializationBehaviorTypes());
        }
        #endregion

        #region Is Context Type
        [Test]
        public void IsContextType_InvalidContextType()
        {
            Evaluator<TCAttribute, TBAttribute, TDAttribute> evaluator = new();
            evaluator.Initialize();

            Assert.IsFalse(evaluator.IsContextType(typeof(TestNonContext)));
        }

        [Test]
        public void IsContextType_ValidContextType()
        {
            Evaluator<TCAttribute, TBAttribute, TDAttribute> evaluator = new();
            evaluator.Initialize();

            Assert.IsTrue(evaluator.IsContextType(typeof(TestContextA)));
        }

        [Test]
        public void IsContextType_UninitializedThrowsException()
        {
            Evaluator<TCAttribute, TBAttribute, TDAttribute> evaluator = new();

            Assert.Throws<InvalidOperationException>(() =>
                evaluator.IsContextType(typeof(TestContextA)));
        }
        #endregion

        #region Initialization
        [Test]
        public void Initialization_InvalidDependencyBehavior()
        {
            Evaluator<TCAttribute, TBNonContextDependencyAttribute, TDAttribute> evaluator = new();

            Assert.Throws<InvalidOperationException>(() => 
                evaluator.Initialize());
        }

        [Test]
        public void Initialization_InvalidParameterCountConstructor()
        {
            Evaluator<TCAttribute, TBInvalidParameterCountConstructorAttribute, TDAttribute> 
                evaluator = new();

            Assert.Throws<InvalidOperationException>(() =>
                evaluator.Initialize());
        }

        [Test]
        public void Initialization_InvalidParameterNameConstructor()
        {
            Evaluator<TCAttribute, TBInvalidParameterNameConstructorAttribute, TDAttribute>
                evaluator = new();

            Assert.Throws<InvalidOperationException>(() =>
                evaluator.Initialize());
        }

        [Test]
        public void Initialization_InvalidParameterTypeConstructor()
        {
            Evaluator<TCAttribute, TBInvalidParameterTypeConstructorAttribute, TDAttribute>
                evaluator = new();

            Assert.Throws<InvalidOperationException>(() =>
                evaluator.Initialize());
        }

        [Test]
        public void Initialization_MissingConstructor()
        {
            Evaluator<TCAttribute, TBMissingConstructorAttribute, TDAttribute> evaluator = new();

            Assert.Throws<InvalidOperationException>(() =>
                evaluator.Initialize());
        }

        [Test]
        public void Initialization_NonContextDependency()
        {
            Evaluator<TCAttribute, TBNonContextDependencyAttribute, TDAttribute> evaluator = new();

            Assert.Throws<InvalidOperationException>(() =>
                evaluator.Initialize());
        }

        [Test]
        public void Initialization_NonOutParameterConstructor()
        {
            Evaluator<TCAttribute, TBNonOutParameterConstructorAttribute, TDAttribute> 
                evaluator = new();

            Assert.Throws<InvalidOperationException>(() =>
                evaluator.Initialize());
        }
        #endregion
    }
}
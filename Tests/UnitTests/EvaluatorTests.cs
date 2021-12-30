using NUnit.Framework;
using System.Reflection;
using Tests.Constructs;

namespace Tests
{
    public class EvaluatorTests
    {
        #region Get Behavior Constructor
        [Test]
        public void GetBehaviorConstructor_HasDefinedConstructor()
        {
            Evaluator<TCAttribute, TBAttribute, TDAttribute, TOAttribute> evaluator = new();
            evaluator.Initialize();

            ConstructorInfo constructor = evaluator.GetBehaviorConstructor(typeof(TestBehaviorA));
            Assert.IsNotNull(constructor);
        }

        [Test]
        public void GetBehaviorConstructor_HasDefaultConstructor()
        {
            Evaluator<TCAttribute, TBAttribute, TDAttribute, TOAttribute> evaluator = new();
            evaluator.Initialize();

            ConstructorInfo constructor = evaluator.GetBehaviorConstructor(typeof(TestBehaviorB));
            Assert.IsNotNull(constructor);
        }

        [Test]
        public void GetBehaviorConstructor_InvalidBehavior()
        {
            Evaluator<TCAttribute, UnusedTBAttribute, TDAttribute, TOAttribute> evaluator = new();
            evaluator.Initialize();

            Assert.Throws<ArgumentException>(() =>
                evaluator.GetBehaviorConstructor(typeof(TestBehaviorA)));
        }

        [Test]
        public void GetBehaviorConstructor_UninitializedThrowsException()
        {
            Evaluator<TCAttribute, TBAttribute, TDAttribute, TOAttribute> evaluator = new();

            Assert.Throws<InvalidOperationException>(() =>
                evaluator.GetBehaviorConstructor(typeof(TestBehaviorA)));
        }
        #endregion

        #region Get Behavior Types
        [Test]
        public void GetBehaviorTypes_HasBehaviorTypes()
        {
            Evaluator<TCAttribute, TBAttribute, TDAttribute, TOAttribute> evaluator = new();
            evaluator.Initialize();

            Type[] behaviors = evaluator.GetBehaviorTypes();

            Assert.AreEqual(2, behaviors.Length);
            Assert.Contains(typeof(TestBehaviorA), behaviors);
            Assert.Contains(typeof(TestBehaviorB), behaviors);
        }

        [Test]
        public void GetBehaviorTypes_HasNoBehaviorTypes()
        {
            Evaluator<TCAttribute, UnusedTBAttribute, TDAttribute, TOAttribute> evaluator = new();
            evaluator.Initialize();

            Assert.IsEmpty(evaluator.GetBehaviorTypes());
        }

        [Test]
        public void GetBehaviorTypes_UninitializedThrowsException()
        {
            Evaluator<TCAttribute, TBAttribute, TDAttribute, TOAttribute> evaluator = new();

            Assert.Throws<InvalidOperationException>(() =>
                evaluator.GetBehaviorTypes());
        }
        #endregion

        #region Get Bindable State Infos
        [Test]
        public void GetBindableStateInfos_HasStates()
        {
            Evaluator<TCAttribute, TBAttribute, TDAttribute, TOAttribute> evaluator = new();
            evaluator.Initialize();

            PropertyInfo[] infos = evaluator.GetBindableStateInfos(typeof(TestContextA));

            Assert.AreEqual(1, infos.Length);
            Assert.AreEqual(nameof(TestContextA.Int), infos[0].Name);
        }

        [Test]
        public void GetBindableStateInfos_HasNoStates()
        {
            Evaluator<TCAttribute, TBAttribute, TDAttribute, TOAttribute> evaluator = new();
            evaluator.Initialize();

            PropertyInfo[] infos = evaluator.GetBindableStateInfos(typeof(TestContextB));

            Assert.IsEmpty(infos);
        }

        [Test]
        public void GetBindableStateInfos_NonContext()
        {
            Evaluator<TCAttribute, TBAttribute, TDAttribute, TOAttribute> evaluator = new();
            evaluator.Initialize();

            Assert.Throws<ArgumentException>(() =>
                evaluator.GetBindableStateInfos(typeof(TestNonContext)));
        }

        [Test]
        public void GetBindableStateInfos_UninitializedThrowsException()
        {
            Evaluator<TCAttribute, TBAttribute, TDAttribute, TOAttribute> evaluator = new();

            Assert.Throws<InvalidOperationException>(() =>
                evaluator.GetBindableStateInfos(typeof(TestContextB)));
        }
        #endregion

        #region Get Context Types
        [Test]
        public void GetContextTypes_HasContextTypes()
        {
            Evaluator<TCAttribute, TBAttribute, TDAttribute, TOAttribute> evaluator = new();
            evaluator.Initialize();

            Type[] contexts = evaluator.GetContextTypes();

            Assert.AreEqual(3, contexts.Length);
            Assert.Contains(typeof(TestContextA), contexts);
            Assert.Contains(typeof(TestContextB), contexts);
            Assert.Contains(typeof(TestContextC), contexts);
        }

        [Test]
        public void GetContextTypes_HasNoContextTypes()
        {
            Evaluator<UnusedTCAttribute, UnusedTBAttribute, 
                TDAttribute, TOAttribute> evaluator = new();
            evaluator.Initialize();

            Assert.IsEmpty(evaluator.GetContextTypes());
        }

        [Test]
        public void GetContextTypes_UninitializedThrowsException()
        {
            Evaluator<TCAttribute, TBAttribute, TDAttribute, TOAttribute> evaluator = new();

            Assert.Throws<InvalidOperationException>(() =>
                evaluator.GetContextTypes());
        }
        #endregion

        #region Get Initialization Behavior Types
        [Test]
        public void GetInitializationBehaviorTypes_HasBehaviors()
        {
            Evaluator<TCAttribute, TBAttribute, TDAttribute, TOAttribute> evaluator = new();
            evaluator.Initialize();

            Type[] behaviors = evaluator.GetInitializationBehaviorTypes();

            Assert.AreEqual(2, behaviors.Length);
            Assert.Contains(typeof(TestBehaviorA), behaviors);
            Assert.Contains(typeof(TestBehaviorB), behaviors);
        }

        [Test]
        public void GetInitializationBehaviorTypes_HasNoBehaviors()
        {
            Evaluator<TCAttribute, UnusedTBAttribute, TDAttribute, TOAttribute> evaluator = new();
            evaluator.Initialize();

            Assert.IsEmpty(evaluator.GetInitializationBehaviorTypes());
        }

        [Test]
        public void GetInitializationBehaviorTypes_UninitializedThrowsException()
        {
            Evaluator<TCAttribute, TBAttribute, TDAttribute, TOAttribute> evaluator = new();

            Assert.Throws<InvalidOperationException>(() =>
                evaluator.GetInitializationBehaviorTypes());
        }
        #endregion

        #region Get On Change Operations
        [Test]
        public void GetOnChangeOperations_InvalidBehaviorThrowsException()
        {
            Evaluator<TCAttribute, UnusedTBAttribute, TDAttribute, TOAttribute> evaluator = new();
            evaluator.Initialize();

            Assert.Throws<ArgumentException>(() =>
                evaluator.GetOnChangeOperations(typeof(TestBehaviorA),
                    TestBehaviorA.ContextAName));
        }

        [Test]
        public void GetOnChangeOperations_InvalidContextProvidesNoOperations()
        {
            Evaluator<TCAttribute, TBAttribute, TDAttribute, TOAttribute> evaluator = new();
            evaluator.Initialize();

            MethodInfo[] operations = evaluator.GetOnChangeOperations(typeof(TestBehaviorA),
                "InvalidContextName");

            Assert.IsEmpty(operations);
        }

        [Test]
        public void GetOnChangeOperations_InvalidStateProvidesNoOperations()
        {
            Evaluator<TCAttribute, TBAttribute, TDAttribute, TOAttribute> evaluator = new();
            evaluator.Initialize();

            MethodInfo[] operations = evaluator.GetOnChangeOperations(typeof(TestBehaviorA),
                TestBehaviorA.ContextBName, "InvalidStateName");

            Assert.IsEmpty(operations);
        }

        [Test]
        public void GetOnChangeOperations_NullContextThrowsException()
        {
            Evaluator<TCAttribute, TBAttribute, TDAttribute, TOAttribute> evaluator = new();
            evaluator.Initialize();

            Assert.Throws<ArgumentNullException>(() =>
                evaluator.GetOnChangeOperations(typeof(TestBehaviorA), null));
        }

        [Test]
        public void GetOnChangeOperations_ProvidesContextOnChangeOperations_HasNoOperations()
        {
            Evaluator<TCAttribute, TBAttribute, TDAttribute, TOAttribute> evaluator = new();
            evaluator.Initialize();

            MethodInfo[] operations = evaluator.GetOnChangeOperations(typeof(TestBehaviorA),
                TestBehaviorA.ContextAName);

            Assert.IsEmpty(operations);
        }

        [Test]
        public void GetOnChangeOperations_ProvidesContextOnChangeOperations_HasOperations()
        {
            Evaluator<TCAttribute, TBAttribute, TDAttribute, TOAttribute> evaluator = new();
            evaluator.Initialize();

            MethodInfo?[] expectedOperations = new MethodInfo?[]
            {
                typeof(TestBehaviorA).GetMethod(nameof(TestBehaviorA.OnContextBChange)),
            };

            MethodInfo[] operations = evaluator.GetOnChangeOperations(typeof(TestBehaviorA),
                TestBehaviorA.ContextBName);

            Assert.AreEqual(expectedOperations.Length, operations.Length);
            Assert.Contains(expectedOperations[0], operations);
        }

        [Test]
        public void GetOnChangeOperations_ProvidesStateOnChangeOperations_HasNoOperations()
        {
            Evaluator<TCAttribute, TBAttribute, TDAttribute, TOAttribute> evaluator = new();
            evaluator.Initialize();

            MethodInfo[] operations = evaluator.GetOnChangeOperations(typeof(TestBehaviorA),
                TestBehaviorA.ContextCName, nameof(TestContextC.Int));

            Assert.IsEmpty(operations);
        }

        [Test]
        public void GetOnChangeOperations_ProvidesStateOnChangeOperations_HasOperations()
        {
            Evaluator<TCAttribute, TBAttribute, TDAttribute, TOAttribute> evaluator = new();
            evaluator.Initialize();

            MethodInfo?[] expectedOperations = new MethodInfo?[]
            {
                typeof(TestBehaviorA).GetMethod("OnContextAIntChange",
                BindingFlags.Instance | BindingFlags.NonPublic)
            };

            MethodInfo[] operations = evaluator.GetOnChangeOperations(typeof(TestBehaviorA),
                TestBehaviorA.ContextAName, nameof(TestContextA.Int));

            Assert.AreEqual(expectedOperations.Length, operations.Length);
            Assert.Contains(expectedOperations[0], operations);

        }

        [Test]
        public void GetOnChangeOperations_UninitializedThrowsException()
        {
            Evaluator<TCAttribute, TBAttribute, TDAttribute, TOAttribute> evaluator = new();

            Assert.Throws<InvalidOperationException>(() =>
                evaluator.GetOnChangeOperations(typeof(TestBehaviorA),
                    TestBehaviorA.ContextAName));
        }
        #endregion

        #region Is Context Type
        [Test]
        public void IsContextType_InvalidContextType()
        {
            Evaluator<TCAttribute, TBAttribute, TDAttribute, TOAttribute> evaluator = new();
            evaluator.Initialize();

            Assert.IsFalse(evaluator.IsContextType(typeof(TestNonContext)));
        }

        [Test]
        public void IsContextType_ValidContextType()
        {
            Evaluator<TCAttribute, TBAttribute, TDAttribute, TOAttribute> evaluator = new();
            evaluator.Initialize();

            Assert.IsTrue(evaluator.IsContextType(typeof(TestContextA)));
        }

        [Test]
        public void IsContextType_UninitializedThrowsException()
        {
            Evaluator<TCAttribute, TBAttribute, TDAttribute, TOAttribute> evaluator = new();

            Assert.Throws<InvalidOperationException>(() =>
                evaluator.IsContextType(typeof(TestContextA)));
        }
        #endregion

        #region Initialization

        // TODO : 10 :: Check for invalid setup for on change operations.

        [Test]
        public void Initialization_InvalidDependencyBehavior()
        {
            Evaluator<TCAttribute, TBNonContextDependencyAttribute, 
                TDAttribute, TOAttribute> evaluator = new();

            Assert.Throws<InvalidOperationException>(() =>
                evaluator.Initialize());
        }

        [Test]
        public void Initialization_InvalidParameterCountConstructor()
        {
            Evaluator<TCAttribute, TBInvalidParameterCountConstructorAttribute, 
                TDAttribute, TOAttribute> evaluator = new();

            Assert.Throws<InvalidOperationException>(() =>
                evaluator.Initialize());
        }

        [Test]
        public void Initialization_InvalidParameterNameConstructor()
        {
            Evaluator<TCAttribute, TBInvalidParameterNameConstructorAttribute, 
                TDAttribute, TOAttribute> evaluator = new();

            Assert.Throws<InvalidOperationException>(() =>
                evaluator.Initialize());
        }

        [Test]
        public void Initialization_InvalidParameterTypeConstructor()
        {
            Evaluator<TCAttribute, TBInvalidParameterTypeConstructorAttribute, 
                TDAttribute, TOAttribute> evaluator = new();

            Assert.Throws<InvalidOperationException>(() =>
                evaluator.Initialize());
        }

        [Test]
        public void Initialization_MissingConstructor()
        {
            Evaluator<TCAttribute, TBMissingConstructorAttribute, 
                TDAttribute, TOAttribute> evaluator = new();

            Assert.Throws<InvalidOperationException>(() =>
                evaluator.Initialize());
        }

        [Test]
        public void Initialization_NonContextDependency()
        {
            Evaluator<TCAttribute, TBNonContextDependencyAttribute, 
                TDAttribute, TOAttribute> evaluator = new();

            Assert.Throws<InvalidOperationException>(() =>
                evaluator.Initialize());
        }

        [Test]
        public void Initialization_NonOutParameterConstructor()
        {
            Evaluator<TCAttribute, TBNonOutParameterConstructorAttribute, 
                TDAttribute, TOAttribute> evaluator = new();

            Assert.Throws<InvalidOperationException>(() =>
                evaluator.Initialize());
        }
        #endregion
    }
}
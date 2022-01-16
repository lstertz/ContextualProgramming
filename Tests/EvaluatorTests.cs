using NUnit.Framework;
using System.Reflection;

namespace EvaluatorTests
{
    #region Shared Constructs
    public class NonBehavior { }

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
    #endregion

    public class GetBehaviorConstructor
    {
        public static Evaluator<TCAttribute, T, TDAttribute, TOAttribute> GetEvaluator<T>()
            where T : BaseBehaviorAttribute => new();

        public class DefinedConstructorBehaviorAttribute : BaseBehaviorAttribute { }
        [DefinedConstructorBehavior]
        [TD<TestContextA>(Binding.Unique, Fulfillment.SelfCreated, "contextA")]
        public class DefinedConstructorBehavior
        {
            public DefinedConstructorBehavior(out TestContextA contextA) => contextA = new();
        }

        public class DefaultConstructorBehaviorAttribute : BaseBehaviorAttribute { }
        [DefaultConstructorBehavior]
        public class DefaultConstructorBehavior { }

        public class TCAttribute : BaseContextAttribute { }
        [TC]
        public class TestContextA { }

        public class TOAttribute : BaseOperationAttribute { }

        [Test]
        public void HasDefinedConstructor()
        {
            var evaluator = GetEvaluator<DefinedConstructorBehaviorAttribute>();
            evaluator.Initialize();

            ConstructorInfo constructor = evaluator.GetBehaviorConstructor(
                typeof(DefinedConstructorBehavior));
            Assert.IsNotNull(constructor);
        }

        [Test]
        public void HasDefaultConstructor()
        {
            var evaluator = GetEvaluator<DefaultConstructorBehaviorAttribute>();
            evaluator.Initialize();

            ConstructorInfo constructor = evaluator.GetBehaviorConstructor(
                typeof(DefaultConstructorBehavior));
            Assert.IsNotNull(constructor);
        }

        [Test]
        public void NonBehaviorThrowsException()
        {
            var evaluator = GetEvaluator<DefinedConstructorBehaviorAttribute>();
            evaluator.Initialize();

            Assert.Throws<ArgumentException>(() =>
                evaluator.GetBehaviorConstructor(typeof(NonBehavior)));
        }

        [Test]
        public void UninitializedThrowsException()
        {
            var evaluator = GetEvaluator<DefinedConstructorBehaviorAttribute>();

            Assert.Throws<InvalidOperationException>(() =>
                evaluator.GetBehaviorConstructor(typeof(DefinedConstructorBehavior)));
        }
    }

    public class GetBehaviorRequiredDependencies
    {
        public static Evaluator<TCAttribute, T, TDAttribute, TOAttribute> GetEvaluator<T>()
            where T : BaseBehaviorAttribute => new();

        public class ExcludesSelfCreatedDependenciesBehaviorAttribute : BaseBehaviorAttribute { }
        [ExcludesSelfCreatedDependenciesBehavior]
        [TD<TestContextA>(Binding.Unique, Fulfillment.Existing, Dep1Name)]
        [TD<TestContextB>(Binding.Unique, Fulfillment.SelfCreated, Dep2Name)]
        public class ExcludesSelfCreatedDependenciesBehavior
        {
            public const string Dep1Name = "contextA";
            public const string Dep2Name = "contextB";

            public ExcludesSelfCreatedDependenciesBehavior(out TestContextB contextB) => 
                contextB = new();
        }

        public class HasExistingDependenciesBehaviorAttribute : BaseBehaviorAttribute { }
        [HasExistingDependenciesBehavior]
        [TD<TestContextA>(Binding.Unique, Fulfillment.Existing, Dep1Name)]
        [TD<TestContextB>(Binding.Unique, Fulfillment.Existing, Dep2Name)]
        public class HasExistingDependenciesBehavior
        {
            public const string Dep1Name = "contextA";
            public const string Dep2Name = "contextB";
        }

        public class IncludesDuplicateExistingDependenciesBehaviorAttribute : 
            BaseBehaviorAttribute { }
        [IncludesDuplicateExistingDependenciesBehavior]
        [TD<TestContextA>(Binding.Unique, Fulfillment.Existing, Dep1Name)]
        [TD<TestContextA>(Binding.Unique, Fulfillment.Existing, Dep2Name)]
        public class IncludesDuplicateExistingDependenciesBehavior
        {
            public const string Dep1Name = "contextA1";
            public const string Dep2Name = "contextA2";
        }

        public class HasNoExistingDependenciesBehaviorAttribute : BaseBehaviorAttribute { }
        [HasNoExistingDependenciesBehavior]
        [TD<TestContextA>(Binding.Unique, Fulfillment.SelfCreated, "contextA")]
        public class HasNoExistingDependenciesBehavior 
        {
            public HasNoExistingDependenciesBehavior(out TestContextA contextA) =>
                contextA = new();
        }

        public class TCAttribute : BaseContextAttribute { }
        [TC]
        public class TestContextA { }
        [TC]
        public class TestContextB { }

        public class TOAttribute : BaseOperationAttribute { }

        [Test]
        public void ExcludesSelfCreatedDependencies()
        {
            var evaluator = GetEvaluator<ExcludesSelfCreatedDependenciesBehaviorAttribute>();
            evaluator.Initialize();

            Tuple<string, Type>[] dependencies = evaluator.GetBehaviorRequiredDependencies(
                typeof(ExcludesSelfCreatedDependenciesBehavior));

            Assert.AreEqual(1, dependencies.Length);
            Assert.AreEqual(ExcludesSelfCreatedDependenciesBehavior.Dep1Name, 
                dependencies[0].Item1);
            Assert.AreEqual(typeof(TestContextA), dependencies[0].Item2);
        }

        [Test]
        public void HasExistingDependencies()
        {
            var evaluator = GetEvaluator<HasExistingDependenciesBehaviorAttribute>();
            evaluator.Initialize();

            Tuple<string, Type>[] dependencies = evaluator.GetBehaviorRequiredDependencies(
                typeof(HasExistingDependenciesBehavior));
            
            Assert.AreEqual(2, dependencies.Length);
            Assert.AreEqual(HasExistingDependenciesBehavior.Dep1Name, dependencies[0].Item1);
            Assert.AreEqual(typeof(TestContextA), dependencies[0].Item2);
            Assert.AreEqual(HasExistingDependenciesBehavior.Dep2Name, dependencies[1].Item1);
            Assert.AreEqual(typeof(TestContextB), dependencies[1].Item2);
        }

        [Test]
        public void HasNoExistingDependencies()
        {
            var evaluator = GetEvaluator<HasNoExistingDependenciesBehaviorAttribute>();
            evaluator.Initialize();

            Tuple<string, Type>[] dependencies = evaluator.GetBehaviorRequiredDependencies(
                typeof(HasNoExistingDependenciesBehavior));
            Assert.IsEmpty(dependencies);
        }

        [Test]
        public void IncludesDuplicateExistingDependencies()
        {
            var evaluator = GetEvaluator<IncludesDuplicateExistingDependenciesBehaviorAttribute>();
            evaluator.Initialize();

            Tuple<string, Type>[] dependencies = evaluator.GetBehaviorRequiredDependencies(
                typeof(IncludesDuplicateExistingDependenciesBehavior));

            Assert.AreEqual(2, dependencies.Length);
            Assert.AreEqual(IncludesDuplicateExistingDependenciesBehavior.Dep1Name, 
                dependencies[0].Item1);
            Assert.AreEqual(typeof(TestContextA), dependencies[0].Item2);
            Assert.AreEqual(IncludesDuplicateExistingDependenciesBehavior.Dep2Name, 
                dependencies[1].Item1);
            Assert.AreEqual(typeof(TestContextA), dependencies[1].Item2);
        }

        [Test]
        public void NonBehaviorThrowsException()
        {
            var evaluator = GetEvaluator<HasExistingDependenciesBehaviorAttribute>();
            evaluator.Initialize();

            Assert.Throws<ArgumentException>(() =>
                evaluator.GetBehaviorRequiredDependencies(typeof(NonBehavior)));
        }

        [Test]
        public void UninitalizedThrowsException()
        {
            var evaluator = GetEvaluator<HasExistingDependenciesBehaviorAttribute>();

            Assert.Throws<InvalidOperationException>(() =>
                evaluator.GetBehaviorRequiredDependencies(
                    typeof(HasExistingDependenciesBehavior)));
        }
    }

    public class GetBehaviorTypes
    {
        public static Evaluator<TCAttribute, T, TDAttribute, TOAttribute> GetEvaluator<T>()
            where T : BaseBehaviorAttribute => new();

        public class HasBehaviorTypesBehaviorAttribute : BaseBehaviorAttribute { }
        [HasBehaviorTypesBehavior]
        public class HasBehaviorTypesBehaviorA { }
        [HasBehaviorTypesBehavior]
        public class HasBehaviorTypesBehaviorB { }

        public class HasNoBehaviorTypesBehaviorAttribute : BaseBehaviorAttribute { }

        public class TCAttribute : BaseContextAttribute { }
        public class TOAttribute : BaseOperationAttribute { }

        [Test]
        public void HasBehaviorTypes()
        {
            var evaluator = GetEvaluator<HasBehaviorTypesBehaviorAttribute>();
            evaluator.Initialize();

            Type[] behaviors = evaluator.GetBehaviorTypes();

            Assert.AreEqual(2, behaviors.Length);
            Assert.Contains(typeof(HasBehaviorTypesBehaviorA), behaviors);
            Assert.Contains(typeof(HasBehaviorTypesBehaviorB), behaviors);
        }

        [Test]
        public void HasNoBehaviorTypes()
        {
            var evaluator = GetEvaluator<HasNoBehaviorTypesBehaviorAttribute>();
            evaluator.Initialize();

            Assert.IsEmpty(evaluator.GetBehaviorTypes());
        }

        [Test]
        public void UninitializedThrowsException()
        {
            var evaluator = GetEvaluator<HasBehaviorTypesBehaviorAttribute>();

            Assert.Throws<InvalidOperationException>(() =>
                evaluator.GetBehaviorTypes());
        }
    }

    public class GetBindableStateInfos
    {
        public static Evaluator<T, TBAttribute, TDAttribute, TOAttribute> GetEvaluator<T>()
            where T : BaseContextAttribute => new();

        public class HasStatesContextAttribute : BaseContextAttribute { }
        [HasStatesContext]
        public class HasStatesContext
        {
            public ContextState<int> Int { get; init; } = 0;
        }
        public class HasNoStatesContextAttribute : BaseContextAttribute { }
        [HasNoStatesContext]

        public class HasNoStatesContext { }

        public class NonContext { }

        public class TBAttribute : BaseBehaviorAttribute { }
        public class TOAttribute : BaseOperationAttribute { }

        [Test]
        public void HasStates()
        {
            var evaluator = GetEvaluator<HasStatesContextAttribute>();
            evaluator.Initialize();

            PropertyInfo[] infos = evaluator.GetBindableStateInfos(typeof(HasStatesContext));

            Assert.AreEqual(1, infos.Length);
            Assert.AreEqual(nameof(HasStatesContext.Int), infos[0].Name);
        }

        [Test]
        public void HasNoStates()
        {
            var evaluator = GetEvaluator<HasNoStatesContextAttribute>();
            evaluator.Initialize();

            PropertyInfo[] infos = evaluator.GetBindableStateInfos(typeof(HasNoStatesContext));

            Assert.IsEmpty(infos);
        }

        [Test]
        public void NonContextThrowsException()
        {
            var evaluator = GetEvaluator<HasStatesContextAttribute>();
            evaluator.Initialize();

            Assert.Throws<ArgumentException>(() =>
                evaluator.GetBindableStateInfos(typeof(NonContext)));
        }

        [Test]
        public void UninitializedThrowsException()
        {
            var evaluator = GetEvaluator<HasStatesContextAttribute>();

            Assert.Throws<InvalidOperationException>(() =>
                evaluator.GetBindableStateInfos(typeof(HasStatesContext)));
        }
    }

    public class GetContextTypes
    {
        public static Evaluator<T, TBAttribute, TDAttribute, TOAttribute> GetEvaluator<T>()
            where T : BaseContextAttribute => new();

        public class HasContextTypesContextAttribute : BaseContextAttribute { }
        [HasContextTypesContext]
        public class HasContextTypesContextA { }
        [HasContextTypesContext]
        public class HasContextTypesContextB { }

        public class HasNoContextTypesContextAttribute : BaseContextAttribute { }

        public class TBAttribute : BaseBehaviorAttribute { }
        public class TOAttribute : BaseOperationAttribute { }

        [Test]
        public void HasContextTypes()
        {
            var evaluator = GetEvaluator<HasContextTypesContextAttribute>();
            evaluator.Initialize();

            Type[] contexts = evaluator.GetContextTypes();

            Assert.AreEqual(2, contexts.Length);
            Assert.Contains(typeof(HasContextTypesContextA), contexts);
            Assert.Contains(typeof(HasContextTypesContextB), contexts);
        }

        [Test]
        public void HasNoContextTypes()
        {
            var evaluator = GetEvaluator<HasNoContextTypesContextAttribute>();
            evaluator.Initialize();

            Assert.IsEmpty(evaluator.GetContextTypes());
        }

        [Test]
        public void UninitializedThrowsException()
        {
            var evaluator = GetEvaluator<HasContextTypesContextAttribute>();

            Assert.Throws<InvalidOperationException>(() =>
                evaluator.GetContextTypes());
        }
    }

    public class GetOnChangeOperations
    {
        public static Evaluator<TCAttribute, T, TDAttribute, TOAttribute> GetEvaluator<T>()
            where T : BaseBehaviorAttribute => new();

        public class HasNoOperationsBehaviorAttribute : BaseBehaviorAttribute { }
        [HasNoOperationsBehavior]
        [TD<TestContextA>(Binding.Unique, Fulfillment.SelfCreated, ContextA)]
        public class HasNoOperationsBehavior
        {
            public const string ContextA = "contextA";

            public HasNoOperationsBehavior(out TestContextA contextA) => contextA = new();
        }
        public class HasOperationsBehaviorAttribute : BaseBehaviorAttribute { }
        [HasOperationsBehavior]
        [TD<TestContextA>(Binding.Unique, Fulfillment.SelfCreated, ContextA)]
        public class HasOperationsBehavior
        {
            public const string ContextA = "contextA";

            public HasOperationsBehavior(out TestContextA contextA) => contextA = new();

            [TO]
            [OnChange(ContextA)]
            public void OnContextAChange(TestContextA contextA) { }

            [TO]
            [OnChange(ContextA, nameof(TestContextA.Int))]
            private void OnContextAIntChange(TestContextA contextA) { }
        }

        public class TCAttribute : BaseContextAttribute { }
        [TC]
        public class TestContextA
        {
            public ContextState<int> Int { get; init; } = 0;
        }

        public class TOAttribute : BaseOperationAttribute { }

        [Test]
        public void InvalidContextProvidesNoOperations()
        {
            var evaluator = GetEvaluator<HasOperationsBehaviorAttribute>();
            evaluator.Initialize();

            MethodInfo[] operations = evaluator.GetOnChangeOperations(
                typeof(HasOperationsBehavior), "invalidContextName");

            Assert.IsEmpty(operations);
        }

        [Test]
        public void InvalidStateProvidesNoOperations()
        {
            var evaluator = GetEvaluator<HasOperationsBehaviorAttribute>();
            evaluator.Initialize();

            MethodInfo[] operations = evaluator.GetOnChangeOperations(
                typeof(HasOperationsBehavior), HasOperationsBehavior.ContextA, "invalidState");

            Assert.IsEmpty(operations);
        }

        [Test]
        public void NonBehaviorThrowsException()
        {
            var evaluator = GetEvaluator<HasOperationsBehaviorAttribute>();
            evaluator.Initialize();

            Assert.Throws<ArgumentException>(() =>
                evaluator.GetOnChangeOperations(typeof(NonBehavior), "contextName"));
        }

        [Test]
        public void NullContextThrowsException()
        {
            var evaluator = GetEvaluator<HasOperationsBehaviorAttribute>();
            evaluator.Initialize();

#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            Assert.Throws<ArgumentNullException>(() =>
                evaluator.GetOnChangeOperations(typeof(HasOperationsBehavior), null));
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
        }

        [Test]
        public void ProvidesContextOnChangeOperations_HasNoOperations()
        {
            var evaluator = GetEvaluator<HasNoOperationsBehaviorAttribute>();
            evaluator.Initialize();

            MethodInfo[] operations = evaluator.GetOnChangeOperations(
                typeof(HasNoOperationsBehavior), HasNoOperationsBehavior.ContextA);

            Assert.IsEmpty(operations);
        }

        [Test]
        public void ProvidesContextOnChangeOperations_HasOperations()
        {
            var evaluator = GetEvaluator<HasOperationsBehaviorAttribute>();
            evaluator.Initialize();

            MethodInfo?[] expectedOperations = new MethodInfo?[]
            {
                    typeof(HasOperationsBehavior).GetMethod(
                        nameof(HasOperationsBehavior.OnContextAChange)),
            };

            MethodInfo[] operations = evaluator.GetOnChangeOperations(
                typeof(HasOperationsBehavior), HasOperationsBehavior.ContextA);

            Assert.AreEqual(expectedOperations.Length, operations.Length);
            Assert.Contains(expectedOperations[0], operations);
        }

        [Test]
        public void ProvidesStateOnChangeOperations_HasNoOperations()
        {
            var evaluator = GetEvaluator<HasNoOperationsBehaviorAttribute>();
            evaluator.Initialize();

            MethodInfo[] operations = evaluator.GetOnChangeOperations(
                typeof(HasNoOperationsBehavior),
                HasNoOperationsBehavior.ContextA, nameof(TestContextA.Int));

            Assert.IsEmpty(operations);
        }

        [Test]
        public void ProvidesStateOnChangeOperations_HasOperations()
        {
            var evaluator = GetEvaluator<HasOperationsBehaviorAttribute>();
            evaluator.Initialize();

            MethodInfo?[] expectedOperations = new MethodInfo?[]
            {
                    typeof(HasOperationsBehavior).GetMethod("OnContextAIntChange",
                        BindingFlags.Instance | BindingFlags.NonPublic)
            };

            MethodInfo[] operations = evaluator.GetOnChangeOperations(
                typeof(HasOperationsBehavior), HasOperationsBehavior.ContextA,
                nameof(TestContextA.Int));

            Assert.AreEqual(expectedOperations.Length, operations.Length);
            Assert.Contains(expectedOperations[0], operations);

        }

        [Test]
        public void UninitializedThrowsException()
        {
            var evaluator = GetEvaluator<HasOperationsBehaviorAttribute>();

            Assert.Throws<InvalidOperationException>(() =>
                evaluator.GetOnChangeOperations(typeof(HasOperationsBehavior),
                    HasOperationsBehavior.ContextA));
        }
    }

    public class Initialization
    {
        public static Evaluator<TCAttribute, T1, TDAttribute, T2> GetEvaluator<T1, T2>()
            where T1 : BaseBehaviorAttribute where T2 : BaseOperationAttribute => new();

        public class InvalidDuplicateDependencyNamesBehaviorAttribute : BaseBehaviorAttribute { }
        [InvalidDuplicateDependencyNamesBehavior]
        [TDAttribute<TestContextA>(Binding.Unique, Fulfillment.SelfCreated, "context")]
        [TDAttribute<TestContextB>(Binding.Unique, Fulfillment.Existing, "context")]
        public class InvalidDuplicateDependencyNamesBehavior
        {
            protected InvalidDuplicateDependencyNamesBehavior(out TestContextA context) => 
                context = new();
        }

        public class InvalidExistingDependencyConstructorBehaviorAttribute : 
            BaseBehaviorAttribute { }
        [InvalidExistingDependencyConstructorBehavior]
        [TDAttribute<TestContextA>(Binding.Unique, Fulfillment.SelfCreated, "contextA")]
        [TDAttribute<TestContextB>(Binding.Unique, Fulfillment.Existing, "contextB")]
        public class InvalidExistingDependencyConstructorBehavior
        {
            protected InvalidExistingDependencyConstructorBehavior(
                out TestContextA contextA, out TestContextB contextB)
            {
                contextA = new();
                contextB = new();
            }
        }

        public class InvalidOperationsBehaviorAttribute : BaseBehaviorAttribute { }
        [InvalidOperationsBehavior]
        [TD<TestContextA>(Binding.Unique, Fulfillment.SelfCreated, ContextA)]
        public class InvalidOperationsBehavior
        {
            public const string ContextA = "contextA";

            public InvalidOperationsBehavior(out TestContextA contextA) => contextA = new();

            [InvalidOperationContextNameOperation]
            public void InvalidOperationContextNameOperation(TestContextA a) { }

            [InvalidOperationContextTypeOperation]
            public void InvalidOperationContextTypeOperation(TestContextB contextA) { }

            [InvalidOnChangeContextOperation]
            [OnChange("a")]
            public void InvalidOnChangeContextOperation(TestContextA contextA) { }

            [InvalidOnChangeStateOperation]
            [OnChange(ContextA, "InvalidState")]
            public void InvalidOnChangeStateOperation(TestContextA contextA) { }
        }

        public class TOAttribute : BaseOperationAttribute { }
        public class InvalidOnChangeContextOperationAttribute : BaseOperationAttribute { }
        public class InvalidOnChangeStateOperationAttribute : BaseOperationAttribute { }
        public class InvalidOperationContextNameOperationAttribute : BaseOperationAttribute { }
        public class InvalidOperationContextTypeOperationAttribute : BaseOperationAttribute { }

        public class InvalidParamCountConstructorBehaviorAttribute : BaseBehaviorAttribute { }
        [InvalidParamCountConstructorBehavior]
        [TDAttribute<TestContextA>(Binding.Unique, Fulfillment.SelfCreated, "contextA")]
        public class InvalidParamCountConstructorBehavior
        {
            protected InvalidParamCountConstructorBehavior(
                out TestContextA contextA, out TestContextB contextB)
            {
                contextA = new();
                contextB = new();
            }
        }

        public class InvalidParamNameConstructorBehaviorAttribute : BaseBehaviorAttribute { }
        [InvalidParamNameConstructorBehavior]
        [TDAttribute<TestContextA>(Binding.Unique, Fulfillment.SelfCreated, "contextA")]
        public class InvalidParamNameConstructorBehavior
        {
            protected InvalidParamNameConstructorBehavior(out TestContextA a) => a = new();
        }

        public class InvalidParamTypeConstructorBehaviorAttribute : BaseBehaviorAttribute { }
        [InvalidParamTypeConstructorBehavior]
        [TDAttribute<TestContextA>(Binding.Unique, Fulfillment.SelfCreated, "contextA")]
        public class InvalidParamTypeConstructorBehavior
        {
            protected InvalidParamTypeConstructorBehavior(out TestContextB contextA) =>
                contextA = new();
        }

        public class MissingConstructorBehaviorAttribute : BaseBehaviorAttribute { }
        [MissingConstructorBehavior]
        [TDAttribute<TestContextA>(Binding.Unique, Fulfillment.SelfCreated, "contextA")]
        public class MissingConstructorBehavior { }

        public class NonContextDependencyBehaviorAttribute : BaseBehaviorAttribute { }
        [NonContextDependencyBehavior]
        [TD<NonContext>(Binding.Unique, Fulfillment.SelfCreated, "nonContext")]
        public class NonContextDependencyBehavior
        {
            public NonContextDependencyBehavior(out NonContext nonContext) =>
                nonContext = new();
        }

        public class NonOutParamBehaviorAttribute : BaseBehaviorAttribute { }
        [NonOutParamBehavior]
        [TDAttribute<TestContextA>(Binding.Unique, Fulfillment.SelfCreated, "contextA")]
        public class NonOutParamBehavior
        {
            public NonOutParamBehavior(TestContextA contextA) => contextA = new();
        }

        public class TCAttribute : BaseContextAttribute { }
        [TC]
        public class TestContextA
        {
            public ContextState<int> Int { get; init; } = 0;
        }
        [TC]
        public class TestContextB { }

        public class NonContext { }

        [Test]
        public void InvalidDuplicateDependencyNames()
        {
            var evaluator = GetEvaluator<InvalidDuplicateDependencyNamesBehaviorAttribute,
                TOAttribute>();

            Assert.Throws<InvalidOperationException>(() =>
                evaluator.Initialize());
        }

        [Test]
        public void InvalidExistingDependencyConstructor()
        {
            var evaluator = GetEvaluator<InvalidExistingDependencyConstructorBehaviorAttribute,
                TOAttribute>();

            Assert.Throws<InvalidOperationException>(() =>
                evaluator.Initialize());
        }

        [Test]
        public void InvalidOnChangeContext()
        {
            var evaluator = GetEvaluator<InvalidOperationsBehaviorAttribute,
                InvalidOnChangeContextOperationAttribute>();

            Assert.Throws<InvalidOperationException>(() =>
                evaluator.Initialize());
        }

        [Test]
        public void InvalidOnChangeState()
        {
            var evaluator = GetEvaluator<InvalidOperationsBehaviorAttribute,
                InvalidOnChangeStateOperationAttribute>();

            Assert.Throws<InvalidOperationException>(() =>
                evaluator.Initialize());
        }

        [Test]
        public void InvalidOperationContextName()
        {
            var evaluator = GetEvaluator<InvalidOperationsBehaviorAttribute,
                InvalidOperationContextNameOperationAttribute>();

            Assert.Throws<InvalidOperationException>(() =>
                evaluator.Initialize());
        }

        [Test]
        public void InvalidOperationContextType()
        {
            var evaluator = GetEvaluator<InvalidOperationsBehaviorAttribute,
                InvalidOperationContextTypeOperationAttribute>();

            Assert.Throws<InvalidOperationException>(() =>
                evaluator.Initialize());
        }

        [Test]
        public void InvalidParameterCountConstructor()
        {
            var evaluator = GetEvaluator<InvalidParamCountConstructorBehaviorAttribute,
                TOAttribute>();

            Assert.Throws<InvalidOperationException>(() =>
                evaluator.Initialize());
        }

        [Test]
        public void InvalidParameterNameConstructor()
        {
            var evaluator = GetEvaluator<InvalidParamNameConstructorBehaviorAttribute,
                TOAttribute>();

            Assert.Throws<InvalidOperationException>(() =>
                evaluator.Initialize());
        }

        [Test]
        public void InvalidParameterTypeConstructor()
        {
            var evaluator = GetEvaluator<InvalidParamTypeConstructorBehaviorAttribute,
                TOAttribute>();

            Assert.Throws<InvalidOperationException>(() =>
                evaluator.Initialize());
        }

        [Test]
        public void MissingConstructor()
        {
            var evaluator = GetEvaluator<MissingConstructorBehaviorAttribute, TOAttribute>();

            Assert.Throws<InvalidOperationException>(() =>
                evaluator.Initialize());
        }

        [Test]
        public void NonContextDependency()
        {
            var evaluator = GetEvaluator<NonContextDependencyBehaviorAttribute, TOAttribute>();

            Assert.Throws<InvalidOperationException>(() =>
                evaluator.Initialize());
        }

        [Test]
        public void NonOutParameterConstructor()
        {
            Evaluator<TCAttribute, NonOutParamBehaviorAttribute,
                TDAttribute, TOAttribute> evaluator = new();

            Assert.Throws<InvalidOperationException>(() =>
                evaluator.Initialize());
        }
    }

    public class IsContextType
    {
        public static Evaluator<T, TBAttribute, TDAttribute, TOAttribute> GetEvaluator<T>()
            where T : BaseContextAttribute => new();

        public class InvalidContextTypeContextAttribute : BaseContextAttribute { }
        public class NonContext { }

        public class ValidContextTypeContextAttribute : BaseContextAttribute { }
        [ValidContextTypeContext]
        public class ValidContextTypeContext { }


        public class TBAttribute : BaseBehaviorAttribute { }
        public class TOAttribute : BaseOperationAttribute { }
        [Test]
        public void InvalidContextType()
        {
            var evaluator = GetEvaluator<InvalidContextTypeContextAttribute>();
            evaluator.Initialize();

            Assert.IsFalse(evaluator.IsContextType(typeof(NonContext)));
        }

        [Test]
        public void ValidContextType()
        {
            var evaluator = GetEvaluator<ValidContextTypeContextAttribute>();
            evaluator.Initialize();

            Assert.IsTrue(evaluator.IsContextType(typeof(ValidContextTypeContext)));
        }

        [Test]
        public void UninitializedThrowsException()
        {
            var evaluator = GetEvaluator<ValidContextTypeContextAttribute>();

            Assert.Throws<InvalidOperationException>(() =>
                evaluator.IsContextType(typeof(ValidContextTypeContext)));
        }
    }
}
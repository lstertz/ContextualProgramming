using ContextualProgramming.Internal;
using NUnit.Framework;
using System.Reflection;

namespace EvaluatorTests
{
    #region Shared Constructs
    public class NonBehavior { }

    public abstract class TMAttribute : BaseMutualismAttribute
    {
        protected TMAttribute(string name, Type type) : 
            base(name, Relationship.Exclusive, type) { }
    }
    public class TMAttribute<T> : TMAttribute
    {
        public TMAttribute(string name) : base(name, typeof(T)) { }
    }


    /// <inheritdoc/>
    public class TMSAttribute : BaseMutualAttribute
    {
        /// <inheritdoc/>
        public TMSAttribute(string mutualistName, string stateName) :
            base(mutualistName, stateName)
        { }
    }

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

    public class BuildBehaviorFactory
    {
        public static Evaluator<TCAttribute, TMAttribute, TMSAttribute, T, 
            TDAttribute, TOAttribute> GetEvaluator<T>()
            where T : BaseBehaviorAttribute => new();

        public class ForBehaviorWithExistingAndSelfCreatedDependenciesBehaviorAttribute :
            BaseBehaviorAttribute
        { }
        [ForBehaviorWithExistingAndSelfCreatedDependenciesBehavior]
        [TD<TestContextA>(Binding.Unique, Fulfillment.SelfCreated, "contextA")]
        [TD<TestContextB>(Binding.Unique, Fulfillment.Existing, "contextB")]
        public class ForBehaviorWithExistingAndSelfCreatedDependenciesBehavior
        {
            public ForBehaviorWithExistingAndSelfCreatedDependenciesBehavior(
                out TestContextA contextA) => contextA = new();
        }
        public class ForBehaviorWithExistingDependenciesBehaviorAttribute :
            BaseBehaviorAttribute
        { }
        [ForBehaviorWithExistingDependenciesBehavior]
        [TD<TestContextA>(Binding.Unique, Fulfillment.Existing, "contextA")]
        public class ForBehaviorWithExistingDependenciesBehavior
        {
        }

        public class ForBehaviorWithOnlySelfCreatedDependenciesBehaviorAttribute :
            BaseBehaviorAttribute
        { }
        [ForBehaviorWithOnlySelfCreatedDependenciesBehavior]
        [TD<TestContextA>(Binding.Unique, Fulfillment.SelfCreated, "contextA")]
        public class ForBehaviorWithOnlySelfCreatedDependenciesBehavior
        {
            public ForBehaviorWithOnlySelfCreatedDependenciesBehavior(
                out TestContextA contextA) => contextA = new();
        }

        public class ForBehaviorWithNoDependenciesBehaviorAttribute : BaseBehaviorAttribute { }
        [ForBehaviorWithNoDependenciesBehavior]
        public class ForBehaviorWithNoDependenciesBehavior { }

        public class TCAttribute : BaseContextAttribute { }
        [TC]
        public class TestContextA { }
        [TC]
        public class TestContextB { }

        public class TOAttribute : BaseOperationAttribute { }

        [Test]
        public void ForBehaviorWithExistingAndSelfCreatedDependencies_ProvidesMatchingFactory()
        {
            var evaluator = GetEvaluator<
                ForBehaviorWithExistingAndSelfCreatedDependenciesBehaviorAttribute>();
            evaluator.Initialize();

            IBehaviorFactory factory = evaluator.BuildBehaviorFactory(
                typeof(ForBehaviorWithExistingAndSelfCreatedDependenciesBehavior));

            Assert.IsNotNull(factory);
            Assert.AreEqual(new Type[] { typeof(TestContextB) }, factory.RequiredDependencyTypes);
        }

        [Test]
        public void ForBehaviorWithExistingDependencies_ProvidesMatchingFactory()
        {
            var evaluator = GetEvaluator<ForBehaviorWithExistingDependenciesBehaviorAttribute>();
            evaluator.Initialize();

            IBehaviorFactory factory = evaluator.BuildBehaviorFactory(
                typeof(ForBehaviorWithExistingDependenciesBehavior));

            Assert.IsNotNull(factory);
            Assert.AreEqual(new Type[] { typeof(TestContextA) }, factory.RequiredDependencyTypes);
        }

        [Test]
        public void ForBehaviorWithNoDependencies_ProvidesMatchingFactory()
        {
            var evaluator = GetEvaluator<ForBehaviorWithNoDependenciesBehaviorAttribute>();
            evaluator.Initialize();

            IBehaviorFactory factory = evaluator.BuildBehaviorFactory(
                typeof(ForBehaviorWithNoDependenciesBehavior));

            Assert.IsNotNull(factory);
            Assert.AreEqual(Array.Empty<Type>(), factory.RequiredDependencyTypes);
        }

        [Test]
        public void ForBehaviorWithOnlySelfCreatedDependencies_ProvidesMatchingFactory()
        {
            var evaluator = GetEvaluator<
                ForBehaviorWithOnlySelfCreatedDependenciesBehaviorAttribute>();
            evaluator.Initialize();

            IBehaviorFactory factory = evaluator.BuildBehaviorFactory(
                typeof(ForBehaviorWithOnlySelfCreatedDependenciesBehavior));

            Assert.IsNotNull(factory);
            Assert.AreEqual(Array.Empty<Type>(), factory.RequiredDependencyTypes);
        }

        [Test]
        public void NonBehavior_ThrowsException()
        {
            var evaluator = GetEvaluator<ForBehaviorWithNoDependenciesBehaviorAttribute>();
            evaluator.Initialize();

            Assert.Throws<ArgumentException>(() =>
                evaluator.BuildBehaviorFactory(typeof(NonBehavior)));
        }

        [Test]
        public void Uninitialized_ThrowsException()
        {
            var evaluator = GetEvaluator<ForBehaviorWithNoDependenciesBehaviorAttribute>();

            Assert.Throws<InvalidOperationException>(() =>
                evaluator.BuildBehaviorFactory(typeof(ForBehaviorWithNoDependenciesBehavior)));
        }
    }

    public class GetBehaviorRequiredDependencies
    {
        public static Evaluator<TCAttribute, TMAttribute, TMSAttribute, T, 
            TDAttribute, TOAttribute> GetEvaluator<T>()
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

        public class ExcludesDuplicateExistingDependenciesBehaviorAttribute :
            BaseBehaviorAttribute
        { }
        [ExcludesDuplicateExistingDependenciesBehavior]
        [TD<TestContextA>(Binding.Unique, Fulfillment.Existing, Dep1Name)]
        [TD<TestContextA>(Binding.Unique, Fulfillment.Existing, Dep2Name)]
        public class ExcludesDuplicateExistingDependenciesBehavior
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
        public void NonBehavior_ThrowsException()
        {
            var evaluator = GetEvaluator<HasExistingDependenciesBehaviorAttribute>();
            evaluator.Initialize();

            Assert.Throws<ArgumentException>(() =>
                evaluator.GetBehaviorRequiredDependencies(typeof(NonBehavior)));
        }

        [Test]
        public void Uninitalized_ThrowsException()
        {
            var evaluator = GetEvaluator<HasExistingDependenciesBehaviorAttribute>();

            Assert.Throws<InvalidOperationException>(() =>
                evaluator.GetBehaviorRequiredDependencies(
                    typeof(HasExistingDependenciesBehavior)));
        }

        [Test]
        public void WithDuplicateDependencies_ExcludesDuplicateExistingDependencies()
        {
            var evaluator = GetEvaluator<ExcludesDuplicateExistingDependenciesBehaviorAttribute>();
            evaluator.Initialize();

            Type[] dependencies = evaluator.GetBehaviorRequiredDependencies(
                typeof(ExcludesDuplicateExistingDependenciesBehavior));

            Assert.AreEqual(1, dependencies.Length);
            Assert.AreEqual(typeof(TestContextA), dependencies[0]);
        }

        [Test]
        public void WithExistingDependencies_ProvidesExistingDependencies()
        {
            var evaluator = GetEvaluator<HasExistingDependenciesBehaviorAttribute>();
            evaluator.Initialize();

            Type[] dependencies = evaluator.GetBehaviorRequiredDependencies(
                typeof(HasExistingDependenciesBehavior));

            Assert.AreEqual(2, dependencies.Length);
            Assert.Contains(typeof(TestContextA), dependencies);
            Assert.Contains(typeof(TestContextB), dependencies);
        }

        [Test]
        public void WithNoExistingDependencies_ProvidesNoExistingDependencies()
        {
            var evaluator = GetEvaluator<HasNoExistingDependenciesBehaviorAttribute>();
            evaluator.Initialize();

            Type[] dependencies = evaluator.GetBehaviorRequiredDependencies(
                typeof(HasNoExistingDependenciesBehavior));
            Assert.IsEmpty(dependencies);
        }

        [Test]
        public void WithSelfCreatedDependencies_ExcludesSelfCreatedDependencies()
        {
            var evaluator = GetEvaluator<ExcludesSelfCreatedDependenciesBehaviorAttribute>();
            evaluator.Initialize();

            Type[] dependencies = evaluator.GetBehaviorRequiredDependencies(
                typeof(ExcludesSelfCreatedDependenciesBehavior));

            Assert.AreEqual(1, dependencies.Length);
            Assert.Contains(typeof(TestContextA), dependencies);
        }
    }

    public class GetBehaviorTypes
    {
        public static Evaluator<TCAttribute, TMAttribute, TMSAttribute, T, 
            TDAttribute, TOAttribute> GetEvaluator<T>()
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
        public void HasBehaviorTypes_ProvidesBehaviorTypes()
        {
            var evaluator = GetEvaluator<HasBehaviorTypesBehaviorAttribute>();
            evaluator.Initialize();

            Type[] behaviors = evaluator.GetBehaviorTypes();

            Assert.AreEqual(2, behaviors.Length);
            Assert.Contains(typeof(HasBehaviorTypesBehaviorA), behaviors);
            Assert.Contains(typeof(HasBehaviorTypesBehaviorB), behaviors);
        }

        [Test]
        public void HasNoBehaviorTypes_ProvidesNoBehaviorTypes()
        {
            var evaluator = GetEvaluator<HasNoBehaviorTypesBehaviorAttribute>();
            evaluator.Initialize();

            Assert.IsEmpty(evaluator.GetBehaviorTypes());
        }

        [Test]
        public void Uninitialized_ThrowsException()
        {
            var evaluator = GetEvaluator<HasBehaviorTypesBehaviorAttribute>();

            Assert.Throws<InvalidOperationException>(() =>
                evaluator.GetBehaviorTypes());
        }
    }

    public class GetBindableStateInfos
    {
        public static Evaluator<T, TMAttribute, TMSAttribute, TBAttribute, 
            TDAttribute, TOAttribute> GetEvaluator<T>()
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
        public void HasNoStates_ProvidesNoStates()
        {
            var evaluator = GetEvaluator<HasNoStatesContextAttribute>();
            evaluator.Initialize();

            PropertyInfo[] infos = evaluator.GetBindableStateInfos(typeof(HasNoStatesContext));

            Assert.IsEmpty(infos);
        }

        [Test]
        public void HasStates_ProvidesStates()
        {
            var evaluator = GetEvaluator<HasStatesContextAttribute>();
            evaluator.Initialize();

            PropertyInfo[] infos = evaluator.GetBindableStateInfos(typeof(HasStatesContext));

            Assert.AreEqual(1, infos.Length);
            Assert.AreEqual(nameof(HasStatesContext.Int), infos[0].Name);
        }

        [Test]
        public void NonContext_ThrowsException()
        {
            var evaluator = GetEvaluator<HasStatesContextAttribute>();
            evaluator.Initialize();

            Assert.Throws<ArgumentException>(() =>
                evaluator.GetBindableStateInfos(typeof(NonContext)));
        }

        [Test]
        public void Uninitialized_ThrowsException()
        {
            var evaluator = GetEvaluator<HasStatesContextAttribute>();

            Assert.Throws<InvalidOperationException>(() =>
                evaluator.GetBindableStateInfos(typeof(HasStatesContext)));
        }
    }

    public class BuildMutualismFulfiller
    {
        public static Evaluator<T, TMAttribute, TMSAttribute, TBAttribute, 
            TDAttribute, TOAttribute> GetEvaluator<T>()
            where T : BaseContextAttribute => new();

        public class HasNoMutualistsContextAttribute : BaseContextAttribute { }
        [HasNoMutualistsContext]
        public class HasNoMutualistsContext { }

        public class HasMutualistsContextAttribute : BaseContextAttribute { }
        [HasMutualistsContext]
        [TM<MutualistContext>(MutualistA)]
        [TM<MutualistContext>(MutualistB)]
        public class HasMutualistsContext
        {
            public const string MutualistA = "MutualistA";
            public const string MutualistB = "MutualistB";
        }

        [HasMutualistsContext]
        public class MutualistContext { }

        public class NonContext { }

        public class TBAttribute : BaseBehaviorAttribute { }
        public class TOAttribute : BaseOperationAttribute { }


        [Test]
        public void NonContext_ThrowsException()
        {
            var evaluator = GetEvaluator<HasNoMutualistsContextAttribute>();
            evaluator.Initialize();

            Assert.Throws<ArgumentException>(() =>
                evaluator.BuildMutualismFulfiller(typeof(NonContext)));
        }

        [Test]
        public void Uninitalized_ThrowsException()
        {
            var evaluator = GetEvaluator<HasNoMutualistsContextAttribute>();

            Assert.Throws<InvalidOperationException>(() =>
                evaluator.BuildMutualismFulfiller(typeof(HasNoMutualistsContext)));
        }

        [Test]
        public void WithMutualism_ProvidesMatchingFulfiller()
        {
            var evaluator = GetEvaluator<HasMutualistsContextAttribute>();
            evaluator.Initialize();

            IMutualismFulfiller? fulfiller = evaluator.BuildMutualismFulfiller(
                typeof(HasMutualistsContext));

            Assert.IsNotNull(fulfiller);
            Assert.AreEqual(new Type[]
            {
                typeof(MutualistContext),
                typeof(MutualistContext)
            }, fulfiller?.MutualistContextTypes);
        }

        [Test]
        public void WithNoMutualism_ProvidesNull()
        {
            var evaluator = GetEvaluator<HasNoMutualistsContextAttribute>();
            evaluator.Initialize();

            IMutualismFulfiller? fulfiller = evaluator.BuildMutualismFulfiller(
                typeof(HasNoMutualistsContext));

            Assert.IsNull(fulfiller);
        }
    }

    public class GetContextTypes
    {
        public static Evaluator<T, TMAttribute, TMSAttribute, 
            TBAttribute, TDAttribute, TOAttribute> GetEvaluator<T>()
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
        public void HasContextTypes_ProvidesContextTypes()
        {
            var evaluator = GetEvaluator<HasContextTypesContextAttribute>();
            evaluator.Initialize();

            Type[] contexts = evaluator.GetContextTypes();

            Assert.AreEqual(2, contexts.Length);
            Assert.Contains(typeof(HasContextTypesContextA), contexts);
            Assert.Contains(typeof(HasContextTypesContextB), contexts);
        }

        [Test]
        public void HasNoContextTypes_ProvidesNoContextTypes()
        {
            var evaluator = GetEvaluator<HasNoContextTypesContextAttribute>();
            evaluator.Initialize();

            Assert.IsEmpty(evaluator.GetContextTypes());
        }

        [Test]
        public void Uninitialized_ThrowsException()
        {
            var evaluator = GetEvaluator<HasContextTypesContextAttribute>();

            Assert.Throws<InvalidOperationException>(() =>
                evaluator.GetContextTypes());
        }
    }

    public class GetMutualStateInfos
    {
        public static Evaluator<T, TMAttribute, TMSAttribute, TBAttribute,
            TDAttribute, TOAttribute> GetEvaluator<T>()
            where T : BaseContextAttribute => new();

        public class HasMutualStatesContextAttribute : BaseContextAttribute { }
        [HasMutualStatesContext]
        [TM<MutualistA>(Mutualist)]
        public class HostA
        {
            public const string Mutualist = "Mutualist";

            [TMS(Mutualist, nameof(MutualistA.ValueA))]
            public ContextState<int> IntA { get; init; } = 0;

            [TMS(Mutualist, nameof(MutualistA.ValueB))]
            public ContextState<int> IntB { get; init; } = 0;

            public ContextState<int> IntC { get; init; } = 0;
        }
        public class HasMultipleMutualStatesPerPropertyContextAttribute : BaseContextAttribute { }
        [HasMultipleMutualStatesPerPropertyContext]
        [TM<MutualistA>(Mutualist)]
        public class HostB
        {
            public const string Mutualist = "Mutualist";

            [TMS(Mutualist, nameof(MutualistA.ValueA))]
            [TMS(Mutualist, nameof(MutualistA.ValueB))]
            public ContextState<int> IntA { get; init; } = 0;
        }
        public class HasDuplicateMutualStatesContextAttribute : BaseContextAttribute { }
        [HasDuplicateMutualStatesContext]
        [TM<MutualistA>(Mutualist)]
        public class HostC
        {
            public const string Mutualist = "Mutualist";

            [TMS(Mutualist, nameof(MutualistA.ValueA))]
            [TMS(Mutualist, nameof(MutualistA.ValueA))]
            public ContextState<int> IntA { get; init; } = 0;
        }

        [HasMutualStatesContext]
        [HasDuplicateMutualStatesContext]
        [HasMultipleMutualStatesPerPropertyContext]
        public class MutualistA
        {
            public ContextState<int> ValueA { get; init; } = 0;

            public ContextState<int> ValueB { get; init; } = 0;
        }

        public class HasNoMutualStatesContextAttribute : BaseContextAttribute { }
        [HasNoMutualStatesContext]
        public class HasNoMutualStatesContext
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
        public void HasDuplicateMutualStates_ProvidesNonDuplicateStates()
        {
            var evaluator = GetEvaluator<HasDuplicateMutualStatesContextAttribute>();
            evaluator.Initialize();

            MutualStateInfo[] infos = evaluator.GetMutualStateInfos(typeof(HostC));

            var hostIntA = typeof(HostC).GetProperty(nameof(HostC.IntA)) ??
                throw new NullReferenceException();
            var mutualistValueA = typeof(MutualistA).GetProperty(nameof(MutualistA.ValueA)) ??
                throw new NullReferenceException();

            Assert.AreEqual(1, infos.Length);
            Assert.Contains(new MutualStateInfo(
                hostIntA, HostC.Mutualist, mutualistValueA), infos);
        }

        [Test]
        public void HasMultipleMutualStatesPerProperty_ProvidesMultipleStatesPerProperty()
        {
            var evaluator = GetEvaluator<HasMultipleMutualStatesPerPropertyContextAttribute>();
            evaluator.Initialize();

            MutualStateInfo[] infos = evaluator.GetMutualStateInfos(typeof(HostB));

            var hostIntA = typeof(HostB).GetProperty(nameof(HostB.IntA)) ??
                throw new NullReferenceException();
            var mutualistValueA = typeof(MutualistA).GetProperty(nameof(MutualistA.ValueA)) ??
                throw new NullReferenceException();
            var mutualistValueB = typeof(MutualistA).GetProperty(nameof(MutualistA.ValueB)) ??
                throw new NullReferenceException();

            Assert.AreEqual(2, infos.Length);
            Assert.Contains(new MutualStateInfo(
                hostIntA, HostB.Mutualist, mutualistValueA), infos);
            Assert.Contains(new MutualStateInfo(
                hostIntA, HostB.Mutualist, mutualistValueB), infos);
        }

        [Test]
        public void HasMutualStates_ProvidesStates()
        {
            var evaluator = GetEvaluator<HasMutualStatesContextAttribute>();
            evaluator.Initialize();

            MutualStateInfo[] infos = evaluator.GetMutualStateInfos(typeof(HostA));

            var hostIntA = typeof(HostA).GetProperty(nameof(HostA.IntA)) ?? 
                throw new NullReferenceException();
            var hostIntB = typeof(HostA).GetProperty(nameof(HostA.IntB)) ??
                throw new NullReferenceException();
            var mutualistValueA = typeof(MutualistA).GetProperty(nameof(MutualistA.ValueA)) ??
                throw new NullReferenceException();
            var mutualistValueB = typeof(MutualistA).GetProperty(nameof(MutualistA.ValueB)) ??
                throw new NullReferenceException();

            Assert.AreEqual(2, infos.Length);
            Assert.Contains(new MutualStateInfo(
                hostIntA, HostA.Mutualist, mutualistValueA), infos);
            Assert.Contains(new MutualStateInfo(
                hostIntB, HostA.Mutualist, mutualistValueB), infos);
        }

        [Test]
        public void HasNoMutualStates_ProvidesStates()
        {
            var evaluator = GetEvaluator<HasNoMutualStatesContextAttribute>();
            evaluator.Initialize();

            MutualStateInfo[] infos = evaluator.GetMutualStateInfos(
                typeof(HasNoMutualStatesContext));

            Assert.IsEmpty(infos);
        }

        [Test]
        public void HasNoStates_ProvidesNoStates()
        {
            var evaluator = GetEvaluator<HasNoStatesContextAttribute>();
            evaluator.Initialize();

            MutualStateInfo[] infos = evaluator.GetMutualStateInfos(typeof(HasNoStatesContext));

            Assert.IsEmpty(infos);
        }

        [Test]
        public void NonContext_ThrowsException()
        {
            var evaluator = GetEvaluator<HasNoMutualStatesContextAttribute>();
            evaluator.Initialize();

            Assert.Throws<ArgumentException>(() => 
            evaluator.GetMutualStateInfos(typeof(NonContext)));
        }

        [Test]
        public void Uninitialized_ThrowsException()
        {
            var evaluator = GetEvaluator<HasNoMutualStatesContextAttribute>();

            Assert.Throws<InvalidOperationException>(() =>
                evaluator.GetMutualStateInfos(typeof(HasNoMutualStatesContext)));
        }
    }

    public class GetOnChangeOperations
    {
        public static Evaluator<TCAttribute, TMAttribute, TMSAttribute, 
            T, TDAttribute, TOAttribute> GetEvaluator<T>()
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
        [TD<TestContextB>(Binding.Unique, Fulfillment.Existing, ContextB)]
        public class HasOperationsBehavior
        {
            public const string ContextA = "contextA";
            public const string ContextB = "contextB";

            public HasOperationsBehavior(out TestContextA contextA) => contextA = new();

            [TO]
            [OnChange(ContextA)]
            public void OnContextAChange(TestContextA contextA) { }

            [TO]
            [OnChange(ContextA, nameof(TestContextA.Int))]
            private void OnContextAIntChange(TestContextA contextA) { }

            [TO]
            [OnChange(ContextB)]
            public void OnContextBChange(TestContextB contextB) { }

            [TO]
            [OnChange(ContextB, nameof(TestContextB.Int))]
            private void OnContextBIntChange(TestContextB contextB) { }
        }

        public class TCAttribute : BaseContextAttribute { }
        [TC]
        public class TestContextA
        {
            public ContextState<int> Int { get; init; } = 0;
        }

        [TC]
        public class TestContextB
        {
            public ContextState<int> Int { get; init; } = 0;
        }

        public class TOAttribute : BaseOperationAttribute { }

        [Test]
        public void InvalidContext_ProvidesNoOperations()
        {
            var evaluator = GetEvaluator<HasOperationsBehaviorAttribute>();
            evaluator.Initialize();

            MethodInfo[] operations = evaluator.GetOnChangeOperations(
                typeof(HasOperationsBehavior), "invalidContextName");

            Assert.IsEmpty(operations);
        }

        [Test]
        public void InvalidState_ProvidesNoOperations()
        {
            var evaluator = GetEvaluator<HasOperationsBehaviorAttribute>();
            evaluator.Initialize();

            MethodInfo[] operations = evaluator.GetOnChangeOperations(
                typeof(HasOperationsBehavior), HasOperationsBehavior.ContextA, "invalidState");

            Assert.IsEmpty(operations);
        }

        [Test]
        public void NonBehavior_ThrowsException()
        {
            var evaluator = GetEvaluator<HasOperationsBehaviorAttribute>();
            evaluator.Initialize();

            Assert.Throws<ArgumentException>(() =>
                evaluator.GetOnChangeOperations(typeof(NonBehavior), "contextName"));
        }

        [Test]
        public void NullContext_ThrowsException()
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
        public void Uninitialized_ThrowsException()
        {
            var evaluator = GetEvaluator<HasOperationsBehaviorAttribute>();

            Assert.Throws<InvalidOperationException>(() =>
                evaluator.GetOnChangeOperations(typeof(HasOperationsBehavior),
                    HasOperationsBehavior.ContextA));
        }
    }

    public class GetOnTeardownOperations
    {
        public static Evaluator<TCAttribute, TMAttribute, TMSAttribute, T, 
            TDAttribute, TOAttribute> GetEvaluator<T>()
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
            [OnTeardown]
            public void OnTeardownA(TestContextA contextA) { }

            [TO]
            [OnTeardown]
            public void OnTeardownB() { }
        }

        public class TCAttribute : BaseContextAttribute { }
        [TC]
        public class TestContextA { }

        public class TOAttribute : BaseOperationAttribute { }


        [Test]
        public void NonBehavior_ThrowsException()
        {
            var evaluator = GetEvaluator<HasOperationsBehaviorAttribute>();
            evaluator.Initialize();

            Assert.Throws<ArgumentException>(() =>
                evaluator.GetOnTeardownOperations(typeof(NonBehavior)));
        }

        [Test]
        public void ProvidesOnTeardownOperations_HasNoOperations()
        {
            var evaluator = GetEvaluator<HasNoOperationsBehaviorAttribute>();
            evaluator.Initialize();

            MethodInfo[] operations = evaluator.GetOnTeardownOperations(
                typeof(HasNoOperationsBehavior));

            Assert.IsEmpty(operations);
        }

        [Test]
        public void ProvidesOnTeardownOperations_HasOperations()
        {
            var evaluator = GetEvaluator<HasOperationsBehaviorAttribute>();
            evaluator.Initialize();

            MethodInfo?[] expectedOperations = new MethodInfo?[]
            {
                    typeof(HasOperationsBehavior).GetMethod(
                        nameof(HasOperationsBehavior.OnTeardownA)),
                    typeof(HasOperationsBehavior).GetMethod(
                        nameof(HasOperationsBehavior.OnTeardownB))
            };

            MethodInfo[] operations = evaluator.GetOnTeardownOperations(
                typeof(HasOperationsBehavior));

            Assert.AreEqual(expectedOperations.Length, operations.Length);
            Assert.Contains(expectedOperations[0], operations);
            Assert.Contains(expectedOperations[1], operations);
        }

        [Test]
        public void Uninitialized_ThrowsException()
        {
            var evaluator = GetEvaluator<HasOperationsBehaviorAttribute>();

            Assert.Throws<InvalidOperationException>(() =>
                evaluator.GetOnTeardownOperations(typeof(HasOperationsBehavior)));
        }
    }

    public class GetOnUpdateOperations
    {
        public static Evaluator<TCAttribute, TMAttribute, TMSAttribute, T,
            TDAttribute, TOAttribute> GetEvaluator<T>()
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
            [OnUpdate]
            public void OnUpdateA(TestContextA contextA) { }

            [TO]
            [OnUpdate]
            public void OnUpdateB() { }
        }

        public class TCAttribute : BaseContextAttribute { }
        [TC]
        public class TestContextA { }

        public class TOAttribute : BaseOperationAttribute { }


        [Test]
        public void NonBehavior_ThrowsException()
        {
            var evaluator = GetEvaluator<HasOperationsBehaviorAttribute>();
            evaluator.Initialize();

            Assert.Throws<ArgumentException>(() =>
                evaluator.GetOnTeardownOperations(typeof(NonBehavior)));
        }

        [Test]
        public void ProvidesOnUpdateOperations_HasNoOperations()
        {
            var evaluator = GetEvaluator<HasNoOperationsBehaviorAttribute>();
            evaluator.Initialize();

            MethodInfo[] operations = evaluator.GetOnUpdateOperations(
                typeof(HasNoOperationsBehavior));

            Assert.IsEmpty(operations);
        }

        [Test]
        public void ProvidesOnUpdateOperations_HasOperations()
        {
            var evaluator = GetEvaluator<HasOperationsBehaviorAttribute>();
            evaluator.Initialize();

            MethodInfo?[] expectedOperations = new MethodInfo?[]
            {
                    typeof(HasOperationsBehavior).GetMethod(
                        nameof(HasOperationsBehavior.OnUpdateA)),
                    typeof(HasOperationsBehavior).GetMethod(
                        nameof(HasOperationsBehavior.OnUpdateB))
            };

            MethodInfo[] operations = evaluator.GetOnUpdateOperations(
                typeof(HasOperationsBehavior));

            Assert.AreEqual(expectedOperations.Length, operations.Length);
            Assert.Contains(expectedOperations[0], operations);
            Assert.Contains(expectedOperations[1], operations);
        }

        [Test]
        public void Uninitialized_ThrowsException()
        {
            var evaluator = GetEvaluator<HasOperationsBehaviorAttribute>();

            Assert.Throws<InvalidOperationException>(() =>
                evaluator.GetOnUpdateOperations(typeof(HasOperationsBehavior)));
        }
    }

    public class Initialization
    {
        public static Evaluator<TCAttribute, TMAttribute, TMSAttribute, T1, 
            TDAttribute, T2> GetEvaluator<T1, T2>()
            where T1 : BaseBehaviorAttribute 
            where T2 : BaseOperationAttribute => new();

        public class TBAttribute : BaseBehaviorAttribute { }

        public class InvalidDependencyConstructorBehaviorExAttribute :
            BaseBehaviorAttribute
        { }
        [InvalidDependencyConstructorBehaviorEx]
        [TDAttribute<TestContextA>(Binding.Unique, Fulfillment.SelfCreated, "contextA")]
        [TDAttribute<TestContextB>(Binding.Unique, Fulfillment.Existing, "contextB")]
        public class InvalidDependencyConstructorBehaviorEx
        {
            protected InvalidDependencyConstructorBehaviorEx(
                out TestContextA contextA, out TestContextB contextB)
            {
                contextA = new();
                contextB = new();
            }
        }

        public class InvalidDependencyConstructorBehaviorScAttribute :
            BaseBehaviorAttribute
        { }
        [InvalidDependencyConstructorBehaviorSc]
        [TDAttribute<TestContextA>(Binding.Unique, Fulfillment.SelfCreated, "contextA")]
        public class InvalidDependencyConstructorBehaviorSc
        {
            public InvalidDependencyConstructorBehaviorSc(TestContextA contextA) =>
                contextA = new();
        }

        public class InvalidDuplicateDependencyNamesBehaviorAttribute : BaseBehaviorAttribute { }
        [InvalidDuplicateDependencyNamesBehavior]
        [TDAttribute<TestContextA>(Binding.Unique, Fulfillment.SelfCreated, "context")]
        [TDAttribute<TestContextB>(Binding.Unique, Fulfillment.Existing, "context")]
        public class InvalidDuplicateDependencyNamesBehavior
        {
            protected InvalidDuplicateDependencyNamesBehavior(out TestContextA context) =>
                context = new();
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

        public class InvalidParamCountConstructorBehaviorExAttribute : BaseBehaviorAttribute { }
        [InvalidParamCountConstructorBehaviorEx]
        [TDAttribute<TestContextA>(Binding.Unique, Fulfillment.Existing, "contextA")]
        public class InvalidParamCountConstructorBehaviorEx
        {
            protected InvalidParamCountConstructorBehaviorEx(
                TestContextA contextA, TestContextB contextB)
            { }
        }

        public class InvalidParamCountConstructorBehaviorScAttribute : BaseBehaviorAttribute { }
        [InvalidParamCountConstructorBehaviorSc]
        [TDAttribute<TestContextA>(Binding.Unique, Fulfillment.SelfCreated, "contextA")]
        public class InvalidParamCountConstructorBehaviorSc
        {
            protected InvalidParamCountConstructorBehaviorSc(
                out TestContextA contextA, out TestContextB contextB)
            {
                contextA = new();
                contextB = new();
            }
        }

        public class InvalidParamNameConstructorBehaviorExAttribute : BaseBehaviorAttribute { }
        [InvalidParamNameConstructorBehaviorEx]
        [TDAttribute<TestContextA>(Binding.Unique, Fulfillment.Existing, "contextA")]
        public class InvalidParamNameConstructorBehaviorEx
        {
            protected InvalidParamNameConstructorBehaviorEx(TestContextA a) { }
        }

        public class InvalidParamNameConstructorBehaviorScAttribute : BaseBehaviorAttribute { }
        [InvalidParamNameConstructorBehaviorSc]
        [TDAttribute<TestContextA>(Binding.Unique, Fulfillment.SelfCreated, "contextA")]
        public class InvalidParamNameConstructorBehaviorSc
        {
            protected InvalidParamNameConstructorBehaviorSc(out TestContextA a) => a = new();
        }

        public class InvalidParamTypeConstructorBehaviorExAttribute : BaseBehaviorAttribute { }
        [InvalidParamTypeConstructorBehaviorEx]
        [TDAttribute<TestContextA>(Binding.Unique, Fulfillment.Existing, "contextA")]
        public class InvalidParamTypeConstructorBehaviorEx
        {
            protected InvalidParamTypeConstructorBehaviorEx(TestContextB contextA) { }
        }

        public class InvalidParamTypeConstructorBehaviorScAttribute : BaseBehaviorAttribute { }
        [InvalidParamTypeConstructorBehaviorSc]
        [TDAttribute<TestContextA>(Binding.Unique, Fulfillment.SelfCreated, "contextA")]
        public class InvalidParamTypeConstructorBehaviorSc
        {
            protected InvalidParamTypeConstructorBehaviorSc(out TestContextB contextA) =>
                contextA = new();
        }

        public class MissingConstructorBehaviorAttribute : BaseBehaviorAttribute { }
        [MissingConstructorBehavior]
        [TDAttribute<TestContextA>(Binding.Unique, Fulfillment.SelfCreated, "contextA")]
        public class MissingConstructorBehavior { }

        public class NonContextDependencyBehaviorExAttribute : BaseBehaviorAttribute { }
        [NonContextDependencyBehaviorEx]
        [TD<NonContext>(Binding.Unique, Fulfillment.Existing, "nonContext")]
        public class NonContextDependencyBehaviorEx
        {
            public NonContextDependencyBehaviorEx(NonContext nonContext) { }
        }

        public class NonContextDependencyBehaviorScAttribute : BaseBehaviorAttribute { }
        [NonContextDependencyBehaviorSc]
        [TD<NonContext>(Binding.Unique, Fulfillment.SelfCreated, "nonContext")]
        public class NonContextDependencyBehaviorSc
        {
            public NonContextDependencyBehaviorSc(out NonContext nonContext) =>
                nonContext = new();
        }

        public class ValidExistingDependencyConstructorBehaviorAttribute :
            BaseBehaviorAttribute
        { }
        [ValidExistingDependencyConstructorBehavior]
        [TDAttribute<TestContextA>(Binding.Unique, Fulfillment.Existing, "contextA")]
        [TDAttribute<TestContextB>(Binding.Unique, Fulfillment.Existing, "contextB")]
        public class ValidExistingDependencyConstructorBehavior
        {
            protected ValidExistingDependencyConstructorBehavior(
                TestContextA contextA, TestContextB contextB)
            {
            }
        }

        public class ValidMixedDependencyConstructorBehaviorAttribute : BaseBehaviorAttribute { }
        [ValidMixedDependencyConstructorBehavior]
        [TDAttribute<TestContextA>(Binding.Unique, Fulfillment.SelfCreated, "contextA")]
        [TDAttribute<TestContextB>(Binding.Unique, Fulfillment.Existing, "contextB")]
        public class ValidMixedDependencyConstructorBehavior
        {
            protected ValidMixedDependencyConstructorBehavior(
                out TestContextA contextA, TestContextB contextB)
            {
                contextA = new();
            }
        }

        public class ValidSelfCreatedDependencyConstructorBehaviorAttribute :
            BaseBehaviorAttribute
        { }
        [ValidSelfCreatedDependencyConstructorBehavior]
        [TDAttribute<TestContextA>(Binding.Unique, Fulfillment.SelfCreated, "contextA")]
        [TDAttribute<TestContextB>(Binding.Unique, Fulfillment.SelfCreated, "contextB")]
        public class ValidSelfCreatedDependencyConstructorBehavior
        {
            protected ValidSelfCreatedDependencyConstructorBehavior(
                out TestContextA contextA, out TestContextB contextB)
            {
                contextA = new();
                contextB = new();
            }
        }

        public class TCAttribute : BaseContextAttribute { }
        [TC]
        public class TestContextA
        {
            public ContextState<int> Int { get; init; } = 0;
        }
        [TC]
        public class TestContextB { }

        public class InvalidDuplicateMutualistNamesContextAttribute : BaseContextAttribute { }
        [InvalidDuplicateMutualistNamesContext]
        [TM<MutualistA>("DuplicateName")]
        [TM<MutualistA>("DuplicateName")]
        public class InvalidDuplicateMutualistNamesContext { }

        public class InvalidMutualStateMutualistNameContextAttribute : BaseContextAttribute { }
        [InvalidMutualStateMutualistNameContext]
        [TM<MutualistA>(Mutualist)]
        public class InvalidMutualStateMutualistNameContext
        {
            public const string Mutualist = "Mutualist";

            [TMS("InvalidName", nameof(MutualistA.ValueA))]
            public ContextState<int> IntA { get; init; } = 0;
        }

        public class InvalidMutualStateStateNameContextAttribute : BaseContextAttribute { }
        [InvalidMutualStateStateNameContext]
        [TM<MutualistA>(Mutualist)]
        public class InvalidMutualStateStateNameContext
        {
            public const string Mutualist = "Mutualist";

            [TMS(Mutualist, "InvalidName")]
            public ContextState<int> IntA { get; init; } = 0;
        }
        public class InvalidMutualStateTypeContextAttribute : BaseContextAttribute { }
        [InvalidMutualStateTypeContext]
        [TM<MutualistA>(Mutualist)]
        public class InvalidMutualStateTypeContext
        {
            public const string Mutualist = "Mutualist";

            [TMS(Mutualist, nameof(MutualistA.ValueA))]
            public ContextState<bool> BoolA { get; init; } = false;
        }

        public class NonContextMutualistContextAttribute : BaseContextAttribute { }
        [NonContextMutualistContext]
        [TM<NonContext>("NonContextMutualistName")]
        public class NonContextMutualistContext { }

        public class NonHostWithMutualStateContextAttribute : BaseContextAttribute { }
        [NonHostWithMutualStateContext]
        public class NonHostWithMutualStateContext
        {
            [TMS("Mutualist", nameof(MutualistA.ValueA))]
            public ContextState<int> IntA { get; init; } = 0;
        }

        [InvalidDuplicateMutualistNamesContext]
        [InvalidMutualStateMutualistNameContext]
        [InvalidMutualStateStateNameContext]
        [InvalidMutualStateTypeContext]
        public class MutualistA
        {
            public ContextState<int> ValueA { get; init; } = 0;
        }

        public class NonContext { }


        [Test]
        public void InvalidDependencyConstructorEx_ThrowsException()
        {
            var evaluator = GetEvaluator<InvalidDependencyConstructorBehaviorExAttribute,
                TOAttribute>();

            Assert.Throws<InvalidOperationException>(() =>
                evaluator.Initialize());
        }

        [Test]
        public void InvalidDependencyConstructorSc_ThrowsException()
        {
            var evaluator = new Evaluator<TCAttribute, TMAttribute, TMSAttribute,
                InvalidDependencyConstructorBehaviorScAttribute, TDAttribute, TOAttribute>();

            Assert.Throws<InvalidOperationException>(() =>
                evaluator.Initialize());
        }

        [Test]
        public void InvalidDuplicateDependencyNames_ThrowsException()
        {
            var evaluator = GetEvaluator<InvalidDuplicateDependencyNamesBehaviorAttribute,
                TOAttribute>();

            Assert.Throws<InvalidOperationException>(() =>
                evaluator.Initialize());
        }

        [Test]
        public void InvalidDuplicateMutualistNames_ThrowsException()
        {
            var evaluator = new Evaluator<InvalidDuplicateMutualistNamesContextAttribute, 
                TMAttribute, TMSAttribute, TBAttribute, TDAttribute, TOAttribute>();

              Assert.Throws<InvalidOperationException>(() =>
                evaluator.Initialize());
        }

        [Test]
        public void InvalidMutualStateMutualistName_ThrowsException()
        {
            var evaluator = new Evaluator<InvalidMutualStateMutualistNameContextAttribute,
                TMAttribute, TMSAttribute, TBAttribute, TDAttribute, TOAttribute>();

            Assert.Throws<InvalidOperationException>(() =>
              evaluator.Initialize());
        }

        [Test]
        public void InvalidMutualStateStateName_ThrowsException()
        {
            var evaluator = new Evaluator<InvalidMutualStateStateNameContextAttribute,
                TMAttribute, TMSAttribute, TBAttribute, TDAttribute, TOAttribute>();

            Assert.Throws<InvalidOperationException>(() =>
              evaluator.Initialize());
        }

        [Test]
        public void InvalidMutualStateType_ThrowsException()
        {
            var evaluator = new Evaluator<InvalidMutualStateTypeContextAttribute,
                TMAttribute, TMSAttribute, TBAttribute, TDAttribute, TOAttribute>();

            Assert.Throws<InvalidOperationException>(() =>
              evaluator.Initialize());
        }

        [Test]
        public void InvalidOnChangeContext_ThrowsException()
        {
            var evaluator = GetEvaluator<InvalidOperationsBehaviorAttribute,
                InvalidOnChangeContextOperationAttribute>();

            Assert.Throws<InvalidOperationException>(() =>
                evaluator.Initialize());
        }

        [Test]
        public void InvalidOnChangeState_ThrowsException()
        {
            var evaluator = GetEvaluator<InvalidOperationsBehaviorAttribute,
                InvalidOnChangeStateOperationAttribute>();

            Assert.Throws<InvalidOperationException>(() =>
                evaluator.Initialize());
        }

        [Test]
        public void InvalidOperationContextName_ThrowsException()
        {
            var evaluator = GetEvaluator<InvalidOperationsBehaviorAttribute,
                InvalidOperationContextNameOperationAttribute>();

            Assert.Throws<InvalidOperationException>(() =>
                evaluator.Initialize());
        }

        [Test]
        public void InvalidOperationContextType_ThrowsException()
        {
            var evaluator = GetEvaluator<InvalidOperationsBehaviorAttribute,
                InvalidOperationContextTypeOperationAttribute>();

            Assert.Throws<InvalidOperationException>(() =>
                evaluator.Initialize());
        }

        [Test]
        public void InvalidParameterCountConstructorEx_ThrowsException()
        {
            var evaluator = GetEvaluator<InvalidParamCountConstructorBehaviorExAttribute,
                TOAttribute>();

            Assert.Throws<InvalidOperationException>(() =>
                evaluator.Initialize());
        }

        [Test]
        public void InvalidParameterCountConstructorSc_ThrowsException()
        {
            var evaluator = GetEvaluator<InvalidParamCountConstructorBehaviorScAttribute,
                TOAttribute>();

            Assert.Throws<InvalidOperationException>(() =>
                evaluator.Initialize());
        }

        [Test]
        public void InvalidParameterNameConstructorEx_ThrowsException()
        {
            var evaluator = GetEvaluator<InvalidParamNameConstructorBehaviorExAttribute,
                TOAttribute>();

            Assert.Throws<InvalidOperationException>(() =>
                evaluator.Initialize());
        }

        [Test]
        public void InvalidParameterNameConstructorSc_ThrowsException()
        {
            var evaluator = GetEvaluator<InvalidParamNameConstructorBehaviorScAttribute,
                TOAttribute>();

            Assert.Throws<InvalidOperationException>(() =>
                evaluator.Initialize());
        }

        [Test]
        public void InvalidParameterTypeConstructorEx_ThrowsException()
        {
            var evaluator = GetEvaluator<InvalidParamTypeConstructorBehaviorExAttribute,
                TOAttribute>();

            Assert.Throws<InvalidOperationException>(() =>
                evaluator.Initialize());
        }

        [Test]
        public void InvalidParameterTypeConstructorSc_ThrowsException()
        {
            var evaluator = GetEvaluator<InvalidParamTypeConstructorBehaviorScAttribute,
                TOAttribute>();

            Assert.Throws<InvalidOperationException>(() =>
                evaluator.Initialize());
        }

        [Test]
        public void MissingConstructor_ThrowsException()
        {
            var evaluator = GetEvaluator<MissingConstructorBehaviorAttribute, TOAttribute>();

            Assert.Throws<InvalidOperationException>(() =>
                evaluator.Initialize());
        }

        [Test]
        public void NonContextMutualist_ThrowsException()
        {
            var evaluator = new Evaluator<NonContextMutualistContextAttribute,
                TMAttribute, TMSAttribute, TBAttribute, TDAttribute, TOAttribute>();

            Assert.Throws<InvalidOperationException>(() =>
                evaluator.Initialize());
        }

        [Test]
        public void NonContextDependencyEx_ThrowsException()
        {
            var evaluator = GetEvaluator<NonContextDependencyBehaviorExAttribute, TOAttribute>();

            Assert.Throws<InvalidOperationException>(() =>
                evaluator.Initialize());
        }

        [Test]
        public void NonContextDependencySc_ThrowsException()
        {
            var evaluator = GetEvaluator<NonContextDependencyBehaviorScAttribute, TOAttribute>();

            Assert.Throws<InvalidOperationException>(() =>
                evaluator.Initialize());
        }

        [Test]
        public void NonHostWithMutualState_ThrowsException()
        {
            var evaluator = new Evaluator<NonHostWithMutualStateContextAttribute,
                TMAttribute, TMSAttribute, TBAttribute, TDAttribute, TOAttribute>();

            Assert.Throws<InvalidOperationException>(() =>
              evaluator.Initialize());
        }

        [Test]
        public void ValidExistingDependencyConstructor_CompletesInitialization()
        {
            var evaluator = GetEvaluator<ValidExistingDependencyConstructorBehaviorAttribute,
                TOAttribute>();

            evaluator.Initialize();
        }

        [Test]
        public void ValidMixedDependencyConstructor_CompletesInitialization()
        {
            var evaluator = GetEvaluator<ValidMixedDependencyConstructorBehaviorAttribute,
                TOAttribute>();

            evaluator.Initialize();
        }

        [Test]
        public void ValidSelfCreatedDependencyConstructor_CompletesInitialization()
        {
            var evaluator = GetEvaluator<ValidSelfCreatedDependencyConstructorBehaviorAttribute,
                TOAttribute>();

            evaluator.Initialize();
        }
    }

    public class IsContextType
    {
        public static Evaluator<T, TMAttribute, TMSAttribute, TBAttribute, 
            TDAttribute, TOAttribute> GetEvaluator<T>()
            where T : BaseContextAttribute => new();

        public class InvalidContextTypeContextAttribute : BaseContextAttribute { }
        public class NonContext { }

        public class ValidContextTypeContextAttribute : BaseContextAttribute { }
        [ValidContextTypeContext]
        public class ValidContextTypeContext { }


        public class TBAttribute : BaseBehaviorAttribute { }
        public class TOAttribute : BaseOperationAttribute { }
        [Test]
        public void InvalidContextType_ReturnsFalse()
        {
            var evaluator = GetEvaluator<InvalidContextTypeContextAttribute>();
            evaluator.Initialize();

            Assert.IsFalse(evaluator.IsContextType(typeof(NonContext)));
        }

        [Test]
        public void ValidContextType_ReturnsTrue()
        {
            var evaluator = GetEvaluator<ValidContextTypeContextAttribute>();
            evaluator.Initialize();

            Assert.IsTrue(evaluator.IsContextType(typeof(ValidContextTypeContext)));
        }

        [Test]
        public void Uninitialized_ThrowsException()
        {
            var evaluator = GetEvaluator<ValidContextTypeContextAttribute>();

            Assert.Throws<InvalidOperationException>(() =>
                evaluator.IsContextType(typeof(ValidContextTypeContext)));
        }
    }
}
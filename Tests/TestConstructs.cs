global using ContextualProgramming;

namespace Tests.Constructs
{
    #region Test Behavior Attributes
    public class TBAttribute : BaseBehaviorAttribute { }
    public class TBNonOutParameterConstructorAttribute : BaseBehaviorAttribute { }
    public class TBNullDependencyConstructorAttribute : BaseBehaviorAttribute { }
    public class UnusedTBAttribute : BaseBehaviorAttribute { }
    #endregion

    #region Test Context Attributes
    public class TCAttribute : BaseContextAttribute { }
    public class UnusedTCAttribute : BaseContextAttribute { }
    #endregion

    #region Test Dependency Attributes
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

    #region Test Operation Attributes
    public class TOAttribute : BaseOperationAttribute { }
    #endregion

    #region Test Classes
    [TB]
    [TDAttribute<TestContextA>(Binding.Unique, Fulfillment.SelfCreated, ContextAName)]
    [TDAttribute<TestContextB>(Binding.Unique, Fulfillment.SelfCreated, ContextBName)]
    [TDAttribute<TestContextC>(Binding.Unique, Fulfillment.SelfCreated, ContextCName)]
    public class TestBehaviorA
    {
        public const string ContextAName = "contextA";
        public const string ContextBName = "contextB";
        public const string ContextCName = "contextC";

        public static int InstanceCount = 0;

        protected TestBehaviorA(out TestContextA contextA, out TestContextB contextB, 
            out TestContextC contextC)
        {
            contextA = new();
            contextB = new();
            contextC = new();

            InstanceCount++;
        }

        ~TestBehaviorA()
        {
            InstanceCount--;
        }


        [TO]
        [OnChange(ContextAName, nameof(TestContextA.Int))]
        private void OnContextAIntChange(TestContextA contextA, TestContextC contextC)
        {
            contextA.OnStateChangeIntValue = contextA.Int;
            contextC.Int.Value = contextA.Int;
        }

        [TO]
        [OnChange(ContextAName)]
        public void OnContextAChange(TestContextA contextA)
        {
            contextA.OnContextChangeIntValue = contextA.Int;
        }

        [TO]
        [OnChange(ContextBName)]
        public void OnContextBChange() { }

        [TO]
        [OnChange(ContextCName, nameof(TestContextC.Int))]
        public void OnContextCIntChange(TestContextC contextC)
        {
            contextC.OnContextChangeIntValue = contextC.Int;
        }
    }

    [TB]
    public class TestBehaviorB { }


    [TBNonOutParameterConstructor]
    [TDAttribute<TestContextA>(Binding.Unique, Fulfillment.SelfCreated, "contextA")]
    public class TestNonOutParameterConstructorBehavior
    {
        protected TestNonOutParameterConstructorBehavior(TestContextA contextA) { }
    }

    [TBNullDependencyConstructor]
    [TDAttribute<TestContextA>(Binding.Unique, Fulfillment.SelfCreated, "contextA")]
    public class TestNullDependencyConstructorBehavior
    {
        protected TestNullDependencyConstructorBehavior(out TestContextA? contextA) =>
            contextA = null;
    }

    public class TestNonBehavior { }


    [TC]
    public class TestContextA
    {
        public int OnStateChangeIntValue { get; set; } = 0;
        public int OnContextChangeIntValue { get; set; } = 0;

        public ContextState<int> Int { get; init; } = 0;
    }

    [TC]
    public class TestContextB { }

    [TC]
    public class TestContextC
    {
        public int OnContextChangeIntValue { get; set; } = 0;

        public ContextState<int> Int { get; init; } = 0;
        public ContextState<string> String { get; init; } = "Test";
    }

    public class TestNonContext { }
    #endregion
}
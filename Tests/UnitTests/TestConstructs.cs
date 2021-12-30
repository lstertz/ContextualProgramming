namespace Tests.Constructs
{
    #region Test Attributes
    public class TBAttribute : BaseBehaviorAttribute { }
    public class TBNonContextDependencyAttribute : BaseBehaviorAttribute { }
    public class TBInvalidDependencyConstructorAttribute : BaseBehaviorAttribute { }
    public class TBInvalidParameterNameConstructorAttribute : BaseBehaviorAttribute { }
    public class TBInvalidParameterCountConstructorAttribute : BaseBehaviorAttribute { }
    public class TBInvalidParameterTypeConstructorAttribute : BaseBehaviorAttribute { }
    public class TBMissingConstructorAttribute : BaseBehaviorAttribute { }
    public class TBNonOutParameterConstructorAttribute : BaseBehaviorAttribute { }
    public class TBNullDependencyConstructorAttribute : BaseBehaviorAttribute { }
    public class UnusedTBAttribute : BaseBehaviorAttribute { }

    public class TCAttribute : BaseContextAttribute { }
    public class UnusedTCAttribute : BaseContextAttribute { }

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
        private void OnContextAIntChange(TestContextA contextA, TestContextB contextB) { }

        [TO]
        [OnChange(ContextBName)]
        public void OnContextBChange() { }
    }

    [TB]
    public class TestBehaviorB { }


    [TBInvalidParameterCountConstructor]
    [TDAttribute<TestContextA>(Binding.Unique, Fulfillment.SelfCreated, "contextA")]
    public class TestInvalidParameterCountConstructorBehavior
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
    public class TBInvalidParameterNameConstructorBehavior
    {
        protected TBInvalidParameterNameConstructorBehavior(out TestContextA a) => a = new();
    }

    [TBInvalidParameterTypeConstructor]
    [TDAttribute<TestContextA>(Binding.Unique, Fulfillment.SelfCreated, "contextA")]
    public class TestInvalidParameterTypeConstructorBehavior
    {
        protected TestInvalidParameterTypeConstructorBehavior(out TestContextB contextA) =>
            contextA = new();
    }

    [TBMissingConstructor]
    [TDAttribute<TestContextA>(Binding.Unique, Fulfillment.SelfCreated, "contextA")]
    public class TestMissingConstructorBehavior { }

    [TBNonContextDependency]
    [TDAttribute<TestNonContext>(Binding.Unique, Fulfillment.SelfCreated, "invalidContext")]
    public class TestInvalidDependencyBehavior
    {
        protected TestInvalidDependencyBehavior(out TestNonContext invalidContext) =>
            invalidContext = new();
    }

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
        public ContextState<int> Int { get; set; } = 10;
    }

    [TC]
    public class TestContextB { }

    [TC]
    public class TestContextC
    {
        public ContextState<int> Int { get; set; } = 10;
    }

    public class TestNonContext { }
    #endregion
}
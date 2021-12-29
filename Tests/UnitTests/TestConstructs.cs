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
    #endregion

    #region Test Classes
    [TB]
    [TDAttribute<TestContextA>(Binding.Unique, Fulfillment.SelfCreated, "contextA")]
    [TDAttribute<TestContextB>(Binding.Unique, Fulfillment.SelfCreated, "contextB")]
    public class TestBehaviorA
    {
        public static int InstanceCount = 0;

        protected TestBehaviorA(out TestContextA contextA, out TestContextB contextB)
        {
            contextA = new();
            contextB = new();

            InstanceCount++;
        }

        ~TestBehaviorA()
        {
            InstanceCount--;
        }
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

    public class TestNonContext { }
    #endregion
}
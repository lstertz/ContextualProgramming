namespace Tests.Constructs
{
    #region Test Attributes
    public class TBAttribute : BehaviorAttribute { }
    public class TBNonContextDependencyAttribute : BehaviorAttribute { }
    public class TBInvalidDependencyConstructorAttribute : BehaviorAttribute { }
    public class TBInvalidParameterNameConstructorAttribute : BehaviorAttribute { }
    public class TBInvalidParameterCountConstructorAttribute : BehaviorAttribute { }
    public class TBInvalidParameterTypeConstructorAttribute : BehaviorAttribute { }
    public class TBMissingConstructorAttribute : BehaviorAttribute { }
    public class TBNonOutParameterConstructorAttribute : BehaviorAttribute { }
    public class UnusedTBAttribute : BehaviorAttribute { }

    public class TCAttribute : ContextAttribute { }
    public class UnusedTCAttribute : ContextAttribute { }

    public abstract class TDAttribute : DependencyAttribute
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
    public class TestBehaviorA
    {
        protected TestBehaviorA(out TestContextA contextA) => contextA = new();
    }

    [TB]
    public class TestBehaviorB { }


    [TBInvalidDependencyConstructor]
    [TDAttribute<TestContextA>(Binding.Unique, Fulfillment.SelfCreated, "contextA")]
    public class TestInvalidDependencyConstructorBehavior
    {
        protected TestInvalidDependencyConstructorBehavior(out TestContextB contextB) =>
            contextB = new();
    }

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
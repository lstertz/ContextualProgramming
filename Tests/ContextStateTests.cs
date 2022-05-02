using ContextualProgramming.Internal;
using NSubstitute;
using NUnit.Framework;

namespace ContextStateTests;

public class Binding
{
    [Test]
    public void BindStateWithIncorrectBindableValueType_ThrowsException()
    {
        ContextState<int> contextState = 10;

        Assert.Throws<InvalidOperationException>(() =>
            (contextState as IBindableState)?.Bind((_) => new BindableValue<string>("")));
    }

    [Test]
    public void BindStateWithNull_ThrowsException()
    {
        ContextState<int> contextState = 10;
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
        Assert.Throws<ArgumentNullException>(() =>
            (contextState as IBindableState)?.Bind(null));
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
    }

    [Test]
    public void BoundStateWithNoValueChange_DoesNotChangeValueOrFlagChange()
    {
        int value = 10;
        IBindableValue<int> bindableValue = Substitute.For<IBindableValue<int>>();
        bindableValue.ClearReceivedCalls();
        bindableValue.Value = value;

        ContextState<int> contextState = value;
        (contextState as IBindableState)?.Bind((_) => bindableValue);

        contextState.Value = value;

        Assert.AreEqual(value, bindableValue.Value);
        bindableValue.DidNotReceive().FlagAsChanged();
    }

    [Test]
    public void BoundStateWithValueChange_ChangesValueAndFlagsChange()
    {
        int value = 10;
        int expectedValue = 11;
        IBindableValue<int> bindableValue = Substitute.For<IBindableValue<int>>();
        bindableValue.ClearReceivedCalls();
        bindableValue.Value = value;

        ContextState<int> contextState = value;
        (contextState as IBindableState)?.Bind((_) => bindableValue);

        contextState.Value = expectedValue;

        Assert.AreEqual(expectedValue, bindableValue.Value);
        bindableValue.Received(1).FlagAsChanged();
    }

    [Test]
    public void ReboundStateWithValueChange_ChangesValueAndFlagsChange()
    {
        int value = 10;
        int expectedValue = 11;
        IBindableValue<int> bindableValue1 = Substitute.For<IBindableValue<int>>();
        bindableValue1.ClearReceivedCalls();
        bindableValue1.Value = value;

        IBindableValue<int> bindableValue2 = Substitute.For<IBindableValue<int>>();
        bindableValue2.ClearReceivedCalls();
        bindableValue2.Value = value;

        ContextState<int> contextState = value;
        (contextState as IBindableState)?.Bind((_) => bindableValue1);
        (contextState as IBindableState)?.Bind((_) => bindableValue2);

        contextState.Value = expectedValue;

        Assert.AreEqual(value, bindableValue1.Value);
        bindableValue1.DidNotReceive().FlagAsChanged();

        Assert.AreEqual(expectedValue, bindableValue2.Value);
        bindableValue2.Received(1).FlagAsChanged();
    }

    [Test]
    public void UnboundStateWithValueChange_DoesNotChangeValueOrFlagChange()
    {
        int value = 10;
        IBindableValue<int> bindableValue = Substitute.For<IBindableValue<int>>();
        bindableValue.ClearReceivedCalls();
        bindableValue.Value = value;

        ContextState<int> contextState = value;
        (contextState as IBindableState)?.Bind((_) => bindableValue);
        (contextState as IBindableState)?.Unbind();

        contextState.Value = 11;

        Assert.AreEqual(value, bindableValue.Value);
        bindableValue.DidNotReceive().FlagAsChanged();
    }
}

public class Construction
{
    [Test]
    public void ImplicitInt()
    {
        int value = 10;
        ContextState<int> contextState = value;

        int directResult = contextState.Value;
        int implicitResult = contextState;

        Assert.AreEqual(value, directResult);
        Assert.AreEqual(value, implicitResult);
    }

    [Test]
    public void ImplicitString()
    {
        string value = "Test";
        ContextState<string> contextState = value;

        string? directResult = contextState.Value;
        string? implicitResult = contextState;

        Assert.AreEqual(value, directResult);
        Assert.AreEqual(value, implicitResult);
    }

    [Test]
    public void NewInt()
    {
        int value = 10;
        ContextState<int> contextState = new(value);

        int directResult = contextState.Value;
        int implicitResult = contextState;

        Assert.AreEqual(value, directResult);
        Assert.AreEqual(value, implicitResult);
    }

    [Test]
    public void NewString()
    {
        string value = "Test";
        ContextState<string> contextState = new(value);

        string? directResult = contextState.Value;
        string? implicitResult = contextState;

        Assert.AreEqual(value, directResult);
        Assert.AreEqual(value, implicitResult);
    }
}

public class Equality
{
    [Test]
    public void EqualOperator_ContextState()
    {
        int value = 10;
        ContextState<int> a = new(value);
        ContextState<int> b = new(value);

        Assert.IsTrue(a == b);
        Assert.IsTrue(b == a);
    }

    [Test]
    public void EqualOperator_Int()
    {
        int value = 10;
        ContextState<int> contextState = value;

        Assert.IsTrue(value == contextState);
        Assert.IsTrue(contextState == value);
    }

    [Test]
    public void EqualOperator_Null()
    {
        ContextState<string>? contextState = null;

        Assert.IsTrue(null == contextState);
        Assert.IsTrue(contextState == null);
    }

    [Test]
    public void Equals_ContextState()
    {
        int value = 10;
        ContextState<int> a = new(value);
        ContextState<int> b = new(value);
        ContextState<int> c = new(11);

        Assert.IsTrue(a.Equals(b));
        Assert.IsTrue(b.Equals(a));

        Assert.IsFalse(a.Equals(c));
        Assert.IsFalse(c.Equals(a));
    }

    [Test]
    public void Equals_DifferentTypes()
    {
        int value = 10;
        ContextState<int> a = new(value);
        string b = "Test";

        Assert.IsFalse(a.Equals(b));
        Assert.IsFalse(b.Equals(a));
    }

    [Test]
    public void Equals_Int()
    {
        int value = 10;
        int comparedValue = 11;
        ContextState<int> contextState = value;

        Assert.IsTrue(contextState.Equals(value));
        Assert.IsFalse(contextState.Equals(comparedValue));

        Assert.IsTrue(value.Equals(contextState));
        Assert.IsFalse(comparedValue.Equals(contextState));
    }
    [Test]
    public void Equals_Null()
    {
        ContextState<int> contextState = 10;

        Assert.IsFalse(contextState.Equals(null));
    }

    [Test]
    public void InequalOperator_ToContextState()
    {
        int value = 10;
        ContextState<int> a = new(value);
        ContextState<int> b = new(11);

        Assert.IsTrue(a != b);
        Assert.IsTrue(b != a);
    }

    [Test]
    public void InequalOperator_ToInt()
    {
        int value = 10;
        ContextState<int> contextState = value;

        Assert.IsTrue(11 != contextState);
        Assert.IsTrue(contextState != 11);
    }

    [Test]
    public void InequalOperator_ToNull()
    {
        string value = "Test";
        ContextState<string> contextState = value;

        Assert.IsTrue(null != contextState);
        Assert.IsTrue(contextState != null);
    }
}

public class GetHashCode
{
    [Test]
    public void NonNullValue()
    {
        int value = 10;
        ContextState<int> contextState = value;

        Assert.AreEqual(value.GetHashCode(), contextState.GetHashCode());
    }

    [Test]
    public void NullValue()
    {
        ContextState<string> contextState = new(null);

        Assert.AreEqual(0, contextState.GetHashCode());
    }
}

public class ToString
{
    [Test]
    public void NonNullMatchesValueToString()
    {
        int value = 10;
        ContextState<int> contextState = value;

        Assert.AreEqual(10.ToString(), contextState.ToString());
    }

    [Test]
    public void NullProvidesEmptyString()
    {
        ContextState<string> contextState = new ContextState<string>(null);

        Assert.AreEqual(string.Empty, contextState.ToString());
    }
}

public class ValueSetting
{
    [Test]
    public void ValueSets()
    {
        int value = 10;
        ContextState<int> contextState = value;

        int newValue = 11;
        contextState.Value = newValue;

        int directResult = contextState.Value;
        int implicitResult = contextState;

        Assert.AreEqual(newValue, directResult);
        Assert.AreEqual(newValue, implicitResult);
    }
}
using ContextualProgramming.Internal;
using NUnit.Framework;

namespace ContextStateTests;

public class Binding
{
    [Test]
    public void BindStateToNull_ThrowsException()
    {
        ContextState<int> contextState = 10;
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
        Assert.Throws<ArgumentNullException>(() =>
            (contextState as IBindableState)?.Bind(null));
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
    }

    [Test]
    public void BoundState_IsBound_ReturnsTrue()
    {
        ContextState<int> contextState = 10;
        (contextState as IBindableState)?.Bind(() => { });

        Assert.IsTrue((contextState as IBindableState).IsBound);
    }

    [Test]
    public void BoundState_ValueChangeWillNotify()
    {
        bool wasNotified = false;

        ContextState<int> contextState = 10;
        (contextState as IBindableState)?.Bind(() => wasNotified = true);

        contextState.Value = 11;

        Assert.IsTrue(wasNotified);
    }

    [Test]
    public void BoundState_ValueChangeWillNotify_FromNull()
    {
        bool wasNotified = false;

        ContextState<string> contextState = new(null);
        (contextState as IBindableState)?.Bind(() => wasNotified = true);

        contextState.Value = "Test";

        Assert.IsTrue(wasNotified);
    }

    [Test]
    public void BoundState_ValueChangeWillNotify_ToNull()
    {
        bool wasNotified = false;

        ContextState<string> contextState = new("Test");
        (contextState as IBindableState)?.Bind(() => wasNotified = true);

        contextState.Value = null;

        Assert.IsTrue(wasNotified);
    }

    [Test]
    public void BoundState_ValueUnchangedDoesNotNotify()
    {
        bool wasNotified = false;

        int value = 10;
        ContextState<int> contextState = value;
        (contextState as IBindableState)?.Bind(() => wasNotified = true);

        contextState.Value = value;

        Assert.IsFalse(wasNotified);
    }

    [Test]
    public void NewState_IsBound_ReturnsFalse()
    {
        ContextState<int> contextState = 10;

        Assert.IsFalse((contextState as IBindableState).IsBound);
    }

    [Test]
    public void ReboundState_ValueChangeWillNotify()
    {
        bool wasNotified = false;

        int value = 10;
        ContextState<int> contextState = value;
        (contextState as IBindableState)?.Bind(() => wasNotified = false);
        (contextState as IBindableState)?.Bind(() => wasNotified = true);

        contextState.Value = 11;

        Assert.IsTrue(wasNotified);
    }

    [Test]
    public void UnboundState_IsBound_ReturnsFalse()
    {
        ContextState<int> contextState = 10;
        (contextState as IBindableState)?.Bind(() => { });
        (contextState as IBindableState)?.Unbind();

        Assert.IsFalse((contextState as IBindableState).IsBound);
    }

    [Test]
    public void UnboundState_ValueChangeDoesNotNotify()
    {
        bool wasNotified = false;

        int value = 10;
        ContextState<int> contextState = value;
        (contextState as IBindableState)?.Bind(() => wasNotified = true);
        (contextState as IBindableState)?.Unbind();

        contextState.Value = 11;

        Assert.IsFalse(wasNotified);
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
    public void Equals_CastAsBindableState()
    {
        int value = 10;
        ContextState<int> a = new(value);
        ContextState<int> b = new(value);
        ContextState<int> c = new(11);

        Assert.IsTrue(b.Equals(a as IBindableState));
        Assert.IsTrue(a.Equals(b as IBindableState));

        Assert.IsFalse(c.Equals(a as IBindableState));
        Assert.IsFalse(a.Equals(c as IBindableState));
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
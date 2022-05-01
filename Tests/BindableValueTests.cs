using ContextualProgramming.Internal;
using NUnit.Framework;

namespace BindableValueTests;

public class Construction
{
    [Test]
    public void Construction_SetsValues()
    {
        int expected = 1;

        BindableValue<int> value = new(expected);

        Assert.AreEqual(expected, value.Value);
    }
}

public class DeregisterContextState
{
    [Test]
    public void DeregisterContext_DeregisteredForOnChange()
    {
        object expectedContext = new();
        string expectedStateName = "stateName";

        int onChangeCalls = 0;

        BindableValue<int> value = new(1);
        value.OnChange += ((change) =>
        {
            if (change.Context == expectedContext && change.StateName == expectedStateName)
                onChangeCalls++;
        });

        value.RegisterContextState(expectedContext, expectedStateName);
        value.DeregisterContextState(expectedContext);
        value.FlagAsChanged();

        Assert.AreEqual(0, onChangeCalls);
    }

    [Test]
    public void DeregisterForDuplicatedContextAndState_DuplicatesDeregisteredForOnChange()
    {
        object expectedContext = new();
        string expectedStateName = "stateName";

        int onChangeCalls = 0;

        BindableValue<int> value = new(1);
        value.OnChange += ((change) =>
        {
            if (change.Context == expectedContext && change.StateName == expectedStateName)
                onChangeCalls++;
        });

        value.RegisterContextState(expectedContext, expectedStateName);
        value.RegisterContextState(expectedContext, expectedStateName);
        value.DeregisterContextState(expectedContext);
        value.FlagAsChanged();

        Assert.AreEqual(0, onChangeCalls);
    }

    [Test]
    public void DeregisterForSameContextWithDifferentStates_DeregisteredBothForOnChange()
    {
        object expectedContext = new();
        string expectedStateNameA = "stateNameA";
        string expectedStateNameB = "stateNameB";

        int onChangeCalls = 0;

        BindableValue<int> value = new(1);
        value.OnChange += ((change) =>
        {
            if (change.Context == expectedContext && (change.StateName == expectedStateNameA ||
                change.StateName == expectedStateNameB))
                onChangeCalls++;
        });

        value.RegisterContextState(expectedContext, expectedStateNameA);
        value.RegisterContextState(expectedContext, expectedStateNameB);
        value.DeregisterContextState(expectedContext);
        value.FlagAsChanged();

        Assert.AreEqual(0, onChangeCalls);
    }

    [Test]
    public void DeregisterNullContext_ThrowsException()
    {
        BindableValue<int> value = new(1);

#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
        Assert.Throws<ArgumentNullException>(() => value.DeregisterContextState(null));
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
    }
}

public class OnChange
{
    [Test]
    public void Subscribe_InvokedForRegisteredWhenFlagged()
    {
        int onChangeCalls = 0;
        void OnChange(ContextChange _)
        {
            onChangeCalls++;
        }

        BindableValue<int> value = new(1);
        value.OnChange += OnChange;

        value.RegisterContextState(new(), "stateName");
        value.FlagAsChanged();

        Assert.AreEqual(1, onChangeCalls);
    }

    [Test]
    public void Unsubscribe_NotInvokedForRegisteredWhenFlagged()
    {
        int onChangeCalls = 0;
        void OnChange(ContextChange _)
        {
            onChangeCalls++;
        }

        BindableValue<int> value = new(1);
        value.OnChange += OnChange;
        value.OnChange -= OnChange;

        value.RegisterContextState(new(), "stateName");
        value.FlagAsChanged();

        Assert.AreEqual(0, onChangeCalls);
    }
}

public class RegisterContextState
{
    [Test]
    public void RegisterContextAndState_RegisteredForOnChange()
    {
        object expectedContext = new();
        string expectedStateName = "stateName";

        int onChangeCalls = 0;

        BindableValue<int> value = new(1);
        value.OnChange += ((change) =>
        {
            if (change.Context == expectedContext && change.StateName == expectedStateName)
                onChangeCalls++;
        });

        value.RegisterContextState(expectedContext, expectedStateName);
        value.FlagAsChanged();

        Assert.AreEqual(1, onChangeCalls);
    }

    [Test]
    public void RegisterDuplicateContextAndState_DuplicatesRegisteredForOnChange()
    {
        object expectedContext = new();
        string expectedStateName = "stateName";

        int onChangeCalls = 0;

        BindableValue<int> value = new(1);
        value.OnChange += ((change) =>
        {
            if (change.Context == expectedContext && change.StateName == expectedStateName)
                onChangeCalls++;
        });

        value.RegisterContextState(expectedContext, expectedStateName);
        value.RegisterContextState(expectedContext, expectedStateName);
        value.FlagAsChanged();

        Assert.AreEqual(2, onChangeCalls);
    }

    [Test]
    public void RegisterNullContext_ThrowsException()
    {
        BindableValue<int> value = new(1);

#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
        Assert.Throws<ArgumentNullException>(() => value.RegisterContextState(null, "stateName"));
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
    }

    [Test]
    public void RegisterNullStateName_ThrowsException()
    {
        BindableValue<int> value = new(1);

#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
        Assert.Throws<ArgumentNullException>(() => value.RegisterContextState(new(), null));
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
    }
}

public class Value
{
    [Test]
    public void SetValue_ReturnsExpectedValue()
    {
        int expected = 2;

        BindableValue<int> value = new(1);
        value.Value = expected;

        Assert.AreEqual(expected, value.Value);
    }
}
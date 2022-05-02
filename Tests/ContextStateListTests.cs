using ContextualProgramming.Internal;
using NSubstitute;
using NUnit.Framework;

namespace ContextStateListTests;

public class Binding
{
    public static readonly int[] SetupValues = { 10, 11 };

    private IBindableValue<List<int>> _bindableValue = Substitute.For<IBindableValue<List<int>>>();
    private ContextStateList<int> _contextStateList = SetupValues;

    [SetUp]
    public void SetUp()
    {
        _bindableValue.Value = new(SetupValues);
        _bindableValue.ClearReceivedCalls();

        (_contextStateList as IBindableState)?.Bind((_) => _bindableValue);
    }


    [Test]
    public void BindStateWithIncorrectBindableValueType_ThrowsException()
    {
        int[] values = { 10, 11 };
        ContextStateList<int> contextStateList = values;

        Assert.Throws<InvalidOperationException>(() =>
            (contextStateList as IBindableState)?.Bind((_) => new BindableValue<string>("")));
    }

    [Test]
    public void BindStateWithNull_ThrowsException()
    {
        int[] values = { 10, 11 };
        ContextStateList<int> contextStateList = values;

#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
        Assert.Throws<ArgumentNullException>(() =>
            (contextStateList as IBindableState)?.Bind(null));
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
    }

    [Test]
    public void BoundState_Add_ChangesValueAndFlagsChange()
    {
        IEnumerable<int> expectedValue = SetupValues.Append(12);

        _contextStateList.Add(12);

        Assert.AreEqual(expectedValue, _bindableValue.Value);
        _bindableValue.Received(1).FlagAsChanged();
    }

    [Test]
    public void BoundState_AddRange_ChangesValueAndFlagsChange()
    {
        IEnumerable<int> expectedValue = SetupValues.Append(12);
        expectedValue = expectedValue.Append(13);

        _contextStateList.AddRange(new int[] { 12, 13 });

        Assert.AreEqual(expectedValue, _bindableValue.Value);
        _bindableValue.Received(1).FlagAsChanged();
    }

    [Test]
    public void BoundState_ClearOfEmptyList_DoesNotChangeValueOrFlagChange()
    {
        IBindableValue<List<int>> bindableValue = Substitute.For<IBindableValue<List<int>>>();
        bindableValue.Value = new(Array.Empty<int>());

        ContextStateList<int> contextStateList = Array.Empty<int>();
        (contextStateList as IBindableState)?.Bind((_) => bindableValue);

        contextStateList.Clear();

        Assert.AreEqual(Array.Empty<int>(), bindableValue.Value);
        bindableValue.DidNotReceive().FlagAsChanged();
    }

    [Test]
    public void BoundState_Clear_ChangesValueAndFlagsChange()
    {
        _contextStateList.Clear();

        Assert.IsEmpty(_bindableValue.Value);
        _bindableValue.Received(1).FlagAsChanged();
    }

    [Test]
    public void BoundState_ElementChange_ChangesValueAndFlagsChange()
    {
        List<int> expectedValue = new(SetupValues);
        expectedValue[0] = 12;

        _contextStateList[0] = 12;

        Assert.AreEqual(expectedValue, _bindableValue.Value);
        _bindableValue.Received(1).FlagAsChanged();
    }

    [Test]
    public void BoundState_ElementsChange_ChangesValueAndFlagsChange()
    {
        int[] expectedValue = new int[] { 12, 13 };

        _contextStateList.Elements = expectedValue;

        Assert.AreEqual(expectedValue, _bindableValue.Value);
        _bindableValue.Received(1).FlagAsChanged();
    }

    [Test]
    public void BoundState_ElementsUnchanged_DoesNotChangeValueOrFlagChange()
    {
        _contextStateList.Elements = SetupValues;

        Assert.AreEqual(SetupValues, _bindableValue.Value);
        _bindableValue.DidNotReceive().FlagAsChanged();
    }

    [Test]
    public void BoundState_ElementUnchanged_DoesNotChangeValueOrFlagsChange()
    {
        _contextStateList[0] = SetupValues[0];

        Assert.AreEqual(SetupValues, _bindableValue.Value);
        _bindableValue.DidNotReceive().FlagAsChanged();
    }

    [Test]
    public void BoundState_EmptySetToEmpty_DoesNotChangeValueOrFlagsChange()
    {
        IBindableValue<List<int>> bindableValue = Substitute.For<IBindableValue<List<int>>>();
        bindableValue.Value = new(Array.Empty<int>());

        ContextStateList<int> contextStateList = Array.Empty<int>();
        (contextStateList as IBindableState)?.Bind((_) => bindableValue);

        contextStateList.Elements = Array.Empty<int>();

        Assert.AreEqual(SetupValues, _bindableValue.Value);
        _bindableValue.DidNotReceive().FlagAsChanged();
    }

    [Test]
    public void BoundState_EmptySetToNull_DoesNotChangeValueOrFlagsChange()
    {
        IBindableValue<List<int>> bindableValue = Substitute.For<IBindableValue<List<int>>>();
        bindableValue.Value = new(Array.Empty<int>());

        ContextStateList<int> contextStateList = Array.Empty<int>();
        (contextStateList as IBindableState)?.Bind((_) => bindableValue);

        contextStateList.Elements = null;

        Assert.AreEqual(SetupValues, _bindableValue.Value);
        _bindableValue.DidNotReceive().FlagAsChanged();
    }

    [Test]
    public void BoundState_Insert_ChangesValueAndFlagsChange()
    {
        int[] expectedValue = new int[] { 10, 12, 11 };

        _contextStateList.Insert(1, 12);

        Assert.AreEqual(expectedValue, _bindableValue.Value);
        _bindableValue.Received(1).FlagAsChanged();
    }

    [Test]
    public void BoundState_InsertRange_ChangesValueAndFlagsChange()
    {
        int[] expectedValue = new int[] { 10, 12, 13, 11 };

        _contextStateList.InsertRange(1, new int[] { 12, 13 });

        Assert.AreEqual(expectedValue, _bindableValue.Value);
        _bindableValue.Received(1).FlagAsChanged();
    }

    [Test]
    public void BoundState_NonRemove_DoesNotChangeValueOrFlagsChange()
    {
        _contextStateList.Remove(12);

        Assert.AreEqual(SetupValues, _bindableValue.Value);
        _bindableValue.DidNotReceive().FlagAsChanged();
    }

    [Test]
    public void BoundState_Remove_ChangesValueAndFlagsChange()
    {
        IEnumerable<int> expectedValue = new int[] { 11 };

        _contextStateList.Remove(10);

        Assert.AreEqual(expectedValue, _bindableValue.Value);
        _bindableValue.Received(1).FlagAsChanged();
    }

    [Test]
    public void BoundState_RemoveAt_ChangesValueAndFlagsChange()
    {
        IEnumerable<int> expectedValue = new int[] { 11 };

        _contextStateList.RemoveAt(0);

        Assert.AreEqual(expectedValue, _bindableValue.Value);
        _bindableValue.Received(1).FlagAsChanged();
    }

    [Test]
    public void BoundState_Retrieval_DoesNotChangeValueOrFlagsChange()
    {
        int value = _contextStateList[0];

        Assert.AreEqual(SetupValues, _bindableValue.Value);
        _bindableValue.DidNotReceive().FlagAsChanged();
    }

    [Test]
    public void ReboundState_Change_ChangesValueAndFlagsChange()
    {
        IEnumerable<int> expectedValue = new int[] { 12, 11 };

        IBindableValue<List<int>> bindableValue1 = Substitute.For<IBindableValue<List<int>>>();
        bindableValue1.Value = new(SetupValues);

        IBindableValue<List<int>> bindableValue2 = Substitute.For<IBindableValue<List<int>>>();
        bindableValue2.Value = new(SetupValues);

        ContextStateList<int> contextStateList = Array.Empty<int>();
        (contextStateList as IBindableState)?.Bind((_) => bindableValue1);
        (contextStateList as IBindableState)?.Bind((_) => bindableValue2);

        contextStateList[0] = 12;

        Assert.AreEqual(SetupValues, bindableValue1.Value);
        bindableValue1.DidNotReceive().FlagAsChanged();

        Assert.AreEqual(expectedValue, bindableValue2.Value);
        bindableValue2.Received(1).FlagAsChanged();
    }

    [Test]
    public void UnboundState_Add_DoesNotChangeValueOrFlagsChange()
    {
        (_contextStateList as IBindableState)?.Unbind();

        _contextStateList.Add(12);

        Assert.AreEqual(SetupValues, _bindableValue.Value);
        _bindableValue.DidNotReceive().FlagAsChanged();
    }

    [Test]
    public void UnboundState_AddRange_DoesNotChangeValueOrFlagsChange()
    {
        (_contextStateList as IBindableState)?.Unbind();

        _contextStateList.AddRange(new int[] { 12, 13 });

        Assert.AreEqual(SetupValues, _bindableValue.Value);
        _bindableValue.DidNotReceive().FlagAsChanged();
    }

    [Test]
    public void UnboundState_Clear_DoesNotChangeValueOrFlagsChange()
    {
        (_contextStateList as IBindableState)?.Unbind();

        _contextStateList.Clear();

        Assert.AreEqual(SetupValues, _bindableValue.Value);
        _bindableValue.DidNotReceive().FlagAsChanged();
    }

    [Test]
    public void UnboundState_ElementChange_DoesNotChangeValueOrFlagsChange()
    {
        (_contextStateList as IBindableState)?.Unbind();

        _contextStateList[0] = 12;

        Assert.AreEqual(SetupValues, _bindableValue.Value);
        _bindableValue.DidNotReceive().FlagAsChanged();
    }

    [Test]
    public void UnboundState_ElementsChange_DoesNotChangeValueOrFlagsChange()
    {
        (_contextStateList as IBindableState)?.Unbind();

        _contextStateList.Elements = new int[] { 12, 13 };

        Assert.AreEqual(SetupValues, _bindableValue.Value);
        _bindableValue.DidNotReceive().FlagAsChanged();
    }

    [Test]
    public void UnboundState_Insert_DoesNotChangeValueOrFlagsChange()
    {
        (_contextStateList as IBindableState)?.Unbind();

        _contextStateList.Insert(1, 12);

        Assert.AreEqual(SetupValues, _bindableValue.Value);
        _bindableValue.DidNotReceive().FlagAsChanged();
    }

    [Test]
    public void UnboundState_InsertRange_DoesNotChangeValueOrFlagsChange()
    {
        (_contextStateList as IBindableState)?.Unbind();

        _contextStateList.InsertRange(1, new int[] { 12, 13 });

        Assert.AreEqual(SetupValues, _bindableValue.Value);
        _bindableValue.DidNotReceive().FlagAsChanged();
    }

    [Test]
    public void UnboundState_RemoveAt_DoesNotChangeValueOrFlagsChange()
    {
        (_contextStateList as IBindableState)?.Unbind();

        _contextStateList.RemoveAt(0);

        Assert.AreEqual(SetupValues, _bindableValue.Value);
        _bindableValue.DidNotReceive().FlagAsChanged();
    }

    [Test]
    public void UnboundState_Remove_DoesNotChangeValueOrFlagsChange()
    {
        (_contextStateList as IBindableState)?.Unbind();

        _contextStateList.Remove(10);

        Assert.AreEqual(SetupValues, _bindableValue.Value);
        _bindableValue.DidNotReceive().FlagAsChanged();
    }
}

public class Construction
{
    [Test]
    public void ImplicitArray()
    {
        int[] values = { 10, 11 };
        ContextStateList<int> contextStateList = values;

        int[]? directResult = contextStateList.Elements;
        int[]? implicitResult = contextStateList;

        Assert.AreEqual(values, directResult);
        Assert.AreEqual(values, implicitResult);
    }

    [Test]
    public void NewArray()
    {
        int[] values = { 10, 11 };
        ContextStateList<int> contextStateList = new(values);

        int[]? directResult = contextStateList.Elements;
        int[]? implicitResult = contextStateList;

        Assert.AreEqual(values, directResult);
        Assert.AreEqual(values, implicitResult);
    }
}

public class Count
{
    [Test]
    public void Add_ReturnsIncrementedCount()
    {
        ContextStateList<int> contextStateList = Array.Empty<int>();

        contextStateList.Add(1);
        Assert.AreEqual(1, contextStateList.Count);
    }

    [Test]
    public void AddRange_ReturnsIncreasedCount()
    {
        int[] array = new int[] { 10, 11 };
        int count = array.Length;

        ContextStateList<int> contextStateList = Array.Empty<int>();

        contextStateList.AddRange(array);
        Assert.AreEqual(count, contextStateList.Count);
    }

    [Test]
    public void Clear_ReturnsZero()
    {
        ContextStateList<int> contextStateList = new int[] {10, 11};

        contextStateList.Clear();
        Assert.AreEqual(0, contextStateList.Count);
    }

    [Test]
    public void ConstructedEmpty_ReturnsZero()
    {
        ContextStateList<int> contextStateList = Array.Empty<int>();

        Assert.AreEqual(0, contextStateList.Count);
    }

    [Test]
    public void ConstructedPopulated_ReturnsCount()
    {
        int[] array = new int[] {10, 11};
        int count = array.Length;

        ContextStateList<int> contextStateList = array;

        Assert.AreEqual(count, contextStateList.Count);
    }

    [Test]
    public void Insert_ReturnsIncrementedCount()
    {
        ContextStateList<int> contextStateList = new int[] { 10, 11 };

        contextStateList.Insert(1, 12);
        Assert.AreEqual(3, contextStateList.Count);
    }

    [Test]
    public void InsertRange_ReturnsIncreasedCount()
    {
        ContextStateList<int> contextStateList = new int[] { 10, 11 };

        contextStateList.InsertRange(1, new int[] {12, 13});
        Assert.AreEqual(4, contextStateList.Count);
    }

    [Test]
    public void Remove_ReturnsDecrementedCount()
    {
        ContextStateList<int> contextStateList = new int[] { 10, 11 };

        contextStateList.Remove(10);
        Assert.AreEqual(1, contextStateList.Count);
    }

    [Test]
    public void RemoveNonExisting_ReturnsOriginalCount()
    {
        ContextStateList<int> contextStateList = new int[] { 10, 11 };

        contextStateList.Remove(1);
        Assert.AreEqual(2, contextStateList.Count);
    }
}

public class Equality
{
    [Test]
    public void EqualOperator_Array()
    {
        int[] values = { 10, 11 };
        ContextStateList<int> contextStateList = values;

        Assert.IsTrue(values == contextStateList);
        Assert.IsTrue(contextStateList == values);
    }

    [Test]
    public void EqualOperator_ContextStateList()
    {
        int[] values = { 10, 11 };
        ContextStateList<int> a = values;
        ContextStateList<int> b = values;

        Assert.IsTrue(a == b);
        Assert.IsTrue(b == a);
    }

    [Test]
    public void EqualOperator_Null()
    {
        ContextStateList<int>? contextStateList = null;

        Assert.IsTrue(null == contextStateList);
        Assert.IsTrue(contextStateList == null);
    }

    [Test]
    public void Equals_Array()
    {
        int[] values = { 10, 11 };
        int[] comparedValues = { 11, 12 };
        ContextStateList<int> contextStateList = values;

        Assert.IsTrue(contextStateList.Equals(values));
        Assert.IsFalse(contextStateList.Equals(comparedValues));
    }

    [Test]
    public void Equals_ContextStateList()
    {
        int[] values = { 10, 11 };
        int[] comparedValues = { 11, 12 };

        ContextStateList<int> a = new(values);
        ContextStateList<int> b = new(values);
        ContextStateList<int> c = new(comparedValues);

        Assert.IsTrue(a.Equals(b));
        Assert.IsTrue(b.Equals(a));

        Assert.IsFalse(a.Equals(c));
        Assert.IsFalse(c.Equals(a));
    }

    [Test]
    public void Equals_DifferentTypes()
    {
        int[] values = { 10, 11 };
        string[] comparedValues = { "a", "b" };

        ContextStateList<int> a = new(values);
        ContextStateList<string> b = new(comparedValues);

        Assert.IsFalse(a.Equals(b));
        Assert.IsFalse(b.Equals(a));
    }

    [Test]
    public void Equals_Null()
    {
        int[] values = { 10, 11 };
        ContextStateList<int> contextStateList = new(values);

        Assert.IsFalse(contextStateList.Equals(null));
    }

    [Test]
    public void InequalOperator_Array()
    {
        int[] values = { 10, 11 };
        int[] comparedValues = { 11, 12 };
        ContextStateList<int> contextStateList = new(values);

        Assert.IsTrue(comparedValues != contextStateList);
        Assert.IsTrue(contextStateList != comparedValues);
    }

    [Test]
    public void InequalOperator_ContextState()
    {
        int[] values = { 10, 11 };
        ContextStateList<int> a = new(values);

        int[] comparedValues = { 11, 12 };
        ContextStateList<int> b = new(comparedValues);

        Assert.IsTrue(a != b);
        Assert.IsTrue(b != a);
    }

    [Test]
    public void InequalOperator_Null()
    {
        int[] values = { 10, 11 };
        ContextStateList<int> contextStateList = values;

        Assert.IsTrue(null != contextStateList);
        Assert.IsTrue(contextStateList != null);
    }
}

public class ListOperations
{
    [Test]
    public void Add_ElementIsAdded()
    {
        int[] values = { 10, 11 };
        int[] expected = { 10, 11, 12 };
        ContextStateList<int> contextStateList = values;

        contextStateList.Add(12);

        Assert.AreEqual(expected, contextStateList.Elements);
    }

    [Test]
    public void AddRange_ElementsAreAdded()
    {
        int[] values = { 10, 11 };
        int[] expected = { 10, 11, 12, 13 };
        ContextStateList<int> contextStateList = values;

        contextStateList.AddRange(new[] { 12, 13 });

        Assert.AreEqual(expected, contextStateList.Elements);
    }

    [Test]
    public void Clear_ElementsAreRemoved()
    {
        int[] values = { 10, 11 };
        ContextStateList<int> contextStateList = values;

        contextStateList.Clear();

        Assert.AreEqual(Array.Empty<int>(), contextStateList.Elements);
    }

    [Test]
    public void Getting_ProvidesExpectedElement()
    {
        int[] values = { 10, 11 };
        ContextStateList<int> contextStateList = values;

        Assert.AreEqual(values[0], contextStateList[0]);
        Assert.AreEqual(values[1], contextStateList[1]);
    }

    [Test]
    public void Insert_ElementIsInserted()
    {
        int[] values = { 10, 11 };
        int[] expected = { 10, 12, 11 };
        ContextStateList<int> contextStateList = values;

        contextStateList.Insert(1, 12);

        Assert.AreEqual(expected, contextStateList.Elements);
    }

    [Test]
    public void Insert_OutOfRangeThrowsException()
    {
        int[] values = { 10, 11 };
        ContextStateList<int> contextStateList = values;

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            contextStateList.Insert(3, 12));
    }

    [Test]
    public void InsertRange_ElementsAreInserted()
    {
        int[] values = { 10, 11 };
        int[] expected = { 10, 12, 13, 11 };
        ContextStateList<int> contextStateList = values;

        contextStateList.InsertRange(1, new[] { 12, 13 });

        Assert.AreEqual(expected, contextStateList.Elements);
    }

    [Test]
    public void InsertRange_OutOfRangeThrowsException()
    {
        int[] values = { 10, 11 };
        ContextStateList<int> contextStateList = values;

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            contextStateList.InsertRange(3, new[] { 12, 13 }));
    }

    [Test]
    public void Remove_ElementIsRemoved()
    {
        int[] values = { 10, 11 };
        int[] expected = { 11 };
        ContextStateList<int> contextStateList = values;

        contextStateList.Remove(10);

        Assert.AreEqual(expected, contextStateList.Elements);
    }

    [Test]
    public void RemoveAt_ElementIsRemoved()
    {
        int[] values = { 10, 11 };
        int[] expected = { 11 };
        ContextStateList<int> contextStateList = values;

        contextStateList.RemoveAt(0);

        Assert.AreEqual(expected, contextStateList.Elements);
    }

    [Test]
    public void RemoveAt_OutOfRangeThrowsException()
    {
        int[] values = { 10, 11 };
        int[] expected = { 11 };
        ContextStateList<int> contextStateList = values;

        Assert.Throws<ArgumentOutOfRangeException>(() => contextStateList.RemoveAt(4));
    }

    [Test]
    public void Setting_ElementSets()
    {
        int[] values = { 10, 11 };
        int[] expected = { 10, 12 };
        ContextStateList<int> contextStateList = values;

        contextStateList[1] = expected[1];

        Assert.AreEqual(expected, contextStateList.Elements);
    }

    [Test]
    public void SettingAll_ElementsAreSet()
    {
        int[] values = { 10, 11 };
        int[] expected = { 10, 11, 12 };
        ContextStateList<int> contextStateList = values;

        contextStateList.Elements = expected;

        Assert.AreEqual(expected, contextStateList.Elements);
    }

    [Test]
    public void SettingAll_NullSetsEmptyList()
    {
        int[] values = { 10, 11 };
        ContextStateList<int> contextStateList = values;

        contextStateList.Elements = null;

        Assert.AreEqual(Array.Empty<int>(), contextStateList.Elements);
    }
}

public class ToString
{
    [Test]
    public void MatchesListToString()
    {
        int[] values = { 10, 11 };
        ContextStateList<int> contextStateList = new(values);

        string? expected = new List<int>(values).ToString();
        Assert.AreEqual(expected, contextStateList.ToString());
    }
}
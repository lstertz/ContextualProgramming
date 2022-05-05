using ContextualProgramming.Internal;
using NUnit.Framework;

namespace ReadonlyContextStateListTests;

public class Construction
{
    [Test]
    public void ImplicitArray()
    {
        int[] values = { 10, 11 };
        ReadonlyContextStateList<int> ReadonlyContextStateList = values;

        int[]? directResult = ReadonlyContextStateList.Elements;
        int[]? implicitResult = ReadonlyContextStateList;

        Assert.AreEqual(values, directResult);
        Assert.AreEqual(values, implicitResult);
    }

    [Test]
    public void NewArray()
    {
        int[] values = { 10, 11 };
        ReadonlyContextStateList<int> ReadonlyContextStateList = new(values);

        int[]? directResult = ReadonlyContextStateList.Elements;
        int[]? implicitResult = ReadonlyContextStateList;

        Assert.AreEqual(values, directResult);
        Assert.AreEqual(values, implicitResult);
    }
}

public class Count
{
    [Test]
    public void ConstructedEmpty_ReturnsZero()
    {
        ReadonlyContextStateList<int> ReadonlyContextStateList = Array.Empty<int>();

        Assert.AreEqual(0, ReadonlyContextStateList.Count);
    }

    [Test]
    public void ConstructedPopulated_ReturnsCount()
    {
        int[] array = new int[] {10, 11};
        int count = array.Length;

        ReadonlyContextStateList<int> ReadonlyContextStateList = array;

        Assert.AreEqual(count, ReadonlyContextStateList.Count);
    }
}

public class Equality
{
    [Test]
    public void EqualOperator_Array()
    {
        int[] values = { 10, 11 };
        ReadonlyContextStateList<int> ReadonlyContextStateList = values;

        Assert.IsTrue(values == ReadonlyContextStateList);
        Assert.IsTrue(ReadonlyContextStateList == values);
    }

    [Test]
    public void EqualOperator_ReadonlyContextStateList()
    {
        int[] values = { 10, 11 };
        ReadonlyContextStateList<int> a = values;
        ReadonlyContextStateList<int> b = values;

        Assert.IsTrue(a == b);
        Assert.IsTrue(b == a);
    }

    [Test]
    public void EqualOperator_Null()
    {
        ReadonlyContextStateList<int>? ReadonlyContextStateList = null;

        Assert.IsTrue(null == ReadonlyContextStateList);
        Assert.IsTrue(ReadonlyContextStateList == null);
    }

    [Test]
    public void Equals_Array()
    {
        int[] values = { 10, 11 };
        int[] comparedValues = { 11, 12 };
        ReadonlyContextStateList<int> ReadonlyContextStateList = values;

        Assert.IsTrue(ReadonlyContextStateList.Equals(values));
        Assert.IsFalse(ReadonlyContextStateList.Equals(comparedValues));
    }

    [Test]
    public void Equals_CastAsIEquatable()
    {
        int[] values = { 10, 11 };
        int[] comparedValues = { 11, 12 };

        ReadonlyContextStateList<int> a = new(values);
        ReadonlyContextStateList<int> b = new(values);
        ReadonlyContextStateList<int> c = new(comparedValues);

        Assert.IsTrue(b.Equals(a as IEquatable<State<int[]>>));
        Assert.IsTrue(a.Equals(b as IEquatable<State<int[]>>));

        Assert.IsFalse(c.Equals(a as IEquatable<State<int[]>>));
        Assert.IsFalse(a.Equals(c as IEquatable<State<int[]>>));
    }

    [Test]
    public void Equals_ReadonlyContextStateList()
    {
        int[] values = { 10, 11 };
        int[] comparedValues = { 11, 12 };

        ReadonlyContextStateList<int> a = new(values);
        ReadonlyContextStateList<int> b = new(values);
        ReadonlyContextStateList<int> c = new(comparedValues);

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

        ReadonlyContextStateList<int> a = new(values);
        ReadonlyContextStateList<string> b = new(comparedValues);

        Assert.IsFalse(a.Equals(b));
        Assert.IsFalse(b.Equals(a));
    }

    [Test]
    public void Equals_Null()
    {
        int[] values = { 10, 11 };
        ReadonlyContextStateList<int> ReadonlyContextStateList = new(values);

        Assert.IsFalse(ReadonlyContextStateList.Equals(null));
    }

    [Test]
    public void InequalOperator_Array()
    {
        int[] values = { 10, 11 };
        int[] comparedValues = { 11, 12 };
        ReadonlyContextStateList<int> ReadonlyContextStateList = new(values);

        Assert.IsTrue(comparedValues != ReadonlyContextStateList);
        Assert.IsTrue(ReadonlyContextStateList != comparedValues);
    }

    [Test]
    public void InequalOperator_ContextState()
    {
        int[] values = { 10, 11 };
        ReadonlyContextStateList<int> a = new(values);

        int[] comparedValues = { 11, 12 };
        ReadonlyContextStateList<int> b = new(comparedValues);

        Assert.IsTrue(a != b);
        Assert.IsTrue(b != a);
    }

    [Test]
    public void InequalOperator_Null()
    {
        int[] values = { 10, 11 };
        ReadonlyContextStateList<int> ReadonlyContextStateList = values;

        Assert.IsTrue(null != ReadonlyContextStateList);
        Assert.IsTrue(ReadonlyContextStateList != null);
    }
}

public class ToString
{
    [Test]
    public void MatchesArrayToString()
    {
        int[] values = { 10, 11 };
        ReadonlyContextStateList<int> ReadonlyContextStateList = new(values);

        string? expected = values.ToString();
        Assert.AreEqual(expected, ReadonlyContextStateList.ToString());
    }
}
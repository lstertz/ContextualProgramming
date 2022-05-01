using ContextualProgramming.Internal;
using NUnit.Framework;

namespace ContextChangeTests;

public class Construction
{

    [Test]
    public void Construction_SetsValues()
    {
        string expectedName = "stateName";
        object expectedContext = new();

        ContextChange change = new(expectedContext, expectedName);

        Assert.AreEqual(expectedContext, change.Context);
        Assert.AreEqual(expectedName, change.StateName);
    }

    [Test]
    public void EnsuresNonNullContext()
    {
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
        Assert.Throws<ArgumentNullException>(() => new ContextChange(null, "stateName"));
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
    }

    [Test]
    public void EnsuresNonNullStateName()
    {
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
        Assert.Throws<ArgumentNullException>(() => new ContextChange(new(), null));
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
    }
}
using NUnit.Framework;

namespace OnChangeAttributeTests;

public class Construction
{
    [Test]
    public void AssignsProperties_NoSpecifiedContextState()
    {
        string expectedDependencyName = "dependencyName";
        string? expectedContextStateName = null;

        OnChangeAttribute attr = new OnChangeAttribute(expectedDependencyName);

        Assert.AreEqual(expectedDependencyName, attr.DependencyName);
        Assert.AreEqual(expectedContextStateName, attr.ContextStateName);
    }

    [Test]
    public void AssignsProperties_SpecifiedContextState()
    {
        string expectedDependencyName = "dependencyName";
        string? expectedContextStateName = "contextStateName";

        OnChangeAttribute attr = new OnChangeAttribute(expectedDependencyName,
            expectedContextStateName);

        Assert.AreEqual(expectedDependencyName, attr.DependencyName);
        Assert.AreEqual(expectedContextStateName, attr.ContextStateName);
    }

    [Test]
    public void EmptyDependencyNameThrowsException()
    {
        Assert.Throws<ArgumentException>(() =>
            new OnChangeAttribute(string.Empty));
    }

    [Test]
    public void NullDependencyNameThrowsException()
    {
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
        Assert.Throws<ArgumentException>(() =>
            new OnChangeAttribute(string.Empty));
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
    }
}
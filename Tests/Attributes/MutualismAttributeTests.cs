using NUnit.Framework;
using Tests.Constructs;

namespace MutualismAttributeTests;

public class Construction
{
    private class NonGenericMutualismAttribute : MutualismAttribute
    {
        public NonGenericMutualismAttribute(string name, Type type) : base(name, type) { }
    }

    [Test]
    public void Construction_AssignsProperties()
    {
        string expectedName = "name";

        MutualismAttribute attr = new MutualismAttribute<TestContextA>(expectedName);

        Assert.AreEqual(expectedName, attr.Name);
    }

    [Test]
    public void EmptyName_ThrowsException()
    {
        Assert.Throws<ArgumentException>(() => 
        new MutualismAttribute<TestContextA>(string.Empty));
    }

    [Test]
    public void NullName_ThrowsException()
    {
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
        Assert.Throws<ArgumentException>(() =>
            new MutualismAttribute<TestContextA>(null));
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
    }

    [Test]
    public void NullType_ThrowsException()
    {
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
        Assert.Throws<ArgumentNullException>(() =>
            new NonGenericMutualismAttribute("name", null));
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
    }
}
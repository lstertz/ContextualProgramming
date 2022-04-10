using NUnit.Framework;
using Tests.Constructs;

namespace MutualismAttributeTests;

public class Construction
{
    private class NonGenericMutualismAttribute : MutualismAttribute
    {
        public NonGenericMutualismAttribute(string name, Type type) : 
            base(name, Relationship.Exclusive, type) { }
    }

    [Test]
    public void Construction_AssignsProperties()
    {
        string expectedName = "name";
        Relationship expectedRelationship = Relationship.Exclusive;

        MutualismAttribute attr = new MutualismAttribute<TestContextA>(
            expectedName, expectedRelationship);

        Assert.AreEqual(expectedName, attr.Name);
        Assert.AreEqual(expectedRelationship, attr.Relationship);
    }

    [Test]
    public void EmptyName_ThrowsException()
    {
        Assert.Throws<ArgumentException>(() => 
        new MutualismAttribute<TestContextA>(string.Empty, Relationship.Exclusive));
    }

    [Test]
    public void NullName_ThrowsException()
    {
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
        Assert.Throws<ArgumentException>(() =>
            new MutualismAttribute<TestContextA>(null, Relationship.Exclusive));
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
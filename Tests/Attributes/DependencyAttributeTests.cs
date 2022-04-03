using NUnit.Framework;
using Tests.Constructs;

namespace DependencyAttributeTests;

public class Construction
{
    private class NonGenericDependencyAttribute : DependencyAttribute
    {
        public NonGenericDependencyAttribute(Binding binding, Fulfillment fulfillment,
            string name, Type type) : base(binding, fulfillment, name, type) { }
    }

    [Test]
    public void Construction_AssignsProperties()
    {
        string expectedName = "name";
        Binding expectedBinding = Binding.Unique;
        Fulfillment expectedFulfillment = Fulfillment.SelfCreated;

        DependencyAttribute attr = new DependencyAttribute<TestContextA>(
            expectedBinding, expectedFulfillment, expectedName);

        Assert.AreEqual(expectedFulfillment, attr.Fulfillment);
        Assert.AreEqual(expectedBinding, attr.Binding);
        Assert.AreEqual(expectedName, attr.Name);
    }

    [Test]
    public void EmptyName_ThrowsException()
    {
        Assert.Throws<ArgumentException>(() =>
            new DependencyAttribute<TestContextA>(Binding.Unique, Fulfillment.SelfCreated,
            string.Empty));
    }

    [Test]
    public void NullName_ThrowsException()
    {
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
        Assert.Throws<ArgumentException>(() =>
            new DependencyAttribute<TestContextA>(Binding.Unique, Fulfillment.SelfCreated,
            null));
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
    }

    [Test]
    public void NullType_ThrowsException()
    {
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
        Assert.Throws<ArgumentNullException>(() =>
            new NonGenericDependencyAttribute(Binding.Unique, Fulfillment.SelfCreated,
            "name", null));
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
    }
}
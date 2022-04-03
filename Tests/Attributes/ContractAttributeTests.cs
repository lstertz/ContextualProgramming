using NUnit.Framework;
using Tests.Constructs;

namespace ContractAttributeTests;

public class Construction
{
    private class NonGenericContractAttribute : ContractAttribute
    {
        public NonGenericContractAttribute(string name, Type type) : base(name, type) { }
    }

    [Test]
    public void Construction_AssignsProperties()
    {
        string expectedName = "name";

        ContractAttribute attr = new ContractAttribute<TestContextA>(expectedName);

        Assert.AreEqual(expectedName, attr.Name);
    }

    [Test]
    public void EmptyName_ThrowsException()
    {
        Assert.Throws<ArgumentException>(() => 
        new ContractAttribute<TestContextA>(string.Empty));
    }

    [Test]
    public void NullName_ThrowsException()
    {
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
        Assert.Throws<ArgumentException>(() =>
            new ContractAttribute<TestContextA>(null));
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
    }

    [Test]
    public void NullType_ThrowsException()
    {
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
        Assert.Throws<ArgumentNullException>(() =>
            new NonGenericContractAttribute("name", null));
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
    }
}
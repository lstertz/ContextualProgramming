using NUnit.Framework;

namespace MutualAttributeTests;

public class Construction
{
    [Test]
    public void Construction_AssignsProperties()
    {
        string expectedMutualistName = "MutualistName";
        string expectedStateName = "StateName";

        MutualAttribute attr = new(expectedMutualistName, expectedStateName);

        Assert.AreEqual(expectedMutualistName, attr.MutualistName);
        Assert.AreEqual(expectedStateName, attr.StateName);
    }

    [Test]
    public void EmptyMutualistName_ThrowsException()
    {
        Assert.Throws<ArgumentException>(() => new MutualAttribute(string.Empty, "StateName"));
    }

    [Test]
    public void EmptyStateName_ThrowsException()
    {
        Assert.Throws<ArgumentException>(() => new MutualAttribute("MutualistName", string.Empty));
    }

    [Test]
    public void NullMutualistName_ThrowsException()
    {
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
        Assert.Throws<ArgumentException>(() => new MutualAttribute(null, "StateName"));
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
    }

    [Test]
    public void NullStateName_ThrowsException()
    {
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
        Assert.Throws<ArgumentException>(() => new MutualAttribute("MutualistName", null));
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
    }
}
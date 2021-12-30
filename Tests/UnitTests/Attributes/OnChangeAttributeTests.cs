using NUnit.Framework;

namespace Tests
{
    public class OnChangeAttributeTests
    {
        #region Construction
        [Test]
        public void Construction_AssignsProperties_NoSpecifiedContextState()
        {
            string expectedDependencyName = "dependencyName";
            string? expectedContextStateName = null;

            OnChangeAttribute attr = new OnChangeAttribute(expectedDependencyName);

            Assert.AreEqual(expectedDependencyName, attr.DependencyName);
            Assert.AreEqual(expectedContextStateName, attr.ContextStateName);
        }

        [Test]
        public void Construction_AssignsProperties_SpecifiedContextState()
        {
            string expectedDependencyName = "dependencyName";
            string? expectedContextStateName = "contextStateName";

            OnChangeAttribute attr = new OnChangeAttribute(expectedDependencyName, 
                expectedContextStateName);

            Assert.AreEqual(expectedDependencyName, attr.DependencyName);
            Assert.AreEqual(expectedContextStateName, attr.ContextStateName);
        }

        [Test]
        public void Construction_EmptyDependencyNameThrowsException()
        {
            Assert.Throws<ArgumentException>(() =>
                new OnChangeAttribute(string.Empty));
        }

        [Test]
        public void Construction_NullDependencyNameThrowsException()
        {
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            Assert.Throws<ArgumentException>(() =>
                new OnChangeAttribute(string.Empty));
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
        }
        #endregion
    }
}
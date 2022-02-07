using ContextualProgramming.Internal;
using NUnit.Framework;

namespace BehaviorInstanceTests
{
    public class TestBehavior { }

    public class TestContext { }

    public class Construction
    {
        [Test]
        public void NullBehaviorThrowsExcpetion()
        {
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            Assert.Throws<ArgumentNullException>(() =>
                new BehaviorInstance(null, new(), Array.Empty<object>()));
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
        }

        [Test]
        public void SetsProperties()
        {
            TestContext expectedContext = new();
            string expectedContextName = "A";
            TestBehavior expectedBehavior = new();
            object[] expectedSelfCreatedContexts = new object[] { expectedContext };

            BehaviorInstance instance = new(expectedBehavior,
                new()
                {
                    { expectedContextName, expectedContext }
                }, expectedSelfCreatedContexts);

            Assert.AreEqual(expectedBehavior, instance.Behavior);

            Assert.AreEqual(1, instance.Contexts.Count);
            Assert.IsTrue(instance.Contexts[expectedContextName] == expectedContext);

            Assert.AreEqual(1, instance.ContextNames.Count);
            Assert.IsTrue(instance.ContextNames[expectedContext] == expectedContextName);

            Assert.AreEqual(expectedSelfCreatedContexts, instance.SelfCreatedContexts);
        }
    }
}
using ContextualProgramming.Internal;
using NUnit.Framework;

namespace MutualismFulfillerTests
{
    #region Shared Constructs
    public class TestContextA { }

    public class TestContextB { }

    public class TestContextC { }
    #endregion

    public class Construction
    {
        [Test]
        public void SetsMutualistContextTypes_Null_NoMutualistContextTypes()
        {
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            MutualismFulfiller fulfiller = new(null);
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.

            Assert.IsEmpty(fulfiller.MutualistContextTypes);
        }

        [Test]
        public void SetsMutualistContextTypes_WithMutualism_HasMutualistContextTypes()
        {
            MutualismFulfiller fulfiller = new(new()
            {
                { "A", typeof(TestContextA)},
                { "B", typeof(TestContextB) },
            });

            Assert.AreEqual(2, fulfiller.MutualistContextTypes.Length);
            Assert.Contains(typeof(TestContextA), fulfiller.MutualistContextTypes);
            Assert.Contains(typeof(TestContextB), fulfiller.MutualistContextTypes);
        }

        [Test]
        public void SetsMutualistContextTypes_WithDuplicateMutualism_HasMutualistContextTypes()
        {
            MutualismFulfiller fulfiller = new(new()
            {
                { "A", typeof(TestContextA) },
                { "B", typeof(TestContextA) },
            });

            Assert.AreEqual(2, fulfiller.MutualistContextTypes.Length);
            Assert.Contains(typeof(TestContextA), fulfiller.MutualistContextTypes);
            Assert.Contains(typeof(TestContextA), fulfiller.MutualistContextTypes);
        }

        [Test]
        public void SetsMutualistContextTypes_WithNoMutualism_NoMutualistContextTypes()
        {
            MutualismFulfiller fulfiller = new(new());

            Assert.IsEmpty(fulfiller.MutualistContextTypes);
        }
    }

    public class Fulfill
    {
#pragma warning disable CS8618
        private MutualismFulfiller _fulfiller;
#pragma warning restore CS8618

        [SetUp]
        public void SetUp()
        {
            _fulfiller = new(new()
            {
                { "A", typeof(TestContextA) },
                { "B", typeof(TestContextB) },
            });
        }

        [Test]
        public void NullContext_ThrowsException()
        {
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            Assert.Throws<ArgumentNullException>(() => _fulfiller.Fulfill(null));
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
        }

        [Test]
        public void WithMutualistContexts_ReturnsMatchingInstantiatedContexts()
        {
            object[] mutualistContexts = _fulfiller.Fulfill(new TestContextC());

            Assert.AreEqual(2, mutualistContexts.Length);
            Assert.AreEqual(typeof(TestContextA), mutualistContexts[0].GetType());
            Assert.AreEqual(typeof(TestContextB), mutualistContexts[1].GetType());
        }

        [Test]
        public void WithNoMutualistContexts_ReturnsNoInstantiatedContexts()
        {
            MutualismFulfiller fulfiller = new(new());

            object[] mutualistContexts = fulfiller.Fulfill(new TestContextC());

            Assert.IsEmpty(mutualistContexts);
        }
    }
}
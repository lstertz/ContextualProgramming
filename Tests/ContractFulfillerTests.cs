using ContextualProgramming.Internal;
using NUnit.Framework;

namespace ContractFulfillerTests
{
    #region Shared Constructs
    public class TestContextA { }

    public class TestContextB { }

    public class TestContextC { }
    #endregion

    public class Construction
    {
        [Test]
        public void SetsContractedContextTypes_Null_NoContractedContextTypes()
        {
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            ContractFulfiller fulfiller = new(null);
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.

            Assert.IsEmpty(fulfiller.ContractedContextTypes);
        }

        [Test]
        public void SetsContractedContextTypes_WithContracts_HasContractedContextTypes()
        {
            ContractFulfiller fulfiller = new(new()
            {
                { "A", typeof(TestContextA)},
                { "B", typeof(TestContextB) },
            });

            Assert.AreEqual(2, fulfiller.ContractedContextTypes.Length);
            Assert.Contains(typeof(TestContextA), fulfiller.ContractedContextTypes);
            Assert.Contains(typeof(TestContextB), fulfiller.ContractedContextTypes);
        }

        [Test]
        public void SetsContractedContextTypes_WithDuplicateContracts_HasContractedContextTypes()
        {
            ContractFulfiller fulfiller = new(new()
            {
                { "A", typeof(TestContextA) },
                { "B", typeof(TestContextA) },
            });

            Assert.AreEqual(2, fulfiller.ContractedContextTypes.Length);
            Assert.Contains(typeof(TestContextA), fulfiller.ContractedContextTypes);
            Assert.Contains(typeof(TestContextA), fulfiller.ContractedContextTypes);
        }

        [Test]
        public void SetsContractedContextTypes_WithNoContracts_NoContractedContextTypes()
        {
            ContractFulfiller fulfiller = new(new());

            Assert.IsEmpty(fulfiller.ContractedContextTypes);
        }
    }

    public class Fulfill
    {
#pragma warning disable CS8618
        private ContractFulfiller _fulfiller;
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
        public void WithContractedContexts_ReturnsMatchingInstantiatedContexts()
        {
            object[] contractedContexts = _fulfiller.Fulfill(new TestContextC());

            Assert.AreEqual(2, contractedContexts.Length);
            Assert.AreEqual(typeof(TestContextA), contractedContexts[0].GetType());
            Assert.AreEqual(typeof(TestContextB), contractedContexts[1].GetType());
        }

        [Test]
        public void WithNoContractedContexts_ReturnsNoInstantiatedContexts()
        {
            ContractFulfiller fulfiller = new(new());

            object[] contractedContexts = fulfiller.Fulfill(new TestContextC());

            Assert.IsEmpty(contractedContexts);
        }
    }
}
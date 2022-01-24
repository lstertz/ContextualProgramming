using ContextualProgramming.Internal;
using NSubstitute;
using NUnit.Framework;
using System.Reflection;

namespace BehaviorFactoryTests
{
    public static class SetUp
    {
        public static BehaviorFactory Factory(
            Dictionary<string, Type>? requiredDependencies = null)
        {
            ConstructorInfo? constructor = typeof(TestBehavior)
               .GetConstructor(Array.Empty<Type>());
            if (constructor == null)
                throw new NullReferenceException();

             return new(constructor, requiredDependencies ?? new());
        }
    }

    #region Shared Constructs
    public class TestBehavior { }

    public class TestContextA { }

    public class TestContextB { }
    #endregion

    public class AddDependency
    {
        [Test]
        public void IgnoresNonDependency()
        {
            BehaviorFactory factory = SetUp.Factory(new()
            {
                { "A", typeof(TestContextA) }
            });

            factory.AddAvailableDependency(new TestContextB());

            Assert.AreEqual(0, factory.NumberOfPendingInstantiations);
        }

        [Test]
        public void MultipleDependencies_CompleteFulfillment_DeterminesInstantiationIsPossible()
        {
            Assert.Ignore();
        }

        [Test]
        public void MultipleDependencies_CompleteFulfillment_UpdatesPendingInstantiations()
        {
            Assert.Ignore();
        }

        [Test]
        public void MultipleDependencies_PartialFulfillment_DeterminesInstantiationIsNotPossible()
        {
            Assert.Ignore();
        }

        [Test]
        public void MultipleDependencies_PartialFulfillment_DoesNotUpdatePendingInstantiations()
        {
            Assert.Ignore();
        }

        [Test]
        public void MultipleSameTypeDependencies_DeterminesInstantiationIsPossible()
        {
            Assert.Ignore();
        }

        [Test]
        public void ProvidedNullDependencyThrowsException()
        {
            BehaviorFactory factory = SetUp.Factory(new()
            {
                { "contextA", typeof(TestContextA) }
            });

#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            Assert.Throws<ArgumentNullException>(() => factory.AddAvailableDependency(null));
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
        }

        [Test]
        public void SingleDependency_DeterminesInstantiationIsPossible()
        {
            BehaviorFactory factory = SetUp.Factory(new()
            {
                { "contextA", typeof(TestContextA) }
            });

            Assert.IsTrue(factory.AddAvailableDependency(new TestContextA()));
        }

        [Test]
        public void SingleDependency_UpdatesPendingInstantiations()
        {
            BehaviorFactory factory = SetUp.Factory(new()
            {
                { "contextA", typeof(TestContextA) }
            });

            factory.AddAvailableDependency(new TestContextA());

            Assert.AreEqual(1, factory.NumberOfPendingInstantiations);
        }
    }

    public class CanInstantiate
    {
        [Test]
        public void AtLeastOnePendingInstantiation()
        {
            BehaviorFactory factory = SetUp.Factory(new()
            {
                { "contextA", typeof(TestContextA) }
            });

            factory.AddAvailableDependency(new TestContextA());

            Assert.IsTrue(factory.CanInstantiate);
        }

        [Test]
        public void InfinitePendingInstantiations()
        {
            BehaviorFactory factory = SetUp.Factory();

            Assert.IsTrue(factory.CanInstantiate);
        }

        [Test]
        public void NoPendingInstantiations()
        {
            BehaviorFactory factory = SetUp.Factory(new()
            {
                { "contextA", typeof(TestContextA) }
            });

            Assert.IsFalse(factory.CanInstantiate);
        }
    }

    public class Construction
    {
        [Test]
        public void DeterminesInfinitePendingInstantiations()
        {
            BehaviorFactory factory = SetUp.Factory();

            Assert.AreEqual(-1, factory.NumberOfPendingInstantiations);
        }

        [Test]
        public void DeterminesNoPendingInstantiations()
        {
            BehaviorFactory factory = SetUp.Factory(new()
            {
                { "A", typeof(TestContextA) }
            });

            Assert.AreEqual(0, factory.NumberOfPendingInstantiations);
        }

        [Test]
        public void NullConstructorThrowsException()
        {
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            Assert.Throws<ArgumentNullException>(() => new BehaviorFactory(null, new()));
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
        }

        [Test]
        public void SetsRequiredDependencies_DuplicateDependencyTypes()
        {
            BehaviorFactory factory = SetUp.Factory(new()
            {
                { "A1", typeof(TestContextA) },
                { "A2", typeof(TestContextA) }
            });

            Assert.AreEqual(1, factory.RequiredDependencyTypes.Length);
            Assert.Contains(typeof(TestContextA), factory.RequiredDependencyTypes);
        }

        [Test]
        public void SetsRequiredDependencies_MultipleDependencies()
        {
            BehaviorFactory factory = SetUp.Factory(new()
            {
                { "A", typeof(TestContextA) },
                { "B", typeof(TestContextB) }
            });

            Assert.AreEqual(2, factory.RequiredDependencyTypes.Length);
            Assert.Contains(typeof(TestContextA), factory.RequiredDependencyTypes);
            Assert.Contains(typeof(TestContextB), factory.RequiredDependencyTypes);
        }

        [Test]
        public void SetsRequiredDependencies_NoDependencies()
        {
            BehaviorFactory factory = SetUp.Factory();

            Assert.IsEmpty(factory.RequiredDependencyTypes);
        }
    }

    public class Process
    {
        [Test]
        public void ConstructorReturningNullDependencyThrowsException()
        {
            Assert.Ignore();
        }

        [Test]
        public void MultiplePendingInstantiation_InstantiatesMultiple()
        {
            Assert.Ignore();
        }

        [Test]
        public void MultiplePendingInstantiation_ResetsPendingInstantiations()
        {
            Assert.Ignore();
        }

        [Test]
        public void NoPendingInstantiations_InstantiatesNone()
        {
            Assert.Ignore();
        }

        [Test]
        public void NoPendingInstantiations_DoesNotUpdatePendingInstantiations()
        {
            Assert.Ignore();
        }

        [Test]
        public void OnePendingInstantiation_InstantiatesOne()
        {
            Assert.Ignore();
        }

        [Test]
        public void OnePendingInstantiation_ResetsPendingInstantiations()
        {
            Assert.Ignore();
        }

        [Test]
        public void SelfFulfilledBehavior_InstantiatesOne()
        {
            Assert.Ignore();
        }

        [Test]
        public void SelfFulfilledBehavior_DoesNotUpdatePendingInstantiations()
        {
            Assert.Ignore();
        }
    }
}
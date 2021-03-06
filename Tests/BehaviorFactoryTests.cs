using ContextualProgramming.Internal;
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

        public static BehaviorFactory SelfFulfilledFactory()
        {
            ConstructorInfo? constructor = typeof(TestBehaviorSelfFulfilled)
               .GetConstructors()[0];
            if (constructor == null)
                throw new NullReferenceException();

            return new(constructor, new());
        }
    }

    public static class Validate
    {
        public static void InstanceEquality<TExpectedBehavior>(BehaviorInstance instance,
            Type[] expectedSelfCreatedContexts,
            Dictionary<string, Type> expectedContexts)
        {
            Assert.AreEqual(typeof(TExpectedBehavior), instance.Behavior.GetType());

            Assert.AreEqual(expectedSelfCreatedContexts.Length,
                instance.SelfCreatedContexts.Length);
            for (int c = 0, count = expectedSelfCreatedContexts.Length; c < count; c++)
                Assert.AreEqual(expectedSelfCreatedContexts[c],
                    instance.SelfCreatedContexts[c].GetType());

            Assert.AreEqual(expectedContexts.Count, instance.Contexts.Count);
            foreach (string expectedContextName in expectedContexts.Keys)
            {
                Assert.IsTrue(instance.Contexts.ContainsKey(expectedContextName));
                Assert.AreEqual(expectedContexts[expectedContextName], instance
                    .Contexts[expectedContextName].GetType());
            }
        }
    }

    #region Shared Constructs
    public class TestBehavior { }

    public class TestBehaviorMixedDependencies
    {
        public TestContextA ContextA { get; set; }

        public TestBehaviorMixedDependencies(TestContextA contextA, out TestContextB contextB)
        {
            ContextA = contextA;
            contextB = new();
        }
    }
    public class TestBehaviorNullDependency
    {
        public TestBehaviorNullDependency( out TestContextA? contextA) => contextA = null;
    }
    public class TestBehaviorRequiresExisting
    {
        public TestContextA ContextA { get; set; }

        public TestBehaviorRequiresExisting(TestContextA contextA) => ContextA = contextA;
    }
    public class TestBehaviorSelfFulfilled
    {
        public TestBehaviorSelfFulfilled(out TestContextA contextA) => contextA = new();
    }

    public class TestContextA { }

    public class TestContextB { }
    #endregion

    public class AddDependency
    {
        [Test]
        public void AlreadyAddedDependency_ReturnsPreviousReturn()
        {
            BehaviorFactory factory = SetUp.Factory(new()
            {
                { "contextA", typeof(TestContextA) },
                { "contextB", typeof(TestContextB) }
            });

            TestContextA contextA = new();
            TestContextB contextB = new();

            bool canInstantiate = factory.AddAvailableDependency(contextA);
            Assert.AreEqual(canInstantiate, factory.AddAvailableDependency(contextA));

            canInstantiate = factory.AddAvailableDependency(contextB);
            Assert.AreEqual(canInstantiate, factory.AddAvailableDependency(contextB));
        }

        [Test]
        public void MultipleDependencies_Fulfillment_ReturnsTrue()
        {
            BehaviorFactory factory = SetUp.Factory(new()
            {
                { "contextA", typeof(TestContextA) },
                { "contextB", typeof(TestContextB) }
            });

            factory.AddAvailableDependency(new TestContextA());
            Assert.IsTrue(factory.AddAvailableDependency(new TestContextB()));
        }

        [Test]
        public void MultipleDependencies_PartialDuplicateFulfillment_ReturnsFalse()
        {
            BehaviorFactory factory = SetUp.Factory(new()
            {
                { "contextA", typeof(TestContextA) },
                { "contextB", typeof(TestContextB) }
            });

            factory.AddAvailableDependency(new TestContextA());
            Assert.IsFalse(factory.AddAvailableDependency(new TestContextA()));
        }

        [Test]
        public void MultipleDependencies_PartialFulfillment_ReturnsFalse()
        {
            BehaviorFactory factory = SetUp.Factory(new()
            {
                { "contextA", typeof(TestContextA) },
                { "contextB", typeof(TestContextB) }
            });

            Assert.IsFalse(factory.AddAvailableDependency(new TestContextA()));
        }

        [Test]
        public void MultipleSameTypeDependencies_Fulfillment_ReturnsTrue()
        {
            BehaviorFactory factory = SetUp.Factory(new()
            {
                { "contextA1", typeof(TestContextA) },
                { "contextA2", typeof(TestContextA) }
            });

            factory.AddAvailableDependency(new TestContextA());
            Assert.IsTrue(factory.AddAvailableDependency(new TestContextA()));
        }

        [Test]
        public void MultipleSameTypeDependencies_PartialFulfillment_ReturnsFalse()
        {
            BehaviorFactory factory = SetUp.Factory(new()
            {
                { "contextA1", typeof(TestContextA) },
                { "contextA2", typeof(TestContextA) }
            });

            Assert.IsFalse(factory.AddAvailableDependency(new TestContextA()));
        }

        [Test]
        public void NonDependency_ReturnsCanInstantiate()
        {
            BehaviorFactory factory = SetUp.Factory(new()
            {
                { "A", typeof(TestContextA) }
            });

            bool expected = factory.CanInstantiate;
            Assert.AreEqual(expected, factory.AddAvailableDependency(new TestContextB()));

            factory.AddAvailableDependency(new TestContextA());

            expected = factory.CanInstantiate;
            Assert.AreEqual(expected, factory.AddAvailableDependency(new TestContextB()));
        }

        [Test]
        public void NullDependency_ThrowsException()
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
        public void SingleDependency_Fulfillment_ReturnsTrue()
        {
            BehaviorFactory factory = SetUp.Factory(new()
            {
                { "contextA", typeof(TestContextA) }
            });

            Assert.IsTrue(factory.AddAvailableDependency(new TestContextA()));
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
        public void LosesPendingInstantiation()
        {
            BehaviorFactory factory = SetUp.Factory(new()
            {
                { "contextA", typeof(TestContextA) }
            });

            TestContextA contextA = new();
            factory.AddAvailableDependency(contextA);
            factory.RemoveAvailableDependency(contextA);

            Assert.IsFalse(factory.CanInstantiate);
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
        public void NullConstructor_ThrowsException()
        {
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            Assert.Throws<ArgumentNullException>(() => new BehaviorFactory(null, new()));
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
        }

        [Test]
        public void SetsRequiredDependencies_DuplicateDependencyTypes_OneRequiredDependency()
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
        public void SetsRequiredDependencies_MultipleDependencies_MultipleRequiredDependencies()
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
        public void SetsRequiredDependencies_NoDependencies_NoRequiredDependencies()
        {
            BehaviorFactory factory = SetUp.Factory();

            Assert.IsEmpty(factory.RequiredDependencyTypes);
        }
    }

    public class NumberOfPendingInstantiations
    {
        [Test]
        public void AlreadyAddedDependency_UnchangedPendingInstantiations()
        {
            BehaviorFactory factory = SetUp.Factory(new()
            {
                { "contextA", typeof(TestContextA) }
            });

            TestContextA context = new();
            factory.AddAvailableDependency(context);

            int pendingInstantiations = factory.NumberOfPendingInstantiations;

            factory.AddAvailableDependency(context);

            Assert.AreEqual(pendingInstantiations, factory.NumberOfPendingInstantiations);
        }

        [Test]
        public void IndependentFactory_InfinitePendingInstantiations()
        {
            BehaviorFactory factory = SetUp.Factory();

            Assert.AreEqual(-1, factory.NumberOfPendingInstantiations);
        }

        [Test]
        public void MultiDependencyFactory_MultipleFulfillment_MultiplePendingInstantiations()
        {
            BehaviorFactory factory = SetUp.Factory(new()
            {
                { "contextA", typeof(TestContextA) },
                { "contextB", typeof(TestContextB) }
            });

            factory.AddAvailableDependency(new TestContextA());
            factory.AddAvailableDependency(new TestContextB());
            factory.AddAvailableDependency(new TestContextA());
            factory.AddAvailableDependency(new TestContextB());

            Assert.AreEqual(2, factory.NumberOfPendingInstantiations);
        }

        [Test]
        public void MultiDependencyFactory_MulitplePartialFulfillment_NoPendingInstantiations()
        {
            BehaviorFactory factory = SetUp.Factory(new()
            {
                { "contextA", typeof(TestContextA) },
                { "contextB", typeof(TestContextB) }
            });

            factory.AddAvailableDependency(new TestContextA());
            factory.AddAvailableDependency(new TestContextA());

            Assert.AreEqual(0, factory.NumberOfPendingInstantiations);
        }

        [Test]
        public void MultiDependencyFactory_PartialFulfillment_NoPendingInstantiations()
        {
            BehaviorFactory factory = SetUp.Factory(new()
            {
                { "contextA", typeof(TestContextA) },
                { "contextB", typeof(TestContextB) }
            });

            factory.AddAvailableDependency(new TestContextA());

            Assert.AreEqual(0, factory.NumberOfPendingInstantiations);
        }

        [Test]
        public void MultiDependencyFactory_PostConstruction_NoPendingInstantiations()
        {
            BehaviorFactory factory = SetUp.Factory(new()
            {
                { "contextA", typeof(TestContextA) },
                { "contextB", typeof(TestContextB) }
            });

            Assert.AreEqual(0, factory.NumberOfPendingInstantiations);
        }

        [Test]
        public void MultiDependencyFactory_SingleFulfillment_OnePendingInstantiation()
        {
            BehaviorFactory factory = SetUp.Factory(new()
            {
                { "contextA", typeof(TestContextA) },
                { "contextB", typeof(TestContextB) }
            });

            factory.AddAvailableDependency(new TestContextA());
            factory.AddAvailableDependency(new TestContextB());

            Assert.AreEqual(1, factory.NumberOfPendingInstantiations);
        }

        [Test]
        public void MultiDependencyFactory_SingleFulfillmentWithPartialFulfillment_OnePendingInstantiation()
        {
            BehaviorFactory factory = SetUp.Factory(new()
            {
                { "contextA", typeof(TestContextA) },
                { "contextB", typeof(TestContextB) }
            });

            factory.AddAvailableDependency(new TestContextA());
            factory.AddAvailableDependency(new TestContextB());
            factory.AddAvailableDependency(new TestContextB());

            Assert.AreEqual(1, factory.NumberOfPendingInstantiations);
        }

        [Test]
        public void MultiSameTypeDependencyFactory_MultipleFulfillment_MultiplePendingInstantiations()
        {
            BehaviorFactory factory = SetUp.Factory(new()
            {
                { "contextA1", typeof(TestContextA) },
                { "contextA2", typeof(TestContextA) }
            });

            factory.AddAvailableDependency(new TestContextA());
            factory.AddAvailableDependency(new TestContextA());
            factory.AddAvailableDependency(new TestContextA());
            factory.AddAvailableDependency(new TestContextA());

            Assert.AreEqual(2, factory.NumberOfPendingInstantiations);
        }

        [Test]
        public void MultiSameTypeDependencyFactory_PartialFulfillment_NoPendingInstantiations()
        {
            BehaviorFactory factory = SetUp.Factory(new()
            {
                { "contextA1", typeof(TestContextA) },
                { "contextA2", typeof(TestContextA) }
            });

            factory.AddAvailableDependency(new TestContextA());

            Assert.AreEqual(0, factory.NumberOfPendingInstantiations);
        }

        [Test]
        public void MultiSameTypeDependencyFactory_SingleFulfillment_OnePendingInstantiation()
        {
            BehaviorFactory factory = SetUp.Factory(new()
            {
                { "contextA1", typeof(TestContextA) },
                { "contextA2", typeof(TestContextA) }
            });

            factory.AddAvailableDependency(new TestContextA());
            factory.AddAvailableDependency(new TestContextA());

            Assert.AreEqual(1, factory.NumberOfPendingInstantiations);
        }

        [Test]
        public void MultiSameTypeDependencyFactory_SingleFulfillmentWithPartialFulfillment_OnePendingInstantiation()
        {
            BehaviorFactory factory = SetUp.Factory(new()
            {
                { "contextA1", typeof(TestContextA) },
                { "contextA2", typeof(TestContextA) }
            });

            factory.AddAvailableDependency(new TestContextA());
            factory.AddAvailableDependency(new TestContextA());
            factory.AddAvailableDependency(new TestContextA());

            Assert.AreEqual(1, factory.NumberOfPendingInstantiations);
        }

        [Test]
        public void SingleDependencyFactory_MultipleFulfillment_MultiplePendingInstantiations()
        {
            BehaviorFactory factory = SetUp.Factory(new()
            {
                { "contextA", typeof(TestContextA) }
            });

            factory.AddAvailableDependency(new TestContextA());
            factory.AddAvailableDependency(new TestContextA());

            Assert.AreEqual(2, factory.NumberOfPendingInstantiations);
        }

        [Test]
        public void SingleDependencyFactory_PostConstruction_NoPendingInstantiations()
        {
            BehaviorFactory factory = SetUp.Factory(new()
            {
                { "contextA", typeof(TestContextA) }
            });

            Assert.AreEqual(0, factory.NumberOfPendingInstantiations);
        }

        [Test]
        public void SingleDependencyFactory_SingleFulfillment_OnePendingInstantiation()
        {
            BehaviorFactory factory = SetUp.Factory(new()
            {
                { "contextA", typeof(TestContextA) }
            });

            factory.AddAvailableDependency(new TestContextA());

            Assert.AreEqual(1, factory.NumberOfPendingInstantiations);
        }
    }

    public class Process
    {
        [Test]
        public void BehaviorConstructorReturningNullDependency_ThrowsException()
        {
            ConstructorInfo? constructor = typeof(TestBehaviorNullDependency)
               .GetConstructors()[0];
            if (constructor == null)
                throw new NullReferenceException();

            BehaviorFactory factory = new(constructor, new());

            Assert.Throws<InvalidOperationException>(() => factory.Process());
        }

        [Test]
        public void MixedDependencyBehavior_ReceivesExistingAndInstantiatesSelfCreated()
        {
            string expectedContextNameA = "contextA";
            string expectedContextNameB = "contextB";

            ConstructorInfo? constructor = typeof(TestBehaviorMixedDependencies)
               .GetConstructors()[0];
            if (constructor == null)
                throw new NullReferenceException();

            BehaviorFactory factory = new(constructor, new()
            {
                { expectedContextNameA, typeof(TestContextA) }
            });

            TestContextA expectedExistingContextA = new();
            factory.AddAvailableDependency(expectedExistingContextA);

            BehaviorInstance[] instances = factory.Process();

            Assert.AreEqual(1, instances.Length);
            var instance = (TestBehaviorMixedDependencies)instances[0].Behavior;

            Assert.AreEqual(expectedExistingContextA, instance.ContextA);
            Validate.InstanceEquality<TestBehaviorMixedDependencies>(instances[0],
                new Type[] { typeof(TestContextB) }, new()
                {
                    { expectedContextNameA, typeof(TestContextA) },
                    { expectedContextNameB, typeof(TestContextB) }
                });
        }

        [Test]
        public void MultiplePendingInstantiations_InstantiatesAllWithDependencies()
        {
            string expectedContextName = "contextA";

            BehaviorFactory factory = SetUp.Factory(new()
            {
                { expectedContextName, typeof(TestContextA) }
            });

            factory.AddAvailableDependency(new TestContextA());
            factory.AddAvailableDependency(new TestContextA());
            factory.AddAvailableDependency(new TestContextA());

            BehaviorInstance[] instances = factory.Process();

            Assert.AreEqual(3, instances.Length);

            Validate.InstanceEquality<TestBehavior>(instances[0],
                Array.Empty<Type>(), new()
                {
                    { expectedContextName, typeof(TestContextA) }
                });
        }

        [Test]
        public void MultiplePendingInstantiations_ResetsPendingInstantiations()
        {
            BehaviorFactory factory = SetUp.Factory(new()
            {
                { "contextA", typeof(TestContextA) }
            });

            factory.AddAvailableDependency(new TestContextA());
            factory.AddAvailableDependency(new TestContextA());
            factory.AddAvailableDependency(new TestContextA());

            int pendingInstantiations = factory.NumberOfPendingInstantiations;

            factory.Process();

            Assert.AreNotEqual(pendingInstantiations, factory.NumberOfPendingInstantiations);
            Assert.AreEqual(0, factory.NumberOfPendingInstantiations);
        }

        [Test]
        public void NoPendingInstantiations_DoesNotUpdatePendingInstantiations()
        {
            BehaviorFactory factory = SetUp.Factory(new()
            {
                { "contextA", typeof(TestContextA) }
            });

            int pendingInstantiations = factory.NumberOfPendingInstantiations;

            factory.Process();

            Assert.AreEqual(pendingInstantiations, factory.NumberOfPendingInstantiations);
        }

        [Test]
        public void NoPendingInstantiations_InstantiatesNone()
        {
            BehaviorFactory factory = SetUp.Factory(new()
            {
                { "contextA", typeof(TestContextA) }
            });

            BehaviorInstance[] instances = factory.Process();

            Assert.IsEmpty(instances);
        }

        [Test]
        public void OnePendingInstantiation_InstantiatesOneWithDependencies()
        {
            string expectedContextNameA = "contextA";
            string expectedContextNameA2 = "contextA2";
            string expectedContextNameB = "contextB";

            ConstructorInfo? constructor = typeof(TestBehaviorSelfFulfilled)
               .GetConstructors()[0];
            if (constructor == null)
                throw new NullReferenceException();

            BehaviorFactory factory = new(constructor, new()
            {
                { expectedContextNameA2, typeof(TestContextA) },
                { expectedContextNameB, typeof(TestContextB) }
            });

            factory.AddAvailableDependency(new TestContextA());
            factory.AddAvailableDependency(new TestContextB());

            BehaviorInstance[] instances = factory.Process();

            Assert.AreEqual(1, instances.Length);
            Validate.InstanceEquality<TestBehaviorSelfFulfilled>(instances[0],
                new Type[] { typeof(TestContextA)}, new()
                {
                    { expectedContextNameA, typeof(TestContextA) },
                    { expectedContextNameA2, typeof(TestContextA) },
                    { expectedContextNameB, typeof(TestContextB) }
                });
        }

        [Test]
        public void OnePendingInstantiation_ResetsPendingInstantiations()
        {
            BehaviorFactory factory = SetUp.Factory(new()
            {
                { "contextA", typeof(TestContextA) }
            });

            factory.AddAvailableDependency(new TestContextA());

            int pendingInstantiations = factory.NumberOfPendingInstantiations;

            factory.Process();

            Assert.AreNotEqual(pendingInstantiations, factory.NumberOfPendingInstantiations);
            Assert.AreEqual(0, factory.NumberOfPendingInstantiations);
        }

        [Test]
        public void RemovedDependency_DoesNotUseRemovedDependencyInInstantiation()
        {
            string expectedContextName = "contextA";

            BehaviorFactory factory = SetUp.Factory(new()
            {
                { expectedContextName, typeof(TestContextA) }
            });

            TestContextA context1 = new();
            TestContextA context2 = new();
            factory.AddAvailableDependency(context1);
            factory.AddAvailableDependency(context2);

            factory.RemoveAvailableDependency(context2);

            BehaviorInstance[] instances = factory.Process();

            Assert.AreEqual(1, instances.Length);
            Assert.AreEqual(context1, instances[0].Contexts[expectedContextName]);
        }

        [Test]
        public void RequiresExistingBehavior_ReceivesExisting()
        {
            string expectedContextNameA = "contextA";

            ConstructorInfo? constructor = typeof(TestBehaviorRequiresExisting)
               .GetConstructors()[0];
            if (constructor == null)
                throw new NullReferenceException();

            BehaviorFactory factory = new(constructor, new()
            {
                { expectedContextNameA, typeof(TestContextA) }
            });

            TestContextA expectedExistingContextA = new();
            factory.AddAvailableDependency(expectedExistingContextA);

            BehaviorInstance[] instances = factory.Process();

            Assert.AreEqual(1, instances.Length);
            var instance = (TestBehaviorRequiresExisting)instances[0].Behavior;

            Assert.AreEqual(expectedExistingContextA, instance.ContextA);
            Validate.InstanceEquality<TestBehaviorRequiresExisting>(instances[0],
                Array.Empty<Type>(), new()
                {
                    { expectedContextNameA, typeof(TestContextA) }
                });
        }

        [Test]
        public void SelfFulfilledBehavior_InstantiatesOneWithSelfCreatedDependencies()
        {
            BehaviorFactory factory = SetUp.SelfFulfilledFactory();

            BehaviorInstance[] instances = factory.Process();

            Assert.AreEqual(1, instances.Length);
            Validate.InstanceEquality<TestBehaviorSelfFulfilled>(instances[0],
                new Type[] { typeof(TestContextA) },
                new() 
                {
                    { "contextA", typeof(TestContextA) }
                });
        }

        [Test]
        public void SelfFulfilledBehavior_DoesNotUpdatePendingInstantiations()
        {
            BehaviorFactory factory = SetUp.SelfFulfilledFactory();

            int pendingInstantiations = factory.NumberOfPendingInstantiations;

            factory.Process();

            Assert.AreEqual(pendingInstantiations, factory.NumberOfPendingInstantiations);
        }
    }

    public class RemoveDependency
    {
        [Test]
        public void MultipleDependencies_LosesFulfillment_ReturnsFalse()
        {
            BehaviorFactory factory = SetUp.Factory(new()
            {
                { "contextA", typeof(TestContextA) },
                { "contextB", typeof(TestContextB) }
            });

            TestContextA contextA = new();
            factory.AddAvailableDependency(contextA);
            factory.AddAvailableDependency(new TestContextB());

            Assert.IsFalse(factory.RemoveAvailableDependency(contextA));
        }

        [Test]
        public void MultipleDependencies_MaintainsFulfillment_ReturnsTrue()
        {
            BehaviorFactory factory = SetUp.Factory(new()
            {
                { "contextA", typeof(TestContextA) },
                { "contextB", typeof(TestContextB) }
            });

            TestContextA contextA = new();
            factory.AddAvailableDependency(contextA);
            factory.AddAvailableDependency(new TestContextA());
            factory.AddAvailableDependency(new TestContextB());

            Assert.IsTrue(factory.RemoveAvailableDependency(contextA));
        }

        [Test]
        public void MultipleDependencies_PartialDuplicateFulfillment_ReturnsFalse()
        {
            BehaviorFactory factory = SetUp.Factory(new()
            {
                { "contextA", typeof(TestContextA) },
                { "contextB", typeof(TestContextB) }
            });

            TestContextA contextA = new();
            factory.AddAvailableDependency(new TestContextA());
            factory.AddAvailableDependency(new TestContextA());
            factory.AddAvailableDependency(contextA);

            Assert.IsFalse(factory.RemoveAvailableDependency(contextA));
        }

        [Test]
        public void MultipleDependencies_PartialFulfillment_ReturnsFalse()
        {
            BehaviorFactory factory = SetUp.Factory(new()
            {
                { "contextA", typeof(TestContextA) },
                { "contextB", typeof(TestContextB) }
            });

            TestContextA contextA = new();
            factory.AddAvailableDependency(new TestContextB());
            factory.AddAvailableDependency(contextA);

            Assert.IsFalse(factory.RemoveAvailableDependency(contextA));
        }

        [Test]
        public void MultipleSameTypeDependencies_LosesFulfillment_ReturnsFalse()
        {
            BehaviorFactory factory = SetUp.Factory(new()
            {
                { "contextA1", typeof(TestContextA) },
                { "contextA2", typeof(TestContextA) }
            });

            TestContextA contextA = new();
            factory.AddAvailableDependency(contextA);
            factory.AddAvailableDependency(new TestContextA());

            Assert.IsFalse(factory.RemoveAvailableDependency(contextA));

            // Rule out order of dependencies.

            factory = SetUp.Factory(new()
            {
                { "contextA1", typeof(TestContextA) },
                { "contextA2", typeof(TestContextA) }
            });

            factory.AddAvailableDependency(new TestContextA());
            factory.AddAvailableDependency(contextA);

            Assert.IsFalse(factory.RemoveAvailableDependency(contextA));
        }

        [Test]
        public void MultipleSameTypeDependencies_MaintainsFulfillment_ReturnsTrue()
        {
            BehaviorFactory factory = SetUp.Factory(new()
            {
                { "contextA1", typeof(TestContextA) },
                { "contextA2", typeof(TestContextA) }
            });

            TestContextA contextA = new();
            factory.AddAvailableDependency(new TestContextA());
            factory.AddAvailableDependency(contextA);
            factory.AddAvailableDependency(new TestContextA());

            Assert.IsTrue(factory.RemoveAvailableDependency(contextA));

            // Rule out order of dependencies.
            factory = SetUp.Factory(new()
            {
                { "contextA1", typeof(TestContextA) },
                { "contextA2", typeof(TestContextA) }
            });

            factory.AddAvailableDependency(new TestContextA());
            factory.AddAvailableDependency(new TestContextA());
            factory.AddAvailableDependency(contextA);

            Assert.IsTrue(factory.RemoveAvailableDependency(contextA));
        }

        [Test]
        public void NonDependency_ReturnsCanInstantiate()
        {
            BehaviorFactory factory = SetUp.Factory(new()
            {
                { "A", typeof(TestContextA) }
            });

            bool expected = factory.CanInstantiate;
            Assert.AreEqual(expected, factory.AddAvailableDependency(new TestContextB()));

            factory.RemoveAvailableDependency(new TestContextA());

            expected = factory.CanInstantiate;
            Assert.AreEqual(expected, factory.RemoveAvailableDependency(new TestContextB()));
        }

        [Test]
        public void NullDependency_ThrowsException()
        {
            BehaviorFactory factory = SetUp.Factory(new()
            {
                { "contextA", typeof(TestContextA) }
            });

#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            Assert.Throws<ArgumentNullException>(() => factory.RemoveAvailableDependency(null));
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
        }

        [Test]
        public void SingleDependency_LosesFulfillment_ReturnsFalse()
        {
            BehaviorFactory factory = SetUp.Factory(new()
            {
                { "contextA", typeof(TestContextA) }
            });

            TestContextA contextA = new();
            factory.AddAvailableDependency(contextA);

            Assert.IsFalse(factory.RemoveAvailableDependency(contextA));
        }

        [Test]
        public void SingleDependency_MaintainsFulfillment_ReturnsTrue()
        {
            BehaviorFactory factory = SetUp.Factory(new()
            {
                { "contextA", typeof(TestContextA) }
            });

            TestContextA contextA = new();
            factory.AddAvailableDependency(contextA);
            factory.AddAvailableDependency(new TestContextA());

            Assert.IsTrue(factory.RemoveAvailableDependency(contextA));
        }

        [Test]
        public void UnaddedDependency_ReturnsCanInstantiate()
        {
            BehaviorFactory factory = SetUp.Factory(new()
            {
                { "contextA", typeof(TestContextA) }
            });

            TestContextA addedContextA = new();
            TestContextA unaddedContextA = new();

            bool canInstantiate = factory.CanInstantiate;
            Assert.AreEqual(canInstantiate, factory.RemoveAvailableDependency(unaddedContextA));

            factory.AddAvailableDependency(addedContextA);

            canInstantiate = factory.CanInstantiate;
            Assert.AreEqual(canInstantiate, factory.AddAvailableDependency(unaddedContextA));
        }
    }
}
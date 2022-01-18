using ContextualProgramming.Internal;
using NSubstitute;
using NUnit.Framework;
using System.Reflection;

namespace BehaviorFactoryTests
{
    // TODO :: Do factory and instance tests.
    // TODO :: Evaluator provides factories to app, update evaluator tests.
    // TOOD :: Update app tests to substitute factories as needed.
    // TODO :: Get back to actual functionality for the task.

    public class AddDependency
    {
        [Test]
        public void DeterminesInstantiationIsNotPossible()
        {
            Assert.Ignore();
        }

        [Test]
        public void DeterminesInstantiationIsPossible()
        {
            Assert.Ignore();
        }

        [Test]
        public void IgnoresNonDependency()
        {
            Assert.Ignore();
        }

        [Test]
        public void UpdatesPendingInstantiations()
        {
            Assert.Ignore();
        }
    }

    public class Construction
    {
        public class TestBehavior { }

        public class TestContextA { }

        public class TestContextB { }

        [Test]
        public void DeterminesInfinitePendingInstantiations()
        {
            ConstructorInfo? constructor = typeof(TestBehavior)
               .GetConstructor(Array.Empty<Type>());
            if (constructor == null)
                throw new NullReferenceException();

            BehaviorFactory factory = new(constructor, new());

            Assert.AreEqual(-1, factory.NumberOfPendingInstantiations);
        }

        [Test]
        public void DeterminesNoPendingInstantiations()
        {
            Assert.Ignore();
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
            ConstructorInfo? constructor = typeof(TestBehavior)
               .GetConstructor(Array.Empty<Type>());
            if (constructor == null)
                throw new NullReferenceException();

            BehaviorFactory factory = new(constructor, new Dictionary<string, Type>
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
            ConstructorInfo? constructor = typeof(TestBehavior)
               .GetConstructor(Array.Empty<Type>());
            if (constructor == null)
                throw new NullReferenceException();

            BehaviorFactory factory = new(constructor, new Dictionary<string, Type>
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
            ConstructorInfo? constructor = typeof(TestBehavior)
               .GetConstructor(Array.Empty<Type>());
            if (constructor == null)
                throw new NullReferenceException();

            BehaviorFactory factory = new(constructor, new());

            Assert.IsEmpty(factory.RequiredDependencyTypes);
        }
    }

    public class Process
    {
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
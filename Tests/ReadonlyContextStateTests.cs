using ContextualProgramming.Internal;
using NUnit.Framework;

namespace ReadonlyContextStateTests
{
    public class Construction
    {
        [Test]
        public void ImplicitInt()
        {
            int value = 10;
            ContextState<int> contextState = value;

            int directResult = contextState.Value;
            int implicitResult = contextState;

            Assert.AreEqual(value, directResult);
            Assert.AreEqual(value, implicitResult);
        }

        [Test]
        public void ImplicitString()
        {
            string value = "Test";
            ContextState<string> contextState = value;

            string? directResult = contextState.Value;
            string? implicitResult = contextState;

            Assert.AreEqual(value, directResult);
            Assert.AreEqual(value, implicitResult);
        }

        [Test]
        public void NewInt()
        {
            int value = 10;
            ContextState<int> contextState = new(value);

            int directResult = contextState.Value;
            int implicitResult = contextState;

            Assert.AreEqual(value, directResult);
            Assert.AreEqual(value, implicitResult);
        }

        [Test]
        public void NewString()
        {
            string value = "Test";
            ContextState<string> contextState = new(value);

            string? directResult = contextState.Value;
            string? implicitResult = contextState;

            Assert.AreEqual(value, directResult);
            Assert.AreEqual(value, implicitResult);
        }
    }

    public class Equality
    {
        [Test]
        public void EqualOperator_ContextState()
        {
            int value = 10;
            ContextState<int> a = new(value);
            ContextState<int> b = new(value);

            Assert.IsTrue(a == b);
            Assert.IsTrue(b == a);
        }

        [Test]
        public void EqualOperator_Int()
        {
            int value = 10;
            ContextState<int> contextState = value;

            Assert.IsTrue(value == contextState);
            Assert.IsTrue(contextState == value);
        }

        [Test]
        public void EqualOperator_Null()
        {
            ContextState<string>? contextState = null;

            Assert.IsTrue(null == contextState);
            Assert.IsTrue(contextState == null);
        }

        [Test]
        public void Equals_ContextState()
        {
            int value = 10;
            ContextState<int> a = new(value);
            ContextState<int> b = new(value);
            ContextState<int> c = new(11);

            Assert.IsTrue(a.Equals(b));
            Assert.IsTrue(b.Equals(a));

            Assert.IsFalse(a.Equals(c));
            Assert.IsFalse(c.Equals(a));
        }

        [Test]
        public void Equals_DifferentTypes()
        {
            int value = 10;
            ContextState<int> a = new(value);
            string b = "Test";

            Assert.IsFalse(a.Equals(b));
            Assert.IsFalse(b.Equals(a));
        }

        [Test]
        public void Equals_Int()
        {
            int value = 10;
            int comparedValue = 11;
            ContextState<int> contextState = value;

            Assert.IsTrue(contextState.Equals(value));
            Assert.IsFalse(contextState.Equals(comparedValue));

            Assert.IsTrue(value.Equals(contextState));
            Assert.IsFalse(comparedValue.Equals(contextState));
        }
        [Test]
        public void Equals_Null()
        {
            ContextState<int> contextState = 10;

            Assert.IsFalse(contextState.Equals(null));
        }

        [Test]
        public void InequalOperator_ToContextState()
        {
            int value = 10;
            ContextState<int> a = new(value);
            ContextState<int> b = new(11);

            Assert.IsTrue(a != b);
            Assert.IsTrue(b != a);
        }

        [Test]
        public void InequalOperator_ToInt()
        {
            int value = 10;
            ContextState<int> contextState = value;

            Assert.IsTrue(11 != contextState);
            Assert.IsTrue(contextState != 11);
        }

        [Test]
        public void InequalOperator_ToNull()
        {
            string value = "Test";
            ContextState<string> contextState = value;

            Assert.IsTrue(null != contextState);
            Assert.IsTrue(contextState != null);
        }
    }

    public class GetHashCode
    {
        [Test]
        public void NonNullValue()
        {
            int value = 10;
            ContextState<int> contextState = value;

            Assert.AreEqual(value.GetHashCode(), contextState.GetHashCode());
        }

        [Test]
        public void NullValue()
        {
            ContextState<string> contextState = new(null);

            Assert.AreEqual(0, contextState.GetHashCode());
        }
    }

    public class ToString
    {
        [Test]
        public void NonNullMatchesValueToString()
        {
            int value = 10;
            ContextState<int> contextState = value;

            Assert.AreEqual(10.ToString(), contextState.ToString());
        }

        [Test]
        public void NullProvidesEmptyString()
        {
            ContextState<string> contextState = new ContextState<string>(null);

            Assert.AreEqual(string.Empty, contextState.ToString());
        }
    }
}
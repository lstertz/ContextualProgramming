using NUnit.Framework;

namespace ReadonlyContextStateTests
{
    public class Construction
    {
        [Test]
        public void ImplicitInt()
        {
            int value = 10;
            ReadonlyContextState<int> ReadonlyContextState = value;

            int directResult = ReadonlyContextState.Value;
            int implicitResult = ReadonlyContextState;

            Assert.AreEqual(value, directResult);
            Assert.AreEqual(value, implicitResult);
        }

        [Test]
        public void ImplicitString()
        {
            string value = "Test";
            ReadonlyContextState<string> ReadonlyContextState = value;

            string? directResult = ReadonlyContextState.Value;
            string? implicitResult = ReadonlyContextState;

            Assert.AreEqual(value, directResult);
            Assert.AreEqual(value, implicitResult);
        }

        [Test]
        public void NewInt()
        {
            int value = 10;
            ReadonlyContextState<int> ReadonlyContextState = new(value);

            int directResult = ReadonlyContextState.Value;
            int implicitResult = ReadonlyContextState;

            Assert.AreEqual(value, directResult);
            Assert.AreEqual(value, implicitResult);
        }

        [Test]
        public void NewString()
        {
            string value = "Test";
            ReadonlyContextState<string> ReadonlyContextState = new(value);

            string? directResult = ReadonlyContextState.Value;
            string? implicitResult = ReadonlyContextState;

            Assert.AreEqual(value, directResult);
            Assert.AreEqual(value, implicitResult);
        }
    }

    public class Equality
    {
        [Test]
        public void EqualOperator_ReadonlyContextState()
        {
            int value = 10;
            ReadonlyContextState<int> a = new(value);
            ReadonlyContextState<int> b = new(value);

            Assert.IsTrue(a == b);
            Assert.IsTrue(b == a);
        }

        [Test]
        public void EqualOperator_Int()
        {
            int value = 10;
            ReadonlyContextState<int> ReadonlyContextState = value;

            Assert.IsTrue(value == ReadonlyContextState);
            Assert.IsTrue(ReadonlyContextState == value);
        }

        [Test]
        public void EqualOperator_Null()
        {
            ReadonlyContextState<string>? ReadonlyContextState = null;

            Assert.IsTrue(null == ReadonlyContextState);
            Assert.IsTrue(ReadonlyContextState == null);
        }

        [Test]
        public void Equals_ReadonlyContextState()
        {
            int value = 10;
            ReadonlyContextState<int> a = new(value);
            ReadonlyContextState<int> b = new(value);
            ReadonlyContextState<int> c = new(11);

            Assert.IsTrue(a.Equals(b));
            Assert.IsTrue(b.Equals(a));

            Assert.IsFalse(a.Equals(c));
            Assert.IsFalse(c.Equals(a));
        }

        [Test]
        public void Equals_DifferentTypes()
        {
            int value = 10;
            ReadonlyContextState<int> a = new(value);
            string b = "Test";

            Assert.IsFalse(a.Equals(b));
            Assert.IsFalse(b.Equals(a));
        }

        [Test]
        public void Equals_Int()
        {
            int value = 10;
            int comparedValue = 11;
            ReadonlyContextState<int> ReadonlyContextState = value;

            Assert.IsTrue(ReadonlyContextState.Equals(value));
            Assert.IsFalse(ReadonlyContextState.Equals(comparedValue));

            Assert.IsTrue(value.Equals(ReadonlyContextState));
            Assert.IsFalse(comparedValue.Equals(ReadonlyContextState));
        }
        [Test]
        public void Equals_Null()
        {
            ReadonlyContextState<int> ReadonlyContextState = 10;

            Assert.IsFalse(ReadonlyContextState.Equals(null));
        }

        [Test]
        public void InequalOperator_ToReadonlyContextState()
        {
            int value = 10;
            ReadonlyContextState<int> a = new(value);
            ReadonlyContextState<int> b = new(11);

            Assert.IsTrue(a != b);
            Assert.IsTrue(b != a);
        }

        [Test]
        public void InequalOperator_ToInt()
        {
            int value = 10;
            ReadonlyContextState<int> ReadonlyContextState = value;

            Assert.IsTrue(11 != ReadonlyContextState);
            Assert.IsTrue(ReadonlyContextState != 11);
        }

        [Test]
        public void InequalOperator_ToNull()
        {
            string value = "Test";
            ReadonlyContextState<string> ReadonlyContextState = value;

            Assert.IsTrue(null != ReadonlyContextState);
            Assert.IsTrue(ReadonlyContextState != null);
        }
    }

    public class ToString
    {
        [Test]
        public void NonNullMatchesValueToString()
        {
            int value = 10;
            ReadonlyContextState<int> ReadonlyContextState = value;

            Assert.AreEqual(10.ToString(), ReadonlyContextState.ToString());
        }

        [Test]
        public void NullProvidesEmptyString()
        {
            ReadonlyContextState<string> ReadonlyContextState = new ReadonlyContextState<string>(null);

            Assert.AreEqual(string.Empty, ReadonlyContextState.ToString());
        }
    }
}
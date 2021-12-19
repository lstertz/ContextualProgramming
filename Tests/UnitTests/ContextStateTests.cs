using ContextualProgramming.Internal;
using NUnit.Framework;

namespace Tests
{
    public class ContextStateTests
    {
        #region Construction
        [Test]
        public void Construct_ImplicitInt()
        {
            int value = 10;
            ContextState<int> contextState = value;

            int directResult = contextState.Value;
            int implicitResult = contextState;

            Assert.AreEqual(value, directResult);
            Assert.AreEqual(value, implicitResult);
        }

        [Test]
        public void Construct_ImplicitString()
        {
            string value = "Test";
            ContextState<string> contextState = value;

            string? directResult = contextState.Value;
            string? implicitResult = contextState;

            Assert.AreEqual(value, directResult);
            Assert.AreEqual(value, implicitResult);
        }

        [Test]
        public void Construct_NewInt()
        {
            int value = 10;
            ContextState<int> contextState = new(value);

            int directResult = contextState.Value;
            int implicitResult = contextState;

            Assert.AreEqual(value, directResult);
            Assert.AreEqual(value, implicitResult);
        }

        [Test]
        public void Construct_NewString()
        {
            string value = "Test";
            ContextState<string> contextState = new(value);

            string? directResult = contextState.Value;
            string? implicitResult = contextState;

            Assert.AreEqual(value, directResult);
            Assert.AreEqual(value, implicitResult);
        }
        #endregion

        #region Equality
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
            ContextState<int> contextState = value;

            Assert.IsTrue(contextState.Equals(value));
            Assert.IsFalse(contextState.Equals(11));

            Assert.IsTrue(value.Equals(contextState));
            Assert.IsFalse(11.Equals(contextState));
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
        #endregion

        #region Value Setting
        [Test]
        public void Set_ValueSets()
        {
            int value = 10;
            ContextState<int> contextState = value;

            int newValue = 11;
            contextState.Value = newValue;

            int directResult = contextState.Value;
            int implicitResult = contextState;

            Assert.AreEqual(newValue, directResult);
            Assert.AreEqual(newValue, implicitResult);
        }

        [Test]
        public void Set_ValueChangeWillNotify()
        {
            bool wasNotified = false;

            ContextState<int> contextState = 10;
            (contextState as IBindableState)?.Bind(() => wasNotified = true);

            contextState.Value = 11;

            Assert.IsTrue(wasNotified);
        }

        [Test]
        public void Set_ValueChangeWillNotify_FromNull()
        {
            bool wasNotified = false;

            ContextState<string> contextState = new(null);
            (contextState as IBindableState)?.Bind(() => wasNotified = true);

            contextState.Value = "Test";

            Assert.IsTrue(wasNotified);
        }

        [Test]
        public void Set_ValueChangeWillNotify_ToNull()
        {
            bool wasNotified = false;

            ContextState<string> contextState = new("Test");
            (contextState as IBindableState)?.Bind(() => wasNotified = true);

            contextState.Value = null;

            Assert.IsTrue(wasNotified);
        }

        [Test]
        public void Set_ValueUnchangedDoesNotNotify()
        {
            bool wasNotified = false;

            int value = 10;
            ContextState<int> contextState = value;
            (contextState as IBindableState)?.Bind(() => wasNotified = true);

            contextState.Value = value;

            Assert.IsFalse(wasNotified);
        }
        #endregion

        #region GetHashCode
        [Test]
        public void GetHashCode_NonNullValue()
        {
            int value = 10;
            ContextState<int> contextState = value;

            Assert.AreEqual(value.GetHashCode(), contextState.GetHashCode());
        }

        [Test]
        public void GetHashCode_NullValue()
        {
            ContextState<string> contextState = new(null);

            Assert.AreEqual(0, contextState.GetHashCode());
        }
        #endregion
    }
}
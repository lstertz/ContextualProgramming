using ContextualProgramming.Internal;
using NUnit.Framework;

namespace Tests
{
    public class ContextStateTests
    {
        #region Binding
        [Test]
        public void Binding_BindStateToNull_ThrowsException()
        {
            ContextState<int> contextState = 10;
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            Assert.Throws<ArgumentNullException>(() =>
                (contextState as IBindableState)?.Bind(null));
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
        }

        [Test]
        public void Binding_BoundState_ValueChangeWillNotify()
        {
            bool wasNotified = false;

            ContextState<int> contextState = 10;
            (contextState as IBindableState)?.Bind(() => wasNotified = true);

            contextState.Value = 11;

            Assert.IsTrue(wasNotified);
        }

        [Test]
        public void Binding_BoundState_ValueChangeWillNotify_FromNull()
        {
            bool wasNotified = false;

            ContextState<string> contextState = new(null);
            (contextState as IBindableState)?.Bind(() => wasNotified = true);

            contextState.Value = "Test";

            Assert.IsTrue(wasNotified);
        }

        [Test]
        public void Binding_BoundState_ValueChangeWillNotify_ToNull()
        {
            bool wasNotified = false;

            ContextState<string> contextState = new("Test");
            (contextState as IBindableState)?.Bind(() => wasNotified = true);

            contextState.Value = null;

            Assert.IsTrue(wasNotified);
        }

        [Test]
        public void Binding_BoundState_ValueUnchangedDoesNotNotify()
        {
            bool wasNotified = false;

            int value = 10;
            ContextState<int> contextState = value;
            (contextState as IBindableState)?.Bind(() => wasNotified = true);

            contextState.Value = value;

            Assert.IsFalse(wasNotified);
        }

        [Test]
        public void Binding_ReboundState_ValueChangeWillNotify()
        {
            bool wasNotified = false;

            int value = 10;
            ContextState<int> contextState = value;
            (contextState as IBindableState)?.Bind(() => wasNotified = false);
            (contextState as IBindableState)?.Bind(() => wasNotified = true);

            contextState.Value = 11;

            Assert.IsTrue(wasNotified);
        }

        [Test]
        public void Binding_UnboundState_ValueChangeDoesNotNotify()
        {
            bool wasNotified = false;

            int value = 10;
            ContextState<int> contextState = value;
            (contextState as IBindableState)?.Bind(() => wasNotified = true);
            (contextState as IBindableState)?.Unbind();

            contextState.Value = 11;

            Assert.IsFalse(wasNotified);
        }
        #endregion

        #region Construction
        [Test]
        public void Construction_ImplicitInt()
        {
            int value = 10;
            ContextState<int> contextState = value;

            int directResult = contextState.Value;
            int implicitResult = contextState;

            Assert.AreEqual(value, directResult);
            Assert.AreEqual(value, implicitResult);
        }

        [Test]
        public void Construction_ImplicitString()
        {
            string value = "Test";
            ContextState<string> contextState = value;

            string? directResult = contextState.Value;
            string? implicitResult = contextState;

            Assert.AreEqual(value, directResult);
            Assert.AreEqual(value, implicitResult);
        }

        [Test]
        public void Construction_NewInt()
        {
            int value = 10;
            ContextState<int> contextState = new(value);

            int directResult = contextState.Value;
            int implicitResult = contextState;

            Assert.AreEqual(value, directResult);
            Assert.AreEqual(value, implicitResult);
        }

        [Test]
        public void Construction_NewString()
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
        public void Equality_EqualOperator_ContextState()
        {
            int value = 10;
            ContextState<int> a = new(value);
            ContextState<int> b = new(value);

            Assert.IsTrue(a == b);
            Assert.IsTrue(b == a);
        }

        [Test]
        public void Equality_EqualOperator_Int()
        {
            int value = 10;
            ContextState<int> contextState = value;

            Assert.IsTrue(value == contextState);
            Assert.IsTrue(contextState == value);
        }

        [Test]
        public void Equality_EqualOperator_Null()
        {
            ContextState<string>? contextState = null;

            Assert.IsTrue(null == contextState);
            Assert.IsTrue(contextState == null);
        }

        [Test]
        public void Equality_Equals_ContextState()
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
        public void Equality_Equals_DifferentTypes()
        {
            int value = 10;
            ContextState<int> a = new(value);
            string b = "Test";

            Assert.IsFalse(a.Equals(b));
            Assert.IsFalse(b.Equals(a));
        }

        [Test]
        public void Equality_Equals_Int()
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
        public void Equality_Equals_Null()
        {
            ContextState<int> contextState = 10;

            Assert.IsFalse(contextState.Equals(null));
        }

        [Test]
        public void Equality_InequalOperator_ToContextState()
        {
            int value = 10;
            ContextState<int> a = new(value);
            ContextState<int> b = new(11);

            Assert.IsTrue(a != b);
            Assert.IsTrue(b != a);
        }

        [Test]
        public void Equality_InequalOperator_ToInt()
        {
            int value = 10;
            ContextState<int> contextState = value;

            Assert.IsTrue(11 != contextState);
            Assert.IsTrue(contextState != 11);
        }

        [Test]
        public void Equality_InequalOperator_ToNull()
        {
            string value = "Test";
            ContextState<string> contextState = value;

            Assert.IsTrue(null != contextState);
            Assert.IsTrue(contextState != null);
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

        #region Value Setting
        [Test]
        public void ValueSetting_ValueSets()
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
        #endregion
    }
}
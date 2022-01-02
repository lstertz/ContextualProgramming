using ContextualProgramming.Internal;
using NUnit.Framework;

namespace Tests
{
    public class ContextStateListTests
    {
        #region Binding
        [Test]
        public void Binding_BindStateToNull_ThrowsException()
        {
            Assert.Ignore();

            ContextState<int> contextState = 10;
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            Assert.Throws<ArgumentNullException>(() =>
                (contextState as IBindableState)?.Bind(null));
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
        }

        [Test]
        public void Binding_BoundState_AddWillNotify()
        {
            Assert.Ignore();

            bool wasNotified = false;

            ContextState<int> contextState = 10;
            (contextState as IBindableState)?.Bind(() => wasNotified = true);

            contextState.Value = 11;

            Assert.IsTrue(wasNotified);
        }

        [Test]
        public void Binding_BoundState_AddRangeWillNotify()
        {
            Assert.Ignore();

            bool wasNotified = false;

            ContextState<int> contextState = 10;
            (contextState as IBindableState)?.Bind(() => wasNotified = true);

            contextState.Value = 11;

            Assert.IsTrue(wasNotified);
        }

        [Test]
        public void Binding_BoundState_ClearWillNotify()
        {
            Assert.Ignore();

            bool wasNotified = false;

            ContextState<int> contextState = 10;
            (contextState as IBindableState)?.Bind(() => wasNotified = true);

            contextState.Value = 11;

            Assert.IsTrue(wasNotified);
        }

        [Test]
        public void Binding_BoundState_ElementChangeWillNotify()
        {
            Assert.Ignore();

            bool wasNotified = false;

            ContextState<int> contextState = 10;
            (contextState as IBindableState)?.Bind(() => wasNotified = true);

            contextState.Value = 11;

            Assert.IsTrue(wasNotified);
        }

        [Test]
        public void Binding_BoundState_ElementsChangeWillNotify()
        {
            Assert.Ignore();

            bool wasNotified = false;

            ContextState<int> contextState = 10;
            (contextState as IBindableState)?.Bind(() => wasNotified = true);

            contextState.Value = 11;

            Assert.IsTrue(wasNotified);
        }

        [Test]
        public void Binding_BoundState_ElementChangeWillNotify_FromNull()
        {
            Assert.Ignore();

            bool wasNotified = false;

            ContextState<string> contextState = new(null);
            (contextState as IBindableState)?.Bind(() => wasNotified = true);

            contextState.Value = "Test";

            Assert.IsTrue(wasNotified);
        }

        [Test]
        public void Binding_BoundState_ElementChangeWillNotify_ToNull()
        {
            Assert.Ignore();

            bool wasNotified = false;

            ContextState<string> contextState = new("Test");
            (contextState as IBindableState)?.Bind(() => wasNotified = true);

            contextState.Value = null;

            Assert.IsTrue(wasNotified);
        }

        [Test]
        public void Binding_BoundState_InsertWillNotify()
        {
            Assert.Ignore();

            bool wasNotified = false;

            ContextState<int> contextState = 10;
            (contextState as IBindableState)?.Bind(() => wasNotified = true);

            contextState.Value = 11;

            Assert.IsTrue(wasNotified);
        }

        [Test]
        public void Binding_BoundState_InsertRangeWillNotify()
        {
            Assert.Ignore();

            bool wasNotified = false;

            ContextState<int> contextState = 10;
            (contextState as IBindableState)?.Bind(() => wasNotified = true);

            contextState.Value = 11;

            Assert.IsTrue(wasNotified);
        }

        [Test]
        public void Binding_BoundState_ElementUnchangedDoesNotNotify()
        {
            Assert.Ignore();

            bool wasNotified = false;

            int value = 10;
            ContextState<int> contextState = value;
            (contextState as IBindableState)?.Bind(() => wasNotified = true);

            contextState.Value = value;

            Assert.IsFalse(wasNotified);
        }

        [Test]
        public void Binding_BoundState_RemoveWillNotify()
        {
            Assert.Ignore();

            bool wasNotified = false;

            ContextState<int> contextState = 10;
            (contextState as IBindableState)?.Bind(() => wasNotified = true);

            contextState.Value = 11;

            Assert.IsTrue(wasNotified);
        }

        [Test]
        public void Binding_BoundState_RemoveAtWillNotify()
        {
            Assert.Ignore();

            bool wasNotified = false;

            ContextState<int> contextState = 10;
            (contextState as IBindableState)?.Bind(() => wasNotified = true);

            contextState.Value = 11;

            Assert.IsTrue(wasNotified);
        }

        [Test]
        public void Binding_BoundState_SizeUnchangedDoesNotNotify()
        {
            Assert.Ignore();

            bool wasNotified = false;

            int value = 10;
            ContextState<int> contextState = value;
            (contextState as IBindableState)?.Bind(() => wasNotified = true);

            contextState.Value = value;

            Assert.IsFalse(wasNotified);
        }

        [Test]
        public void Binding_ReboundState_ChangeWillNotify()
        {
            Assert.Ignore();

            bool wasNotified = false;

            int value = 10;
            ContextState<int> contextState = value;
            (contextState as IBindableState)?.Bind(() => wasNotified = false);
            (contextState as IBindableState)?.Bind(() => wasNotified = true);

            contextState.Value = 11;

            Assert.IsTrue(wasNotified);
        }

        [Test]
        public void Binding_UnboundState_AddDoesNotNotify()
        {
            Assert.Ignore();

            bool wasNotified = false;

            int value = 10;
            ContextState<int> contextState = value;
            (contextState as IBindableState)?.Bind(() => wasNotified = true);
            (contextState as IBindableState)?.Unbind();

            contextState.Value = 11;

            Assert.IsFalse(wasNotified);
        }

        [Test]
        public void Binding_UnboundState_AddRangeDoesNotNotify()
        {
            Assert.Ignore();

            bool wasNotified = false;

            int value = 10;
            ContextState<int> contextState = value;
            (contextState as IBindableState)?.Bind(() => wasNotified = true);
            (contextState as IBindableState)?.Unbind();

            contextState.Value = 11;

            Assert.IsFalse(wasNotified);
        }

        [Test]
        public void Binding_UnboundState_ClearDoesNotNotify()
        {
            Assert.Ignore();

            bool wasNotified = false;

            int value = 10;
            ContextState<int> contextState = value;
            (contextState as IBindableState)?.Bind(() => wasNotified = true);
            (contextState as IBindableState)?.Unbind();

            contextState.Value = 11;

            Assert.IsFalse(wasNotified);
        }

        [Test]
        public void Binding_UnboundState_ElementChangeDoesNotNotify()
        {
            Assert.Ignore();

            bool wasNotified = false;

            int value = 10;
            ContextState<int> contextState = value;
            (contextState as IBindableState)?.Bind(() => wasNotified = true);
            (contextState as IBindableState)?.Unbind();

            contextState.Value = 11;

            Assert.IsFalse(wasNotified);
        }

        [Test]
        public void Binding_UnboundState_ElementsChangeDoesNotNotify()
        {
            Assert.Ignore();

            bool wasNotified = false;

            int value = 10;
            ContextState<int> contextState = value;
            (contextState as IBindableState)?.Bind(() => wasNotified = true);
            (contextState as IBindableState)?.Unbind();

            contextState.Value = 11;

            Assert.IsFalse(wasNotified);
        }

        [Test]
        public void Binding_UnboundState_InsertDoesNotNotify()
        {
            Assert.Ignore();

            bool wasNotified = false;

            int value = 10;
            ContextState<int> contextState = value;
            (contextState as IBindableState)?.Bind(() => wasNotified = true);
            (contextState as IBindableState)?.Unbind();

            contextState.Value = 11;

            Assert.IsFalse(wasNotified);
        }

        [Test]
        public void Binding_UnboundState_InsertRangeDoesNotNotify()
        {
            Assert.Ignore();

            bool wasNotified = false;

            int value = 10;
            ContextState<int> contextState = value;
            (contextState as IBindableState)?.Bind(() => wasNotified = true);
            (contextState as IBindableState)?.Unbind();

            contextState.Value = 11;

            Assert.IsFalse(wasNotified);
        }

        [Test]
        public void Binding_UnboundState_RemoveDoesNotNotify()
        {
            Assert.Ignore();

            bool wasNotified = false;

            int value = 10;
            ContextState<int> contextState = value;
            (contextState as IBindableState)?.Bind(() => wasNotified = true);
            (contextState as IBindableState)?.Unbind();

            contextState.Value = 11;

            Assert.IsFalse(wasNotified);
        }

        [Test]
        public void Binding_UnboundState_RemoveAtDoesNotNotify()
        {
            Assert.Ignore();

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
        public void Construction_ImplicitArray()
        {
            int[] values = { 10, 11 };
            ContextStateList<int> contextStateList = values;

            int[]? directResult = contextStateList.Elements;
            int[]? implicitResult = contextStateList;

            Assert.AreEqual(values, directResult);
            Assert.AreEqual(values, implicitResult);
        }

        [Test]
        public void Construction_NewArray()
        {
            int[] values = { 10, 11 };
            ContextStateList<int> contextStateList = new(values);

            int[]? directResult = contextStateList.Elements;
            int[]? implicitResult = contextStateList;

            Assert.AreEqual(values, directResult);
            Assert.AreEqual(values, implicitResult);
        }
        #endregion

        #region Equality
        [Test]
        public void Equality_EqualOperator_Array()
        {
            int[] values = { 10, 11 };
            ContextStateList<int> contextStateList = values;

            Assert.IsTrue(values == contextStateList);
            Assert.IsTrue(contextStateList == values);
        }

        [Test]
        public void Equality_EqualOperator_ContextStateList()
        {
            int[] values = { 10, 11 };
            ContextStateList<int> a = values;
            ContextStateList<int> b = values;

            Assert.IsTrue(a == b);
            Assert.IsTrue(b == a);
        }

        [Test]
        public void Equality_EqualOperator_Null()
        {
            ContextStateList<int>? contextStateList = null;

            Assert.IsTrue(null == contextStateList);
            Assert.IsTrue(contextStateList == null);
        }

        [Test]
        public void Equality_Equals_Array()
        {
            int[] values = { 10, 11 };
            int[] comparedValues = { 11, 12 };
            ContextStateList<int> contextStateList = values;

            Assert.IsTrue(contextStateList.Equals(values));
            Assert.IsFalse(contextStateList.Equals(comparedValues));
        }

        [Test]
        public void Equality_Equals_ContextStateList()
        {
            int[] values = { 10, 11 };
            int[] comparedValues = { 11, 12 };

            ContextStateList<int> a = new(values);
            ContextStateList<int> b = new(values);
            ContextStateList<int> c = new(comparedValues);

            Assert.IsTrue(a.Equals(b));
            Assert.IsTrue(b.Equals(a));

            Assert.IsFalse(a.Equals(c));
            Assert.IsFalse(c.Equals(a));
        }

        [Test]
        public void Equality_Equals_DifferentTypes()
        {
            int[] values = { 10, 11 };
            string[] comparedValues = { "a", "b" };

            ContextStateList<int> a = new(values);
            ContextStateList<string> b = new(comparedValues);

            Assert.IsFalse(a.Equals(b));
            Assert.IsFalse(b.Equals(a));
        }

        [Test]
        public void Equality_Equals_Null()
        {
            int[] values = { 10, 11 };
            ContextStateList<int> contextStateList = new(values);

            Assert.IsFalse(contextStateList.Equals(null));
        }

        [Test]
        public void Equality_InequalOperator_ToArray()
        {
            int[] values = { 10, 11 };
            int[] comparedValues = { 11, 12 };
            ContextStateList<int> contextStateList = new(values);

            Assert.IsTrue(comparedValues != contextStateList);
            Assert.IsTrue(contextStateList != comparedValues);
        }

        [Test]
        public void Equality_InequalOperator_ToContextState()
        {
            int[] values = { 10, 11 };
            ContextStateList<int> a = new(values);

            int[] comparedValues = { 11, 12 };
            ContextStateList<int> b = new(comparedValues);

            Assert.IsTrue(a != b);
            Assert.IsTrue(b != a);
        }

        [Test]
        public void Equality_InequalOperator_ToNull()
        {
            int[] values = { 10, 11 };
            ContextStateList<int> contextStateList = values;

            Assert.IsTrue(null != contextStateList);
            Assert.IsTrue(contextStateList != null);
        }
        #endregion

        #region List Operations
        [Test]
        public void ListOperations_Add_ElementIsAdded()
        {
            Assert.Ignore();

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
        public void ListOperations_AddRange_ElementsAreAdded()
        {
            Assert.Ignore();

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
        public void ListOperations_Clear_ElementsAreRemoved()
        {
            Assert.Ignore();

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
        public void ListOperations_Insert_ElementIsInserted()
        {
            Assert.Ignore();

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
        public void ListOperations_InsertRange_ElementsAreInserted()
        {
            Assert.Ignore();

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
        public void ListOperations_Remove_ElementIsRemoved()
        {
            Assert.Ignore();

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
        public void ListOperations_RemoveAt_ElementIsRemoved()
        {
            Assert.Ignore();

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
        public void ListOperations_Setting_ElementSets()
        {
            Assert.Ignore();

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
        public void ListOperations_SettingAll_ElementsAreSet()
        {
            Assert.Ignore();

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

        #region To String
        [Test]
        public void ToString_MatchesListToString()
        {
            int[] values = { 10, 11 };
            ContextStateList<int> contextStateList = new(values);

            string? expected = new List<int>(values).ToString();
            Assert.AreEqual(expected, contextStateList.ToString());
        }
        #endregion
    }
}
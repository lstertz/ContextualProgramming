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
            int[] values = { 10, 11 };
            ContextStateList<int> contextStateList = values;

#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            Assert.Throws<ArgumentNullException>(() =>
                (contextStateList as IBindableState)?.Bind(null));
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
        }

        [Test]
        public void Binding_BoundState_AddRangeWillNotify()
        {
            bool wasNotified = false;

            int[] values = { 10, 11 };
            ContextStateList<int> contextStateList = values;
            (contextStateList as IBindableState)?.Bind(() => wasNotified = true);

            contextStateList.AddRange(new int[] { 12, 13 });

            Assert.IsTrue(wasNotified);
        }

        [Test]
        public void Binding_BoundState_AddWillNotify()
        {
            bool wasNotified = false;

            int[] values = { 10, 11 };
            ContextStateList<int> contextStateList = values;
            (contextStateList as IBindableState)?.Bind(() => wasNotified = true);

            contextStateList.Add(12);

            Assert.IsTrue(wasNotified);
        }

        [Test]
        public void Binding_BoundState_ClearOfEmptyListWillNotNotify()
        {
            bool wasNotified = false;

            ContextStateList<int> contextStateList = Array.Empty<int>();
            (contextStateList as IBindableState)?.Bind(() => wasNotified = true);

            contextStateList.Clear();

            Assert.IsFalse(wasNotified);
        }

        [Test]
        public void Binding_BoundState_ClearWillNotify()
        {
            bool wasNotified = false;

            int[] values = { 10, 11 };
            ContextStateList<int> contextStateList = values;
            (contextStateList as IBindableState)?.Bind(() => wasNotified = true);

            contextStateList.Clear();

            Assert.IsTrue(wasNotified);
        }

        [Test]
        public void Binding_BoundState_ElementChangeWillNotify()
        {
            bool wasNotified = false;

            int[] values = { 10, 11 };
            ContextStateList<int> contextStateList = values;
            (contextStateList as IBindableState)?.Bind(() => wasNotified = true);

            contextStateList[0] = 12;

            Assert.IsTrue(wasNotified);
        }

        [Test]
        public void Binding_BoundState_ElementsChangeWillNotify()
        {
            bool wasNotified = false;

            int[] values = { 10, 11 };
            ContextStateList<int> contextStateList = values;
            (contextStateList as IBindableState)?.Bind(() => wasNotified = true);

            contextStateList.Elements = new int[] { 12, 13 };

            Assert.IsTrue(wasNotified);
        }

        [Test]
        public void Binding_BoundState_ElementChangeWillNotify_FromNull()
        {
            bool wasNotified = false;

            string?[] values = { null, "a" };
            ContextStateList<string> contextStateList = values;
            (contextStateList as IBindableState)?.Bind(() => wasNotified = true);

            contextStateList[0] = "b";

            Assert.IsTrue(wasNotified);
        }

        [Test]
        public void Binding_BoundState_ElementChangeWillNotify_ToNull()
        {
            bool wasNotified = false;

            string?[] values = { "a", "b" };
            ContextStateList<string> contextStateList = values;
            (contextStateList as IBindableState)?.Bind(() => wasNotified = true);

            contextStateList[0] = null;

            Assert.IsTrue(wasNotified);
        }

        [Test]
        public void Binding_BoundState_ElementUnchangedDoesNotNotify()
        {
            bool wasNotified = false;

            int[] values = { 10, 11 };
            ContextStateList<int> contextStateList = values;
            (contextStateList as IBindableState)?.Bind(() => wasNotified = true);

            contextStateList[0] = 10;

            Assert.IsFalse(wasNotified);
        }

        [Test]
        public void Binding_BoundState_EmptySetToEmptyDoesNotNotify()
        {
            bool wasNotified = false;

            ContextStateList<int> contextStateList = Array.Empty<int>();
            (contextStateList as IBindableState)?.Bind(() => wasNotified = true);

            contextStateList.Elements = Array.Empty<int>();

            Assert.IsFalse(wasNotified);
        }

        [Test]
        public void Binding_BoundState_EmptySetToNullDoesNotNotify()
        {
            bool wasNotified = false;

            ContextStateList<int> contextStateList = Array.Empty<int>();
            (contextStateList as IBindableState)?.Bind(() => wasNotified = true);

            contextStateList.Elements = null;

            Assert.IsFalse(wasNotified);
        }

        [Test]
        public void Binding_BoundState_InsertWillNotify()
        {
            bool wasNotified = false;

            int[] values = { 10, 11 };
            ContextStateList<int> contextStateList = values;
            (contextStateList as IBindableState)?.Bind(() => wasNotified = true);

            contextStateList.Insert(1, 12);

            Assert.IsTrue(wasNotified);
        }

        [Test]
        public void Binding_BoundState_InsertRangeWillNotify()
        {
            bool wasNotified = false;

            int[] values = { 10, 11 };
            ContextStateList<int> contextStateList = values;
            (contextStateList as IBindableState)?.Bind(() => wasNotified = true);

            contextStateList.InsertRange(1, new int[] {12, 13 });

            Assert.IsTrue(wasNotified);
        }

        [Test]
        public void Binding_BoundState_NonRemoveWillNotify()
        {
            bool wasNotified = false;

            int[] values = { 10, 11 };
            ContextStateList<int> contextStateList = values;
            (contextStateList as IBindableState)?.Bind(() => wasNotified = true);

            contextStateList.Remove(12);

            Assert.IsFalse(wasNotified);
        }

        [Test]
        public void Binding_BoundState_RemoveWillNotify()
        {
            bool wasNotified = false;

            int[] values = { 10, 11 };
            ContextStateList<int> contextStateList = values;
            (contextStateList as IBindableState)?.Bind(() => wasNotified = true);

            contextStateList.Remove(10);

            Assert.IsTrue(wasNotified);
        }

        [Test]
        public void Binding_BoundState_RemoveAtWillNotify()
        {
            bool wasNotified = false;

            int[] values = { 10, 11 };
            ContextStateList<int> contextStateList = values;
            (contextStateList as IBindableState)?.Bind(() => wasNotified = true);

            contextStateList.RemoveAt(0);

            Assert.IsTrue(wasNotified);
        }

        [Test]
        public void Binding_BoundState_RetrievalDoesNotNotify()
        {
            bool wasNotified = false;

            int[] values = { 10, 11 };
            ContextStateList<int> contextStateList = values;
            (contextStateList as IBindableState)?.Bind(() => wasNotified = true);

            int value = contextStateList[0];

            Assert.IsFalse(wasNotified);
        }

        [Test]
        public void Binding_ReboundState_ChangeWillNotify()
        {
            bool wasNotified = false;

            int[] values = { 10, 11 };
            ContextStateList<int> contextStateList = values;
            (contextStateList as IBindableState)?.Bind(() => wasNotified = false);
            (contextStateList as IBindableState)?.Bind(() => wasNotified = true);

            contextStateList[0] = 12;

            Assert.IsTrue(wasNotified);
        }

        [Test]
        public void Binding_UnboundState_AddDoesNotNotify()
        {
            bool wasNotified = false;

            int[] values = { 10, 11 };
            ContextStateList<int> contextStateList = values;
            (contextStateList as IBindableState)?.Bind(() => wasNotified = true);
            (contextStateList as IBindableState)?.Unbind();

            contextStateList.Add(12);

            Assert.IsFalse(wasNotified);
        }

        [Test]
        public void Binding_UnboundState_AddRangeDoesNotNotify()
        {
            bool wasNotified = false;

            int[] values = { 10, 11 };
            ContextStateList<int> contextStateList = values;
            (contextStateList as IBindableState)?.Bind(() => wasNotified = true);
            (contextStateList as IBindableState)?.Unbind();

            contextStateList.AddRange(new int[] { 12, 13 });

            Assert.IsFalse(wasNotified);
        }

        [Test]
        public void Binding_UnboundState_ClearDoesNotNotify()
        {
            bool wasNotified = false;

            int[] values = { 10, 11 };
            ContextStateList<int> contextStateList = values;
            (contextStateList as IBindableState)?.Bind(() => wasNotified = true);
            (contextStateList as IBindableState)?.Unbind();

            contextStateList.Clear();

            Assert.IsFalse(wasNotified);
        }

        [Test]
        public void Binding_UnboundState_ElementChangeDoesNotNotify()
        {
            bool wasNotified = false;

            int[] values = { 10, 11 };
            ContextStateList<int> contextStateList = values;
            (contextStateList as IBindableState)?.Bind(() => wasNotified = true);
            (contextStateList as IBindableState)?.Unbind();

            contextStateList[0] = 12;

            Assert.IsFalse(wasNotified);
        }

        [Test]
        public void Binding_UnboundState_ElementsChangeDoesNotNotify()
        {
            bool wasNotified = false;

            int[] values = { 10, 11 };
            ContextStateList<int> contextStateList = values;
            (contextStateList as IBindableState)?.Bind(() => wasNotified = true);
            (contextStateList as IBindableState)?.Unbind();

            contextStateList.Elements = new int[] { 12, 13 };

            Assert.IsFalse(wasNotified);
        }

        [Test]
        public void Binding_UnboundState_InsertDoesNotNotify()
        {
            bool wasNotified = false;

            int[] values = { 10, 11 };
            ContextStateList<int> contextStateList = values;
            (contextStateList as IBindableState)?.Bind(() => wasNotified = true);
            (contextStateList as IBindableState)?.Unbind();

            contextStateList.Insert(1, 12);

            Assert.IsFalse(wasNotified);
        }

        [Test]
        public void Binding_UnboundState_InsertRangeDoesNotNotify()
        {
            bool wasNotified = false;

            int[] values = { 10, 11 };
            ContextStateList<int> contextStateList = values;
            (contextStateList as IBindableState)?.Bind(() => wasNotified = true);
            (contextStateList as IBindableState)?.Unbind();

            contextStateList.InsertRange(1, new int[] {12, 13 });

            Assert.IsFalse(wasNotified);
        }

        [Test]
        public void Binding_UnboundState_RemoveAtDoesNotNotify()
        {
            bool wasNotified = false;

            int[] values = { 10, 11 };
            ContextStateList<int> contextStateList = values;
            (contextStateList as IBindableState)?.Bind(() => wasNotified = true);
            (contextStateList as IBindableState)?.Unbind();

            contextStateList.RemoveAt(0);

            Assert.IsFalse(wasNotified);
        }

        [Test]
        public void Binding_UnboundState_RemoveDoesNotNotify()
        {
            bool wasNotified = false;

            int[] values = { 10, 11 };
            ContextStateList<int> contextStateList = values;
            (contextStateList as IBindableState)?.Bind(() => wasNotified = true);
            (contextStateList as IBindableState)?.Unbind();

            contextStateList.Remove(10);

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
            int[] values = { 10, 11 };
            int[] expected = { 10, 11, 12 };
            ContextStateList<int> contextStateList = values;

            contextStateList.Add(12);

            Assert.AreEqual(expected, contextStateList.Elements);
        }

        [Test]
        public void ListOperations_AddRange_ElementsAreAdded()
        {
            int[] values = { 10, 11 };
            int[] expected = { 10, 11, 12, 13 };
            ContextStateList<int> contextStateList = values;

            contextStateList.AddRange(new[] { 12, 13 });

            Assert.AreEqual(expected, contextStateList.Elements);
        }

        [Test]
        public void ListOperations_Clear_ElementsAreRemoved()
        {
            int[] values = { 10, 11 };
            ContextStateList<int> contextStateList = values;

            contextStateList.Clear();

            Assert.AreEqual(Array.Empty<int>(), contextStateList.Elements);
        }

        [Test]
        public void ListOperations_Getting_ProvidesExpectedElement()
        {
            int[] values = { 10, 11 };
            ContextStateList<int> contextStateList = values;

            Assert.AreEqual(values[0], contextStateList[0]);
            Assert.AreEqual(values[1], contextStateList[1]);
        }

        [Test]
        public void ListOperations_Insert_ElementIsInserted()
        {
            int[] values = { 10, 11 };
            int[] expected = { 10, 12, 11 };
            ContextStateList<int> contextStateList = values;

            contextStateList.Insert(1, 12);

            Assert.AreEqual(expected, contextStateList.Elements);
        }

        [Test]
        public void ListOperations_Insert_OutOfRangeThrowsException()
        {
            int[] values = { 10, 11 };
            ContextStateList<int> contextStateList = values;

            Assert.Throws<ArgumentOutOfRangeException>(() =>
                contextStateList.Insert(3, 12));
        }

        [Test]
        public void ListOperations_InsertRange_ElementsAreInserted()
        {
            int[] values = { 10, 11 };
            int[] expected = { 10, 12, 13, 11 };
            ContextStateList<int> contextStateList = values;

            contextStateList.InsertRange(1, new[] { 12, 13 });

            Assert.AreEqual(expected, contextStateList.Elements);
        }

        [Test]
        public void ListOperations_InsertRange_OutOfRangeThrowsException()
        {
            int[] values = { 10, 11 };
            ContextStateList<int> contextStateList = values;

            Assert.Throws<ArgumentOutOfRangeException>(() =>
                contextStateList.InsertRange(3, new[] { 12, 13 }));
        }

        [Test]
        public void ListOperations_Remove_ElementIsRemoved()
        {
            int[] values = { 10, 11 };
            int[] expected = { 11 };
            ContextStateList<int> contextStateList = values;

            contextStateList.Remove(10);

            Assert.AreEqual(expected, contextStateList.Elements);
        }

        [Test]
        public void ListOperations_RemoveAt_ElementIsRemoved()
        {
            int[] values = { 10, 11 };
            int[] expected = { 11 };
            ContextStateList<int> contextStateList = values;

            contextStateList.RemoveAt(0);

            Assert.AreEqual(expected, contextStateList.Elements);
        }

        [Test]
        public void ListOperations_RemoveAt_OutOfRangeThrowsException()
        {
            int[] values = { 10, 11 };
            int[] expected = { 11 };
            ContextStateList<int> contextStateList = values;

            Assert.Throws<ArgumentOutOfRangeException>(() => contextStateList.RemoveAt(4));
        }

        [Test]
        public void ListOperations_Setting_ElementSets()
        {
            int[] values = { 10, 11 };
            int[] expected = { 10, 12 };
            ContextStateList<int> contextStateList = values;

            contextStateList[1] = expected[1];

            Assert.AreEqual(expected, contextStateList.Elements);
        }

        [Test]
        public void ListOperations_SettingAll_ElementsAreSet()
        {
            int[] values = { 10, 11 };
            int[] expected = { 10, 11, 12 };
            ContextStateList<int> contextStateList = values;

            contextStateList.Elements = expected;

            Assert.AreEqual(expected, contextStateList.Elements);
        }

        [Test]
        public void ListOperations_SettingAll_NullSetsEmptyList()
        {
            int[] values = { 10, 11 };
            ContextStateList<int> contextStateList = values;

            contextStateList.Elements = null;

            Assert.AreEqual(Array.Empty<int>(), contextStateList.Elements);
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
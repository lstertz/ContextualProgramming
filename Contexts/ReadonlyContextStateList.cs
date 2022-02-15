using ContextualProgramming.Internal;
using System.Linq;

namespace ContextualProgramming
{
    /// <summary>
    /// Encapsulates a list of elements to be used 
    /// within a context (<see cref="BaseContextAttribute"/>).
    /// </summary>
    /// <typeparam name="T">The type of elements encapsulated.
    /// This type should be a primitive-like type (int, string, etc.) and not 
    /// an object or struct with internal values.</typeparam>
    public class ReadonlyContextStateList<T> : State<T?[]>
    {
        /// <summary>
        /// Implicitly converts an array of values to their equivalent readonly context state list.
        /// </summary>
        /// <param name="values">The array of values to be converted.</param>
        public static implicit operator ReadonlyContextStateList<T>(T?[]? values) => new(values);

        /// <summary>
        /// Implicitly converts a readonly context state list to its underlying array of values.
        /// </summary>
        /// <param name="stateList">The readonly context state list to be converted.</param>
        public static implicit operator T?[]?(ReadonlyContextStateList<T> stateList) =>
            stateList.InternalValue;

        /// <summary>
        /// Checks for equality between two context state lists.
        /// </summary>
        /// <param name="a">The first state.</param>
        /// <param name="b">The second state.</param>
        /// <returns>Whether the two states are equal.</returns>
        public static bool operator ==(ReadonlyContextStateList<T>? a, 
            ReadonlyContextStateList<T>? b) =>
            Equals(a, null) ? Equals(b, null) : a.Equals(b);

        /// <summary>
        /// Checks for inequality between two context state lists.
        /// </summary>
        /// <param name="a">The first state.</param>
        /// <param name="b">The second state.</param>
        /// <returns>Whether the two states are inequal.</returns>
        public static bool operator !=(ReadonlyContextStateList<T>? a, 
            ReadonlyContextStateList<T>? b) =>
            Equals(a, null) ? !Equals(b, null) : !a.Equals(b);


        /// <summary>
        /// Provides the element of this readonly context state list at the specified index.
        /// </summary>
        /// <param name="index">The index of the element to be provided.</param>
        /// <returns>The element.</returns>
        public T? this[int index] => InternalValue[index];

        /// <summary>
        /// Provides the number of encapsulated elements of the readonly context state list.
        /// </summary>
        public int Count => InternalValue.Length;

        /// <summary>
        /// The encapsulated elements of the readonly context state list.
        /// </summary>
        public T?[]? Elements => InternalValue.ToArray();


        /// <summary>
        /// Constructs a new readonly context state list with the specified elements 
        /// for it to encapsulate.
        /// </summary>
        /// <param name="values">The encapsulated elements of the readonly context 
        /// state list.</param>
        public ReadonlyContextStateList(T?[]? values) : base(values == null ? 
            Array.Empty<T?>() : values.ToArray()) { }


        /// <inheritdoc/>
        protected override State<T?[]>? Convert(object? other)
        {
            if (other is T?[] array)
                return new ReadonlyContextStateList<T>(array);

            if (other is List<T?> list)
                return new ReadonlyContextStateList<T>(list.ToArray());

            return null;
        }


        /// <inheritdoc/>
        public override bool Equals(State<T?[]>? other)
        {
            ReadonlyContextStateList<T>? o = other as ReadonlyContextStateList<T>;
            if (Equals(o, null))
                return false;

            T?[] elements = InternalValue;
            T?[] otherElements = o.InternalValue;
            if (elements.Length != otherElements.Length)
                return false;

            for (int c = 0, count = elements.Length; c < count; c++)
            {
                T? element = elements[c];
                if (element == null)
                {
                    if (otherElements[c] != null)
                        return false;
                }
                else if (!element.Equals(otherElements[c]))
                    return false;
            }

            return true;
        }

        /// <inheritdoc/>
        public override bool Equals(object? obj) => base.Equals(obj);

        /// <inheritdoc/>
        public override int GetHashCode() => base.GetHashCode();
    }
}

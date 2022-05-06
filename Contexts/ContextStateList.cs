using ContextualProgramming.Internal;

namespace ContextualProgramming
{
    /// <summary>
    /// Encapsulates a list of elements to be used 
    /// within a context (<see cref="BaseContextAttribute"/>).
    /// </summary>
    /// <typeparam name="T">The type of elements encapsulated.
    /// This type should be a primitive-like type (int, string, etc.) and not 
    /// an object or struct with internal values.</typeparam>
    public class ContextStateList<T> : State<List<T?>>, IBindableState
    {
        /// <summary>
        /// Implicitly converts an array of values to their equivalent context state list.
        /// </summary>
        /// <param name="values">The array of values to be converted.</param>
        public static implicit operator ContextStateList<T>(T?[]? values) => new(values);

        /// <summary>
        /// Implicitly converts a context state list to its underlying array of values.
        /// </summary>
        /// <param name="stateList">The context state list to be converted.</param>
        public static implicit operator T?[]?(ContextStateList<T> stateList) =>
            stateList.InternalValue.ToArray();

        /// <summary>
        /// Checks for equality between two context state lists.
        /// </summary>
        /// <param name="a">The first state.</param>
        /// <param name="b">The second state.</param>
        /// <returns>Whether the two states are equal.</returns>
        public static bool operator ==(ContextStateList<T>? a, ContextStateList<T>? b) =>
            Equals(a, null) ? Equals(b, null) : a.Equals(b);

        /// <summary>
        /// Checks for inequality between two context state lists.
        /// </summary>
        /// <param name="a">The first state.</param>
        /// <param name="b">The second state.</param>
        /// <returns>Whether the two states are inequal.</returns>
        public static bool operator !=(ContextStateList<T>? a, ContextStateList<T>? b) =>
            Equals(a, null) ? !Equals(b, null) : !a.Equals(b);


        /// <summary>
        /// Provides the element of this context state list at the specified index.
        /// </summary>
        /// <param name="index">The index of the element to be provided.</param>
        /// <returns>The element.</returns>
        public T? this[int index]
        {
            get => InternalValue[index];
            set
            {
                T? element = InternalValue[index];
                if (element == null)
                {
                    if (value == null)
                        return;
                }
                else if (element.Equals(value))
                    return;

                InternalValue[index] = value;
                _onChange?.Invoke();
            }
        }

        /// <summary>
        /// Provides the number of encapsulated elements of the context state list.
        /// </summary>
        public int Count => InternalValue.Count;

        /// <summary>
        /// The encapsulated elements of the context state list.
        /// </summary>
        public T?[]? Elements
        {
            get => InternalValue.ToArray(); 
            set
            {
                List<T?> elements = InternalValue;

                if (elements.Count == 0 && (value == null || value.Length == 0))
                    return;
                if (elements.Equals(value))
                    return;

                elements.Clear();
                if (value != null && value.Length != 0)
                    elements.AddRange(value);

                _onChange?.Invoke();
            }
        }

        /// <inheritdoc/>
        bool IBindableState.IsBound => _onChange != null;


        private Action? _onChange;


        /// <summary>
        /// Constructs a new context state list with the specified elements for it to encapsulate.
        /// </summary>
        /// <param name="values">The encapsulated elements of the context state list.</param>
        public ContextStateList(T?[]? values) : base(values == null ? new() : new(values)) { }


        /// <inheritdoc/>
        protected override State<List<T?>>? Convert(object? other)
        {
            if (other is ContextStateList<T> contextStateList)
                return new ContextStateList<T>(contextStateList.Elements);

            if (other is T?[] array)
                return new ContextStateList<T>(array);

            if (other is List<T?> list)
                return new ContextStateList<T>(list.ToArray());

            return null;
        }


        /// <inheritdoc/>
        void IBindableState.Bind(Action onChange) => _onChange = onChange ??
            throw new ArgumentNullException(nameof(onChange), $"The binding action cannot " +
                $"be null. If attempting to unbind, use {nameof(IBindableState.Unbind)}.");

        /// <inheritdoc/>
        void IBindableState.Unbind() => _onChange = null;


        /// <inheritdoc/>
        public override bool Equals(State<List<T?>>? other)
        {
            ContextStateList<T>? o = other as ContextStateList<T>;
            if (Equals(o, null))
                return false;

            List<T?> elements = InternalValue;
            List<T?> otherElements = o.InternalValue;
            if (elements.Count != otherElements.Count)
                return false;

            for (int c = 0, count = elements.Count; c < count; c++)
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


        /// <summary>
        /// Adds the provided element.
        /// </summary>
        /// <remarks>
        /// This operation is considered a change in contextual state.
        /// </remarks>
        /// <param name="element">The element to be added.</param>
        public void Add(T element)
        {
            InternalValue.Add(element);
            _onChange?.Invoke();
        }

        /// <summary>
        /// Adds the provided elements.
        /// </summary>
        /// <remarks>
        /// This operation is considered a change in contextual state.
        /// </remarks>
        /// <param name="elements">The elements to be added.</param>
        public void AddRange(T?[] elements)
        {
            InternalValue.AddRange(elements);
            _onChange?.Invoke();
        }

        /// <summary>
        /// Removes all elements.
        /// </summary>
        /// <remarks>
        /// This operation is considered a change in contextual state.
        /// </remarks>
        public void Clear()
        {
            if (InternalValue.Count == 0)
                return;

            InternalValue.Clear();
            _onChange?.Invoke();
        }

        /// <summary>
        /// Inserts the provided element at the specified index.
        /// </summary>
        /// <remarks>
        /// This operation is considered a change in contextual state.
        /// </remarks>
        /// <param name="index">The index at which the element is to be inserted.</param>
        /// <param name="element">The element to be inserted.</param>
        public void Insert(int index, T element)
        {
            InternalValue.Insert(index, element);
            _onChange?.Invoke();
        }

        /// <summary>
        /// Inserts the provided elements starting at the specified index.
        /// </summary>
        /// <remarks>
        /// This operation is considered a change in contextual state.
        /// </remarks>
        /// <param name="index">The index at which the elements are to be inserted.</param>
        /// <param name="elements">The elements to be inserted.</param>
        public void InsertRange(int index, T?[] elements)
        {
            InternalValue.InsertRange(index, elements);
            _onChange?.Invoke();
        }

        /// <summary>
        /// Removes the specified element.
        /// </summary>
        /// <remarks>
        /// This operation is considered a change in contextual state.
        /// </remarks>
        /// <param name="element">The element to be removed.</param>
        public void Remove(T element)
        {
            if (InternalValue.Remove(element))
                _onChange?.Invoke();
        }

        /// <summary>
        /// Removes the element at the specified index.
        /// </summary>
        /// <remarks>
        /// This operation is considered a change in contextual state.
        /// </remarks>
        /// <param name="index">The index at which the removal is to occur.</param>
        public void RemoveAt(int index)
        {
            InternalValue.RemoveAt(index);
            _onChange?.Invoke();
        }


        /// <inheritdoc/>
        public override bool Equals(object? obj) => base.Equals(obj);

        /// <inheritdoc/>
        public override int GetHashCode() => base.GetHashCode();
    }
}

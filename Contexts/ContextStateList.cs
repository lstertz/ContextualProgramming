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
    public class ContextStateList<T> : IBindableState, IEquatable<ContextStateList<T>>
    {
        public static implicit operator ContextStateList<T>(T?[]? values) => new(values);
        public static implicit operator T?[]?(ContextStateList<T> contextStateList) =>
            contextStateList._elements.ToArray();

        public static bool operator ==(ContextStateList<T>? a, ContextStateList<T>? b) => 
            Equals(a, null) ? Equals(b, null) : a.Equals(b);
        public static bool operator !=(ContextStateList<T>? a, ContextStateList<T>? b) =>
            Equals(a, null) ? !Equals(b, null) : !a.Equals(b);


        public T? this[int index]
        {
            get => _elements[index];
            set
            {
                T? element = _elements[index];
                if (element == null)
                {
                    if (value == null)
                        return;
                }
                else if (element.Equals(value))
                    return;

                _elements[index] = value;
                _onChange?.Invoke();
            }
        }

        /// <summary>
        /// The encapsulated elements of the context state list.
        /// </summary>
        public T?[]? Elements
        {
            get => _elements.ToArray(); 
            set
            {
                if (_elements.Count == 0 && (value == null || value.Length == 0))
                    return;
                else if (_elements.Equals(value))
                    return;

                _elements.Clear();
                if (value != null && value.Length != 0)
                    _elements.AddRange(value);

                _onChange?.Invoke();
            }
        }
        private readonly List<T?> _elements;

        private Action? _onChange;


        /// <summary>
        /// Constructs a new context state list with the specified elements for it to encapsulate.
        /// </summary>
        /// <param name="values">The encapsulated elements of the context state list.</param>
        public ContextStateList(T?[]? values)
        {
            if (values == null)
                _elements = new();
            else
                _elements = new(values);
        }


        /// <inheritdoc/>
        void IBindableState.Bind(Action onChange) => _onChange = onChange ??
            throw new ArgumentNullException($"The binding action cannot be null. " +
                $"If attempting to unbind, use {nameof(IBindableState.Unbind)}.");

        /// <inheritdoc/>
        void IBindableState.Unbind() => _onChange = null;


        /// <summary>
        /// Adds the provided element.
        /// </summary>
        /// <remarks>
        /// This operation is considered a change in contextual state.
        /// </remarks>
        /// <param name="element">The element to be added.</param>
        public void Add(T element)
        {
            _elements.Add(element);
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
            _elements.AddRange(elements);
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
            if (_elements.Count == 0)
                return;

            _elements.Clear();
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
            _elements.Insert(index, element);
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
            _elements.InsertRange(index, elements);
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
            if (_elements.Remove(element))
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
            _elements.RemoveAt(index);
            _onChange?.Invoke();
        }


        /// <inheritdoc/>
        public override bool Equals(object? other)
        {
            return Equals(other as ContextStateList<T>);
        }

        /// <inheritdoc/>
        public bool Equals(ContextStateList<T>? other)
        {
            if (Equals(other, null))
                return false;

            if (_elements.Count != other._elements.Count)
                return false;

            for (int c = 0, count = _elements.Count; c < count; c++)
            {
                T? element = _elements[c];
                if (element == null)
                {
                    if (other._elements[c] != null)
                        return false;
                }
                else if (!element.Equals(other._elements[c]))
                    return false;
            }

            return true;
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        /// <inheritdoc/>
        public override string? ToString()
        {
            return _elements.ToString();
        }
    }
}

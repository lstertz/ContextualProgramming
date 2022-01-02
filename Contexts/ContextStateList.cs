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
    public class ContextStateList<T> : /*IBindableState,*/ IEquatable<ContextStateList<T>>
    {
        public static implicit operator ContextStateList<T>(T?[]? values) => 
            new ContextStateList<T>(values);
        public static implicit operator T?[]?(ContextStateList<T> contextStateList) =>
            contextStateList._elements.ToArray();

        public static bool operator ==(ContextStateList<T>? a, ContextStateList<T>? b) => 
            Equals(a, null) ? Equals(b, null) : a.Equals(b);
        public static bool operator !=(ContextStateList<T>? a, ContextStateList<T>? b) =>
            Equals(a, null) ? !Equals(b, null) : !a.Equals(b);


        /// <summary>
        /// The encapsulated elements of the context state list.
        /// </summary>
        public T?[]? Elements
        {
            get => _elements.ToArray(); 
            set
            {
                if (_elements.Count == 0 && value == null)
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

        /*
        /// <inheritdoc/>
        void IBindableState.Bind(Action onChange)
        {
            if (onChange == null)
                throw new ArgumentNullException($"The binding action cannot be null. " +
                    $"If attempting to unbind, use {nameof(IBindableState.Unbind)}.");

            _onChange = onChange;
        }

        /// <inheritdoc/>
        void IBindableState.Unbind()
        {
            _onChange = null;
        }
        */


        public void Add(T element)
        {

        }

        public void AddRange(T?[] elements)
        {

        }

        public void Clear()
        {

        }

        public void Insert(int index, T element)
        {

        }

        public void InsertRange(int index, T?[] elements)
        {
        }

        public void Remove(T element)
        {

        }

        public void RemoveAt(int index)
        {

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

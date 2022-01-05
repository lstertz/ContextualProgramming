namespace System
{
    /// <summary>
    /// Extensions for <see cref="System"/>-level scope types 
    /// (<see cref="object"/>, <see cref="string"/>, etc.).
    /// </summary>
    internal static class Extensions
    {
        /// <summary>
        /// Ensures that the value is not null.
        /// </summary>
        /// <typeparam name="T">The type of the value.</typeparam>
        /// <param name="value">The value.</param>
        /// <returns>The guaranteed non-null value.</returns>
        /// <exception cref="ArgumentNullException">Thrown if the value is null.</exception>
        public static T EnsureNotNull<T>(this T? value) => 
            value == null ? throw new ArgumentNullException(nameof(value)) : value;
    }
}

namespace System.Collections.Generic
{
    /// <summary>
    /// Extensions for <see cref="Collections"/>-level scope types 
    /// (<see cref="object"/>, <see cref="string"/>, etc.).
    /// </summary>
    internal static class Extensions
    {
        /// <summary>
        /// Flips this dictionary's keys and values in a new dictionary.
        /// </summary>
        /// <typeparam name="K">The type of this dictionary's keys.</typeparam>
        /// <typeparam name="V">The type of this dictionary's values.</typeparam>
        /// <param name="dict">This dictionary whose keys and values 
        /// are to be flipped.</param>
        /// <returns>A new dictionary with the values of the original dictionary 
        /// as its keys and the original dictionary's keys as its values.</returns>
        public static Dictionary<V, K> Flip<K, V>(this Dictionary<K, V> dict)
            where K : notnull where V : notnull
        {
            Dictionary<V, K> flipped = new();
            foreach (K key in dict.Keys)
                flipped.Add(dict[key], key);

            return flipped;
        }
    }
}

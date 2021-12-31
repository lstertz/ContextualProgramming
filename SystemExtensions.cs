namespace System
{
    /// <summary>
    /// Extensions for <see cref="System"/>-level scope types 
    /// (<see cref="object"/>, <see cref="string"/>, etc.).
    /// </summary>
    public static class SystemExtensions
    {
        /// <summary>
        /// Ensures that the value is not null.
        /// </summary>
        /// <typeparam name="T">The type of the value.</typeparam>
        /// <param name="value">The value.</param>
        /// <returns>The guaranteed non-null value.</returns>
        /// <exception cref="ArgumentNullException">Thrown if the value is null.</exception>
        public static T EnsureNonNullable<T>(this T? value) => 
            value == null ? throw new ArgumentNullException(nameof(value)) : value;
    }
}

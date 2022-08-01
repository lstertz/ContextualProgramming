namespace ContextualProgramming.IO.Internal;

/// <summary>
/// Displays any text specified in <see cref="ConsoleOutput"/> in the console.
/// </summary>
[Dependency<ConsoleOutput>(Binding.Unique, Fulfillment.Existing, Output)]
[Behavior]
public class ConsoleDisplaying
{
    private const string Output = "output";


    /// <summary>
    /// Defines a console whose static functions will serve as the means 
    /// to update the display.
    /// </summary>
    protected interface IConsole
    {
        /// <summary>
        /// Clears the display of the console.
        /// </summary>
        public static abstract void Clear();

        /// <summary>
        /// Writes the provided value to the console.
        /// </summary>
        /// <param name="value">The value to be written.</param>
        public static abstract void Write(string? value);

        /// <summary>
        /// Writes the provided value as a line to the console.
        /// </summary>
        /// <remarks>
        /// A written line will have a newline appended to it as part of the write.
        /// </remarks>
        /// <param name="value">The value to be written as a line.</param>
        public static abstract void WriteLine(string? value);
    }

    /// <summary>
    /// Wrapper for <see cref="System.Console"/>.
    /// </summary>
    private class Console : IConsole
    {
        /// <summary>
        /// <see cref="System.Console.Clear"/>
        /// </summary>
        public static void Clear() => System.Console.Clear();

        /// <summary>
        /// <see cref="System.Console.Write(string?)"/>
        /// </summary>
        public static void Write(string? value) => System.Console.Write(value);

        /// <summary>
        /// <see cref="System.Console.WriteLine(string?)"/>
        /// </summary>
        public static void WriteLine(string? value) => System.Console.WriteLine(value);
    }


    /// <summary>
    /// <see cref="UpdateDisplay(ConsoleOutput)"/>
    /// </summary>
    /// <typeparam name="TConsole">The static console whose 
    /// display will be updated.</typeparam>
    protected static void UpdateDisplay<TConsole>(ConsoleOutput output)
        where TConsole : IConsole
    {
        TConsole.Clear();

        for (int c = 0; c < output.Lines.Count; c++)
            TConsole.WriteLine(output.Lines[c]);
        TConsole.Write(output.ActiveText);
    }


    /// <summary>
    /// Updates the displayed text on the console with the text 
    /// specified by the <paramref name="output"/>.
    /// </summary>
    /// <param name="output">Defines the texts to be output to the console.</param>
    [Operation]
    [OnChange(Output)]
    public virtual void UpdateDisplay(ConsoleOutput output)
    {
        UpdateDisplay<Console>(output);
    }
}
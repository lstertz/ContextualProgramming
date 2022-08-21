using System.Text;

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
        /// Gets the width of the buffer area.
        /// </summary>
        public static abstract int BufferWidth { get; }

        /// <summary>
        /// Gets the row position of the cursor within the buffer area.
        /// </summary>
        public static abstract int CursorTop { get; }

        /// <summary>
        /// Sets a value indicating whether the cursor is visible.
        /// </summary>
        public static abstract bool CursorVisible { set; }

        /// <summary>
        /// Clears the display of the console.
        /// </summary>
        public static abstract void Clear();

        /// <summary>
        /// Sets the position of the cursor.
        /// </summary>
        public static abstract void SetCursorPosition(int left, int top);

        /// <summary>
        /// Writes the provided value to the console.
        /// </summary>
        /// <param name="value">The value to be written.</param>
        public static abstract void Write(string? value);
    }

    /// <summary>
    /// Wrapper for <see cref="System.Console"/>.
    /// </summary>
    private class Console : IConsole
    {
        /// <inheritdoc/>
        public static int BufferWidth 
        { 
            get => System.Console.BufferWidth;
        }

        /// <summary>
        /// <see cref="System.Console.CursorTop"/>
        /// </summary>
        public static int CursorTop
        {
            get => System.Console.CursorTop;
            set => System.Console.CursorTop = value;
        }

        /// <inheritdoc/>
        public static bool CursorVisible 
        {
            set => System.Console.CursorVisible = value;
        }

        /// <summary>
        /// <see cref="System.Console.Clear"/>
        /// </summary>
        public static void Clear() => System.Console.Clear();

        /// <summary>
        /// <see cref="System.Console.SetCursorPosition"/>
        /// </summary>
        public static void SetCursorPosition(int left, int top) =>
            System.Console.SetCursorPosition(left, top);

        /// <summary>
        /// <see cref="System.Console.Write(string?)"/>
        /// </summary>
        public static void Write(string? value) => System.Console.Write(value);
    }


    /// <summary>
    /// <see cref="UpdateDisplay(ConsoleOutput)"/>
    /// </summary>
    /// <typeparam name="TConsole">The static console whose 
    /// display will be updated.</typeparam>
    protected static void UpdateDisplay<TConsole>(ConsoleOutput output)
        where TConsole : IConsole
    {
        int bufferWidth = TConsole.BufferWidth;

        StringBuilder displayString = new();
        int newLeft = output.ActiveText.Value.Length;
        int newTop = output.Lines.Count;

        for (int c = 0; c < output.Lines.Count; c++)
            displayString.AppendLine(output.Lines[c].PadRight(bufferWidth));

        displayString.AppendLine(output.ActiveText.Value.PadRight(bufferWidth));

        for (int c = output.Lines.Count, count = TConsole.CursorTop; c < count; c++)
            displayString.AppendLine(" ".PadRight(bufferWidth));

        TConsole.CursorVisible = false;

        TConsole.SetCursorPosition(0, 0);
        TConsole.Write(displayString.ToString());
        TConsole.SetCursorPosition(newLeft, newTop);

        TConsole.CursorVisible = true;
    }


    /// <summary>
    /// Updates the display with text initially present in the output context.
    /// </summary>
    public ConsoleDisplaying(ConsoleOutput output)
        => UpdateDisplay(output);


    /// <summary>
    /// Updates the displayed text on the console with the text 
    /// specified by the <paramref name="output"/>.
    /// </summary>
    /// <param name="output">Defines the texts to be output to the console.</param>
    [Operation]
    [OnChange(Output)]
    public virtual void UpdateDisplay(ConsoleOutput output) => UpdateDisplay<Console>(output);
}
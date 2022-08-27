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
        public static int BufferWidth => System.Console.BufferWidth;

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
        int newLeft = output.ActiveText.Value.Length % bufferWidth;
        int newTop = 0;

        for (int c = 0; c < output.Lines.Count; c++)
            AppendLine(displayString, output.Lines[c], bufferWidth, ref newTop);

        if (newTop < TConsole.CursorTop || output.ActiveText.Value != string.Empty)
            AppendLine(displayString, output.ActiveText.Value, bufferWidth, ref newTop, false);

        for (int c = newTop, count = TConsole.CursorTop; c < count; c++)
            displayString.AppendLine(" ".PadRight(bufferWidth));

        TConsole.CursorVisible = false;

        TConsole.SetCursorPosition(0, 0);
        TConsole.Write(displayString.ToString());
        TConsole.SetCursorPosition(newLeft, newTop);

        TConsole.CursorVisible = true;
    }

    /// <summary>
    /// Appends the provided line to the display string, taking into account wrapping that should 
    /// occur for the specified buffer width.
    /// </summary>
    /// <param name="displayString">The string builder for the display string, 
    /// which is the string being appended to.</param>
    /// <param name="line">The line being appended.</param>
    /// <param name="bufferWidth">The buffer width, which defines the wrapping 
    /// imposed upon the line being appended.</param>
    /// <param name="newTop">The cursor line after the appending.</param>
    /// <param name="incrementForFinalLine">Whether the cursor line should 
    /// be incremented after the final line has been appended.</param>
    private static void AppendLine(StringBuilder displayString, string line, int bufferWidth,
        ref int newTop, bool incrementForFinalLine = true)
    {
        if (line.Length > 0)
        {
            while (line.Length >= bufferWidth)
            {
                displayString.AppendLine(line[..bufferWidth]);
                line = line[bufferWidth..];
                newTop++;
            }

            if (line.Length == 0)
                return;
        }

        displayString.AppendLine(line.PadRight(bufferWidth));
        if (incrementForFinalLine)
            newTop++;
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
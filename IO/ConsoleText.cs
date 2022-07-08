namespace ContextualProgramming.IO;

/// <summary>
/// Provides the text that will be present in the console.
/// </summary>
public class ConsoleText
{
    /// <summary>
    /// The text of a line currently being input or being defined in lieu of actual input.
    /// </summary>
    /// <remarks>
    /// Any such text will be added as a new line in <see cref="Lines"/> once 
    /// the input has been finalized (e.g. the user pressed 'enter').
    /// </remarks>
    public ContextState<string> CurrentLine { get; init; } = string.Empty;

    /// <summary>
    /// The lines of text known to the console.
    /// </summary>
    /// <remarks>
    /// Lines are expected to be ordered from earliest to latest, like a queue.
    /// </remarks>
    public ContextStateList<string> Lines { get; init; } = Array.Empty<string>();
}
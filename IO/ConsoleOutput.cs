namespace ContextualProgramming.IO;

/// <summary>
/// Defines the settings for and lines to display to the console.
/// </summary>
[Context]
public class ConsoleOutput
{
    /// <summary>
    /// The current unsubmitted text that the user is manipulating, 
    /// essentially the user's input to the console.
    /// </summary>
    public ContextState<string> ActiveText { get; init; } = string.Empty;

    /// <summary>
    /// The lines of text to be displayed to the console.
    /// </summary>
    /// <remarks>
    /// Lines are expected to be ordered from earliest to latest, like a queue.
    /// </remarks>
    public ContextStateList<string> Lines { get; init; } = Array.Empty<string>();
}
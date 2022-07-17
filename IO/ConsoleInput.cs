namespace ContextualProgramming.IO;

/// <summary>
/// Defines the settings for and input read from the console.
/// </summary>
[Context]
public class ConsoleInput
{
    /// <summary>
    /// The text of a line currently being input or being defined in lieu of actual input.
    /// </summary>
    /// <remarks>
    /// Any such text will be added as a new input in <see cref="Submitted"/> once 
    /// it has been submitted (e.g. the user pressed 'enter').
    /// </remarks>
    public ContextState<string> Unsubmitted { get; init; } = string.Empty;

    /// <summary>
    /// The lines of text that have been submitted through the console.
    /// </summary>
    /// <remarks>
    /// Lines are expected to be ordered from earliest to latest, like a queue.
    /// </remarks>
    public ContextStateList<string> Submitted { get; init; } = Array.Empty<string>();
}
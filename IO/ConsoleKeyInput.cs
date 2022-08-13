namespace ContextualProgramming.IO;

/// <summary>
/// Defines the details of any key input from the console and 
/// the settings for handling that input.
/// </summary>
[Context]
public class ConsoleKeyInput
{
    /// <summary>
    /// The keys that are currently being pressed.
    /// </summary>
    public ContextStateList<ConsoleKeyInfo> PressedKeys { get; init; } = 
        Array.Empty<ConsoleKeyInfo>();

    /// <summary>
    /// The number of consecutive times the <see cref="PressedKeys"/> have been 
    /// read as pressed input.
    /// </summary>
    public ContextState<int> PressedTicks { get; init; } = 0;

    /// <summary>
    /// The keys that were released on the last check for input.
    /// </summary>
    public ContextStateList<ConsoleKeyInfo> ReleasedKeys { get; init; } = 
        Array.Empty<ConsoleKeyInfo>();
}
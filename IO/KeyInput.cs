namespace ContextualProgramming.IO;

/// <summary>
/// Defines the details of any key input from the console and 
/// the settings for handling that input.
/// </summary>
[Context]
public class KeyInput
{
    // TODO :: Possibly update from the changes in KeyListener.

    /// <summary>
    /// The current key that is being pressed.
    /// </summary>
    public ContextState<ConsoleKeyInfo> ActiveKey { get; init; } = new ConsoleKeyInfo();

    /// <summary>
    /// The number of consecutive times the <see cref="ActiveKey"/> has been 
    /// read as an input.
    /// </summary>
    public ContextState<int> InputCount { get; init; } = 0;
}
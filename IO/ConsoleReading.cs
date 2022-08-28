namespace ContextualProgramming.IO.Internal;

/// <summary>
/// Reads and records console input.
/// </summary>
[Behavior]
[Dependency<ConsoleInput>(Binding.Unique, Fulfillment.Existing, Input)]
[Dependency<ConsoleKeyInput>(Binding.Unique, Fulfillment.SelfCreated, KeyInput)]
public class ConsoleReading
{
    private const string Input = "input";
    private const string KeyInput = "keyInput";

    /// <summary>
    /// Sets up the reader with the specified input settings.
    /// </summary>
    public ConsoleReading(ConsoleInput input) { }

    /// <summary>
    /// Evauates any available keyboard input as if read from the console and either 
    /// alters the currently unsubmitted input or submits the last known unsubmitted input.
    /// </summary>
    /// <param name="input">The record for submitted and unsubmitted input.</param>
    /// <param name="keyInput">The current key input to be evaluated as 
    /// text input read from the console.</param>
    [Operation]
    [OnChange(KeyInput)]
    public void ReadKeyInput(ConsoleInput input, ConsoleKeyInput keyInput)
    {
        if (keyInput.PressedKeys.Count == 0)
            return;

        ConsoleKeyInfo info;
        if (!GetValidKey(keyInput, out info))
            return;

        if (info.Key == ConsoleKey.Enter)
        {
            string line = input.Unsubmitted;
            input.Submitted.Add(line);

            input.Unsubmitted.Value = string.Empty;
        }
        else if (info.Key == ConsoleKey.Backspace || info.Key == ConsoleKey.Delete)
        {
            string line = input.Unsubmitted;
            if (line == null || line.Length == 0)
                return;

            input.Unsubmitted.Value = line[0..^1];
        }
        else
            input.Unsubmitted.Value += info.KeyChar;
    }

    /// <summary>
    /// Provides the key info from the current input and specifies whether it is 
    /// valid to be read.
    /// </summary>
    /// <param name="keyInput">The current key input to be evaluated.</param>
    /// <param name="info">The appropriate key info to possibly be read.</param>
    /// <returns>Whether the current input is valid to be read.</returns>
    private bool GetValidKey(ConsoleKeyInput keyInput, out ConsoleKeyInfo info)
    {
        info = keyInput.PressedKeys[^1];
        if (info.Modifiers.HasFlag(ConsoleModifiers.Alt))
            return false;
        if (info.Modifiers.HasFlag(ConsoleModifiers.Control))
            return false;

        return true;
    }
}
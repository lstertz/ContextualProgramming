namespace ContextualProgramming.IO.Internal;

/// <summary>
/// Reads and records console input.
/// </summary>
[Behavior]
[Dependency<ConsoleInput>(Binding.Unique, Fulfillment.Existing, Input)]
[Dependency<KeyInput>(Binding.Unique, Fulfillment.SelfCreated, KeyInput)]
public class ConsoleListener
{
    private const string Input = "input";
    private const string KeyInput = "keyInput";

    /// <summary>
    /// Sets up the reader with the specified input settings.
    /// </summary>
    public ConsoleListener(ConsoleInput input)
    {
    }

    /// <summary>
    /// Evauates any available keyboard input as if read from the console and either 
    /// alters the currenly unsubmitted input or submits the last known unsubmitted input.
    /// </summary>
    /// <param name="input">The record for submitted and unsubmitted input.</param>
    /// <param name="keyInput">The current key input to be evaluated as 
    /// text input read from the console.</param>
    [Operation]
    [OnChange(KeyInput)]
    public void EvaluateKeyInput(ConsoleInput input, KeyInput keyInput)
    {
        // TODO :: Update as KeyInput changes for globalized inputs.

        ConsoleKeyInfo info = keyInput.ActiveKey;
        if (info.Key == ConsoleKey.Enter)
        {
            string? line = input.Unsubmitted;
            if (line != null)
                input.Submitted.Add(line);

            input.Unsubmitted.Value = string.Empty;
        }
        else if (info.Key == ConsoleKey.Backspace)
        {
            string? line = input.Unsubmitted;
            if (line == null || line.Length == 0)
                return;

            input.Unsubmitted.Value = line[0..^1];
        }
        else
            input.Unsubmitted.Value += info.KeyChar;
    }
}
using ContextualProgramming.Internal;

namespace ContextualProgramming.IO;

/// <summary>
/// Reads and records console input.
/// </summary>
[Behavior]
[Dependency<ConsoleInput>(Binding.Unique, Fulfillment.Existing, Input)]
public class ConsoleReader
{
    private const string Input = "input";

    /// <summary>
    /// Sets up the reader with the specified input settings.
    /// </summary>
    public ConsoleReader(ConsoleInput input)
    {
    }

    /// <summary>
    /// Reads any available keyboard input from the console and evaluates it to 
    /// either alter the currenly unsubmitted input or to submit the last known unsubmitted input.
    /// </summary>
    /// <param name="input">The record for submitted and unsubmitted input.</param>
    [Operation]
    [OnUpdate]
    public void ReadConsole(ConsoleInput input)
    {
        while (Console.KeyAvailable)
        {
            var info = Console.ReadKey(true);
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
                    continue;

                input.Unsubmitted.Value = line[0..^1];
            }
            else
                input.Unsubmitted.Value += info.KeyChar;
        }
    }
}
using ContextualProgramming.Internal;

namespace ContextualProgramming.IO;

/// <summary>
/// Reads console input as specified by <see cref="ConsoleInput"/> and 
/// logs any read text to <see cref="ConsoleText"/>.
/// </summary>
[Behavior]
[Dependency<ConsoleText>(Binding.Unique, Fulfillment.Existing, Text)]
[Dependency<ConsoleInput>(Binding.Unique, Fulfillment.Existing, InputSettings)]
public class ConsoleReader
{
    private const string InputSettings = "inputSettings";
    private const string Text = "text";

    /// <summary>
    /// Sets up the reader's asynchronous console reader.
    /// </summary>
    public ConsoleReader(ConsoleInput inputSettings, ConsoleText text)
    {
    }

    /// <summary>
    /// Reads any available input from the console and logs it to the log.
    /// </summary>
    /// <param name="text">The record for input text where new input is logged.</param>
    [Operation]
    [OnUpdate]
    public void ReadConsole(ConsoleText text)
    {
        while (Console.KeyAvailable)
        {
            var info = Console.ReadKey(true);
            if (info.Key == ConsoleKey.Enter)
            {
                string? line = text.CurrentLine;
                if (line != null)
                    text.Lines.Add(line);

                text.CurrentLine.Value = string.Empty;
            }
            else if (info.Key == ConsoleKey.Backspace)
            {
                string? line = text.CurrentLine;
                if (line == null || line.Length == 0)
                    continue;

                text.CurrentLine.Value = line[0..^1];
            }
            else
                text.CurrentLine.Value += info.KeyChar;
        }
    }
}
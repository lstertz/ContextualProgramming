namespace ContextualProgramming.IO.Internal;

/// <summary>
/// Displays any text specified in <see cref="ConsoleOutput"/> in the console.
/// </summary>
[Dependency<ConsoleOutput>(Binding.Unique, Fulfillment.Existing, Output)]
[Behavior]
public class ConsoleDisplayer
{
    private const string Output = "output";


    /// <summary>
    /// Updates the displayed text on the console with the text 
    /// specified by the <paramref name="output"/>.
    /// </summary>
    /// <param name="output">Defines the texts to be output to the console.</param>
    [Operation]
    [OnChange(Output)]
    public void UpdateDisplay(ConsoleOutput output)
    {

    }
}
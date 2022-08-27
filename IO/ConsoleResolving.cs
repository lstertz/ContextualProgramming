namespace ContextualProgramming.IO.Internal;

/// <summary>
/// Evalutes new input recorded in <see cref="ConsoleInput"/> 
/// to determine the appropriate states of <see cref="ConsoleOutput"/>.
/// </summary>
[Behavior]
[Dependency<ConsoleInput>(Binding.Unique, Fulfillment.Existing, Input)]
[Dependency<ConsoleOutput>(Binding.Unique, Fulfillment.Existing, Output)]
public class ConsoleResolving
{
    private const string Input = "input";
    private const string Output = "output";

    // TODO :: Map console input to console output appropriately.
}
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

    // WORKAROUND : 30 :: For not knowing which indices of input.Submitted are new.
    // Note that removals and additions to input.Submitted in the same update may result 
    //     in submissions being excluded from output.Lines.
    private int _lastSubCount = 0;

    /// <summary>
    /// Performs initial resolving for the provided input and output.
    /// </summary>
    /// <param name="input">The input to be resolved to the output.</param>
    /// <param name="output">The output being modified by the input's resolution.</param>
    public ConsoleResolving(ConsoleInput input, ConsoleOutput output)
    {
        output.ActiveText.Value = input.Unsubmitted.Value;
        _lastSubCount = input.Submitted.Count;
    }

    /// <summary>
    /// Resolves the provided input and output.
    /// </summary>
    /// <param name="input">The input to be resolved to the output.</param>
    /// <param name="output">The output being modified by the input's resolution.</param>
    [Operation]
    [OnChange(Input)]
    public void ResolveInput(ConsoleInput input, ConsoleOutput output)
    {
        output.ActiveText.Value = input.Unsubmitted.Value;

        if (input.Submitted.Count < _lastSubCount)
        {
            _lastSubCount = input.Submitted.Count;
            return;
        }

        for (int count = input.Submitted.Count; _lastSubCount < count; _lastSubCount++)
            output.Lines.Add(input.Submitted[_lastSubCount]);
    }
}
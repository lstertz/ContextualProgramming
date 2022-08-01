namespace ContextualProgramming.Showcase.Contexts;

[Context]
public class ContextA
{
    public ContextState<int> State { get; init; } = 0;

    public ContextStateList<int> StateList { get; init; } = Array.Empty<int>();
}
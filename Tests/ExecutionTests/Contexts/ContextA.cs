namespace Tests.Contexts;

[Context]
public class ContextA
{
    public ContextState<int> State { get; set; } = 0;
}
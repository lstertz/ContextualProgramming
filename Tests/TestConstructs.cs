global using ContextualProgramming;

namespace Tests.Constructs
{
    #region Test Context Attributes
    public class TCAttribute : BaseContextAttribute { }
    #endregion

    #region Test Classes
    [TC]
    public class TestContextA
    {
        public int OnStateChangeIntValue { get; set; } = 0;
        public int OnContextChangeIntValue { get; set; } = 0;

        public ContextState<int> Int { get; init; } = 0;
    }
    #endregion
}
using ContextualProgramming.IO;
using ContextualProgramming.IO.Internal;
using NUnit.Framework;

namespace ConsoleReadingTests;

public class ReadConsole
{
    private ConsoleReading _reading = null!;


    [SetUp]
    public void SetUp()
    {
        _reading = new(new());
    }

    [Test]
    public void ProvidedActiveKey_RecordsKeyAsInput()
    {
        ConsoleKeyInput keyInput = new(); // Set up.
        ConsoleInput input = new();

        _reading.EvaluateKeyInput(input, keyInput); // TODO :: Possibly rename.

        // Assert input's updated state.
        Assert.Fail();
    }
}

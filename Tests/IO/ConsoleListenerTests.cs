using ContextualProgramming.IO;
using ContextualProgramming.IO.Internal;
using NUnit.Framework;

namespace ConsoleListenerTests;

public class ReadConsole
{
    private ConsoleListener _listener = null!;


    [SetUp]
    public void SetUp()
    {
        _listener = new(new());
    }

    [Test]
    public void ProvidedActiveKey_RecordsKeyAsInput()
    {
        KeyInput keyInput = new(); // Set up.
        ConsoleInput input = new();

        _listener.EvaluateKeyInput(input, keyInput); // TODO :: Possibly rename.

        // Assert input's updated state.
        Assert.Fail();
    }
}

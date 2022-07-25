using ContextualProgramming.IO.Internal;
using NUnit.Framework;

namespace ConsoleDisplayerTests;

public class Displaying
{
    private ConsoleDisplayer _displayer = null!;
    private StringWriter _writer = new();


    [SetUp]
    public void SetUp()
    {
        _displayer = new();
        Console.SetOut(_writer);
    }

    [TearDown]
    public void TearDown()
    {
        Console.OpenStandardOutput();
    }


    [Test]
    public void EmptyLinesAndNoActiveText_DisplaysNothing()
    {
        string expectedText = string.Empty;

        _displayer.UpdateDisplay(new());

        Assert.AreEqual(expectedText, _writer.ToString());
    }
}
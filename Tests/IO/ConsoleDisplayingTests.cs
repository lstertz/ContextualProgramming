using ContextualProgramming.IO;
using ContextualProgramming.IO.Internal;
using NUnit.Framework;
using System.Text;

using TestConsole = ConsoleDisplayingTests.UpdateDisplay.TestableConsoleDisplaying.StringConsole;

namespace ConsoleDisplayingTests;

public class UpdateDisplay
{
    public class TestableConsoleDisplaying : ConsoleDisplaying
    {
        public class StringConsole : IConsole
        {
            private static readonly StringBuilder _stringBuilder = new();


            public static void Clear() => _stringBuilder.Clear();

            public static string GetDisplay() => _stringBuilder.ToString();

            public static void Write(string? value) => _stringBuilder.Append(value);

            public static void WriteLine(string? value) => _stringBuilder.AppendLine(value);
        }

        public override void UpdateDisplay(ConsoleOutput output)
        {
            UpdateDisplay<TestConsole>(output);
        }
    }


    private TestableConsoleDisplaying _displaying = null!;


    [SetUp]
    public void SetUp()
    {
        _displaying = new();
    }

    [TearDown]
    public void TearDown()
    {
        TestConsole.Clear();
    }


    [Test]
    public void EmptyLinesAndNoActiveText_DisplaysNothing()
    {
        string expectedText = string.Empty;

        _displaying.UpdateDisplay(new());

        Assert.AreEqual(expectedText, TestConsole.GetDisplay());
    }

    [Test]
    public void EmptyLinesWithActiveText_DisplaysActiveText()
    {
        string expectedText = "Active Text";

        _displaying.UpdateDisplay(new()
        {
            ActiveText = expectedText
        });

        Assert.AreEqual(expectedText, TestConsole.GetDisplay());
    }

    [Test]
    public void LinesAndNoActiveText_DisplaysLines()
    {
        string line1 = "Line1";
        string line2 = "Line2";
        string expectedText = $"{line1}\r\n{line2}\r\n";

        _displaying.UpdateDisplay(new()
        {
            Lines = new string[] { line1, line2 }
        });

        Assert.AreEqual(expectedText, TestConsole.GetDisplay());
    }

    [Test]
    public void LinesWithActiveText_DisplaysLinesWithActiveText()
    {
        string line1 = "Line1";
        string line2 = "Line2";
        string activeText = "ActiveText";
        string expectedText = $"{line1}\r\n{line2}\r\n{activeText}";

        _displaying.UpdateDisplay(new()
        {
            ActiveText = activeText,
            Lines = new string[] { line1, line2 }
        });

        Assert.AreEqual(expectedText, TestConsole.GetDisplay());
    }


    [Test]
    public void SubsequentEmptyLinesWithNoActiveText_DisplaysNothing()
    {
        string line1 = "Line1";
        string line2 = "Line2";
        string activeText = "ActiveText";
        string expectedText = string.Empty;

        _displaying.UpdateDisplay(new()
        {
            ActiveText = activeText,
            Lines = new string[] { line1, line2 }
        });
        _displaying.UpdateDisplay(new());

        Assert.AreEqual(expectedText, TestConsole.GetDisplay());
    }
}
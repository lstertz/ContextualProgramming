using ContextualProgramming.IO;
using ContextualProgramming.IO.Internal;
using NUnit.Framework;
using System.Text;

using FakeConsole = ConsoleDisplayingTests.TestableConsoleDisplaying.StringConsole;

namespace ConsoleDisplayingTests;

public class TestableConsoleDisplaying : ConsoleDisplaying
{
    public class StringConsole : IConsole
    {
        private static readonly StringBuilder _stringBuilder = new();

        public static int BufferWidth { get; set; } = 5;

        public static bool CanClear { get; set; } = true;

        public static int CursorLeft { get; private set; } = 0;

        public static int CursorTop { get; set; } = 0;

        public static bool CursorVisible { get; set; } = true;

        public static void Reset()
        {
            CursorTop = 0;
            CursorLeft = 0;
            CursorVisible = true;

            CanClear = true;
            _stringBuilder.Clear();
        }

        public static string GetDisplay() => _stringBuilder.ToString();

        public static void SetCursorPosition(int left, int top)
        {
            if (left == 0 && top == 0 && CanClear)
            {
                _stringBuilder.Clear();
                CanClear = false;
            }

            CursorLeft = left;
            CursorTop = top;
        }

        public static void Write(string? value) => _stringBuilder.Append(value);
    }

    public TestableConsoleDisplaying(ConsoleOutput output) : base(output) { }

    public override void UpdateDisplay(ConsoleOutput output) =>
        UpdateDisplay<FakeConsole>(output);
}

public class Constructor
{
    [TearDown]
    public void TearDown()
    {
        FakeConsole.Reset();
    }


    [Test]
    public void AtLengthLinesWithAtLengthActiveText_WrapsByBufferWidth()
    {
        string line1 = "aaaaa";
        string activeText = "bbbbb";
        string expectedText = $"aaaaa\r\nbbbbb\r\n";

        new TestableConsoleDisplaying(new()
        {
            ActiveText = activeText,
            Lines = new string[] { line1 }
        });

        Assert.AreEqual(expectedText, FakeConsole.GetDisplay());
        Assert.AreEqual(0, FakeConsole.CursorLeft);
        Assert.AreEqual(2, FakeConsole.CursorTop);
    }


    [Test]
    public void EmptyLinesAndNoActiveText_DisplaysNothing()
    {
        string expectedText = string.Empty;

        new TestableConsoleDisplaying(new());

        Assert.AreEqual(expectedText, FakeConsole.GetDisplay());
        Assert.AreEqual(0, FakeConsole.CursorLeft);
        Assert.AreEqual(0, FakeConsole.CursorTop);
    }

    [Test]
    public void EmptyLinesWithActiveText_DisplaysActiveText()
    {
        string expectedText = "Text";

        new TestableConsoleDisplaying(new()
        {
            ActiveText = expectedText
        });

        Assert.AreEqual($"{expectedText} \r\n", FakeConsole.GetDisplay());
        Assert.AreEqual(4,FakeConsole.CursorLeft);
        Assert.AreEqual(0, FakeConsole.CursorTop);
    }

    [Test]
    public void LinesAndNoActiveText_DisplaysLines()
    {
        string line1 = "Line1";
        string line2 = "Line2";
        string expectedText = $"{line1}\r\n{line2}\r\n";

        new TestableConsoleDisplaying(new()
        {
            Lines = new string[] { line1, line2 }
        });

        Assert.AreEqual(expectedText, FakeConsole.GetDisplay());
        Assert.AreEqual(0, FakeConsole.CursorLeft);
        Assert.AreEqual(2, FakeConsole.CursorTop);
    }

    [Test]
    public void LinesWithActiveText_DisplaysLinesWithActiveText()
    {
        string line1 = "Line1";
        string line2 = "Line2";
        string activeText = "Text";
        string expectedText = $"{line1}\r\n{line2}\r\n{activeText} \r\n";

        new TestableConsoleDisplaying(new()
        {
            ActiveText = activeText,
            Lines = new string[] { line1, line2 }
        });

        Assert.AreEqual(expectedText, FakeConsole.GetDisplay());
        Assert.AreEqual(4, FakeConsole.CursorLeft);
        Assert.AreEqual(2, FakeConsole.CursorTop);
    }


    [Test]
    public void LongLinesWithLongActiveText_WrapsByBufferWidth()
    {
        string line1 = "LongLine";
        string activeText = "LongText";
        string expectedText = $"LongL\r\nine  \r\nLongT\r\next  \r\n";

        new TestableConsoleDisplaying(new()
        {
            ActiveText = activeText,
            Lines = new string[] { line1 }
        });

        Assert.AreEqual(expectedText, FakeConsole.GetDisplay());
        Assert.AreEqual(3, FakeConsole.CursorLeft);
        Assert.AreEqual(3, FakeConsole.CursorTop);
    }
}


public class UpdateDisplay
{
    private TestableConsoleDisplaying _displaying = null!;


    [SetUp]
    public void SetUp()
    {
        _displaying = new(new());
    }

    [TearDown]
    public void TearDown()
    {
        FakeConsole.Reset();
    }


    [Test]
    public void AtLengthLinesWithAtLengthActiveText_WrapsByBufferWidth()
    {
        string line1 = "aaaaa";
        string activeText = "bbbbb";
        string expectedText = $"aaaaa\r\nbbbbb\r\n";

        _displaying.UpdateDisplay(new()
        {
            ActiveText = activeText,
            Lines = new string[] { line1 }
        });

        Assert.AreEqual(expectedText, FakeConsole.GetDisplay());
        Assert.AreEqual(0, FakeConsole.CursorLeft);
        Assert.AreEqual(2, FakeConsole.CursorTop);
    }


    [Test]
    public void EmptyLinesAndNoActiveText_DisplaysNothing()
    {
        string expectedText = string.Empty;

        _displaying.UpdateDisplay(new());

        Assert.AreEqual(expectedText, FakeConsole.GetDisplay());
        Assert.AreEqual(0, FakeConsole.CursorLeft);
        Assert.AreEqual(0, FakeConsole.CursorTop);
    }

    [Test]
    public void EmptyLinesWithActiveText_DisplaysActiveText()
    {
        string expectedText = "Text";

        _displaying.UpdateDisplay(new()
        {
            ActiveText = expectedText
        });

        Assert.AreEqual($"{expectedText} \r\n", FakeConsole.GetDisplay());
        Assert.AreEqual(4, FakeConsole.CursorLeft);
        Assert.AreEqual(0, FakeConsole.CursorTop);
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

        Assert.AreEqual(expectedText, FakeConsole.GetDisplay());
        Assert.AreEqual(0, FakeConsole.CursorLeft);
        Assert.AreEqual(2, FakeConsole.CursorTop);
    }

    [Test]
    public void LinesWithActiveText_DisplaysLinesWithActiveText()
    {
        string line1 = "Line1";
        string line2 = "Line2";
        string activeText = "Text";
        string expectedText = $"{line1}\r\n{line2}\r\n{activeText} \r\n";

        _displaying.UpdateDisplay(new()
        {
            ActiveText = activeText,
            Lines = new string[] { line1, line2 }
        });

        Assert.AreEqual(expectedText, FakeConsole.GetDisplay());
        Assert.AreEqual(4, FakeConsole.CursorLeft);
        Assert.AreEqual(2, FakeConsole.CursorTop);
    }


    [Test]
    public void LongLinesWithLongActiveText_WrapsByBufferWidth()
    {
        string line1 = "LongLine";
        string activeText = "LongText";
        string expectedText = $"LongL\r\nine  \r\nLongT\r\next  \r\n";

        _displaying.UpdateDisplay(new()
        {
            ActiveText = activeText,
            Lines = new string[] { line1 }
        });

        Assert.AreEqual(expectedText, FakeConsole.GetDisplay());
        Assert.AreEqual(3, FakeConsole.CursorLeft);
        Assert.AreEqual(3, FakeConsole.CursorTop);
    }


    [Test]
    public void SubsequentEmptyLinesWithActiveText_DisplaysActiveTextWithClearingLines()
    {
        string line1 = "Line1";
        string line2 = "Line2";
        string activeText1 = "Text";
        string activeText2 = "Text";
        string bufferClearLine = "     \r\n";
        string expectedText = $"{activeText2} \r\n{bufferClearLine}{bufferClearLine}";

        _displaying.UpdateDisplay(new()
        {
            ActiveText = activeText1,
            Lines = new string[] { line1, line2 }
        });
        FakeConsole.CanClear = true;
        _displaying.UpdateDisplay(new()
        {
            ActiveText = activeText2
        });

        Assert.AreEqual(expectedText, FakeConsole.GetDisplay());
        Assert.AreEqual(4, FakeConsole.CursorLeft);
        Assert.AreEqual(0, FakeConsole.CursorTop);
    }

    [Test]
    public void SubsequentEmptyLinesWithNoActiveText_DisplaysClearingLines()
    {
        string line1 = "Line1";
        string line2 = "Line2";
        string activeText = "Text";
        string bufferClearLine = "     \r\n";
        string expectedText = $"{bufferClearLine}{bufferClearLine}{bufferClearLine}";

        _displaying.UpdateDisplay(new()
        {
            ActiveText = activeText,
            Lines = new string[] { line1, line2 }
        });
        FakeConsole.CanClear = true;
        _displaying.UpdateDisplay(new());

        Assert.AreEqual(expectedText, FakeConsole.GetDisplay());
        Assert.AreEqual(0, FakeConsole.CursorLeft);
        Assert.AreEqual(0, FakeConsole.CursorTop);
    }

    [Test]
    public void SubsequentLinesWithActiveText_DisplaysLinesWithActiveTextWithClearingLines()
    {
        string line1 = "Line1";
        string line2 = "Line2";
        string line3 = "Line3";
        string activeText1 = "Text";
        string activeText2 = "Text";
        string bufferClearLine = "     \r\n";
        string expectedText = $"{line3}\r\n{activeText2} \r\n{bufferClearLine}";

        _displaying.UpdateDisplay(new()
        {
            ActiveText = activeText1,
            Lines = new string[] { line1, line2 }
        });
        FakeConsole.CanClear = true;
        _displaying.UpdateDisplay(new()
        {
            ActiveText = activeText2,
            Lines = new string[] { line3 }
        });

        Assert.AreEqual(expectedText, FakeConsole.GetDisplay());
        Assert.AreEqual(4, FakeConsole.CursorLeft);
        Assert.AreEqual(1, FakeConsole.CursorTop);
    }

    [Test]
    public void SubsequentLinesWithNoActiveText_DisplaysLinesWithClearingLines()
    {
        string line1 = "Line1";
        string line2 = "Line2";
        string line3 = "Line3";
        string activeText = "Text";
        string bufferClearLine = "     \r\n";
        string expectedText = $"{line3}\r\n{bufferClearLine}{bufferClearLine}";

        _displaying.UpdateDisplay(new()
        {
            ActiveText = activeText,
            Lines = new string[] { line1, line2 }
        });
        FakeConsole.CanClear = true;
        _displaying.UpdateDisplay(new()
        {
            Lines = new string[] { line3 }
        });

        Assert.AreEqual(expectedText, FakeConsole.GetDisplay());
        Assert.AreEqual(0, FakeConsole.CursorLeft);
        Assert.AreEqual(1, FakeConsole.CursorTop);
    }

    [Test]
    public void SubsequentSubsequentLinesWithActiveText_DisplaysLinesWithActiveText()
    {
        string line1 = "Line1";
        string line2 = "Line2";
        string line3 = "Line3";
        string activeText1 = "Text";
        string activeText2 = "Text";
        string expectedText = $"{line3}\r\n{activeText2} \r\n";

        _displaying.UpdateDisplay(new()
        {
            ActiveText = activeText1,
            Lines = new string[] { line1, line2 }
        });
        FakeConsole.CanClear = true;
        _displaying.UpdateDisplay(new()
        {
            ActiveText = activeText2,
            Lines = new string[] { line3 }
        });
        FakeConsole.CanClear = true;
        _displaying.UpdateDisplay(new()
        {
            ActiveText = activeText2,
            Lines = new string[] { line3 }
        });

        Assert.AreEqual(expectedText, FakeConsole.GetDisplay());
        Assert.AreEqual(4, FakeConsole.CursorLeft);
        Assert.AreEqual(1, FakeConsole.CursorTop);
    }
}
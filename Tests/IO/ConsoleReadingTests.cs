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
    public void ProvidedPressedAltedCtrledKey_RecordsNothing()
    {
        char expectedKey = 'a';

        ConsoleInput input = new();
        ConsoleKeyInput keyInput = new();
        keyInput.PressedKeys.Add(new ConsoleKeyInfo(expectedKey, ConsoleKey.A,
            false, true, true));

        _reading.ReadKeyInput(input, keyInput);

        Assert.IsEmpty(input.Submitted.Elements);
        Assert.AreEqual(string.Empty, input.Unsubmitted.Value);
    }

    [Test]
    public void ProvidedPressedAltedKey_RecordsNothing()
    {
        char expectedKey = 'a';

        ConsoleInput input = new();
        ConsoleKeyInput keyInput = new();
        keyInput.PressedKeys.Add(new ConsoleKeyInfo(expectedKey, ConsoleKey.A,
            false, true, false));

        _reading.ReadKeyInput(input, keyInput);

        Assert.IsEmpty(input.Submitted.Elements);
        Assert.AreEqual(string.Empty, input.Unsubmitted.Value);
    }

    [Test]
    public void ProvidedPressedBackspaceKey_RemovesLastKeyFromUnsubmitted()
    {
        ConsoleInput input = new()
        {
            Unsubmitted = "a"
        };
        ConsoleKeyInput keyInput = new();
        keyInput.PressedKeys.Add(new ConsoleKeyInfo('\b', ConsoleKey.Backspace,
            false, false, false));

        _reading.ReadKeyInput(input, keyInput);

        Assert.IsEmpty(input.Submitted.Elements);
        Assert.AreEqual(string.Empty, input.Unsubmitted.Value);
    }

    [Test]
    public void ProvidedPressedBackspaceKeyForEmptyUnsubmitted_DoesNothing()
    {
        ConsoleInput input = new();
        ConsoleKeyInput keyInput = new();
        keyInput.PressedKeys.Add(new ConsoleKeyInfo('\b', ConsoleKey.Backspace,
            false, false, false));

        _reading.ReadKeyInput(input, keyInput);

        Assert.IsEmpty(input.Submitted.Elements);
        Assert.AreEqual(string.Empty, input.Unsubmitted.Value);
    }

    [Test]
    public void ProvidedPressedCtrledKey_RecordsNothing()
    {
        char expectedKey = 'a';

        ConsoleInput input = new();
        ConsoleKeyInput keyInput = new();
        keyInput.PressedKeys.Add(new ConsoleKeyInfo(expectedKey, ConsoleKey.A,
            false, false, true));

        _reading.ReadKeyInput(input, keyInput);

        Assert.IsEmpty(input.Submitted.Elements);
        Assert.AreEqual(string.Empty, input.Unsubmitted.Value);
    }

    [Test]
    public void ProvidedPressedDeleteKey_RemovesLastKeyFromUnsubmitted()
    {
        ConsoleInput input = new()
        {
            Unsubmitted = "a"
        };
        ConsoleKeyInput keyInput = new();
        keyInput.PressedKeys.Add(new ConsoleKeyInfo('\b', ConsoleKey.Delete,
            false, false, false));

        _reading.ReadKeyInput(input, keyInput);

        Assert.IsEmpty(input.Submitted.Elements);
        Assert.AreEqual(string.Empty, input.Unsubmitted.Value);
    }

    [Test]
    public void ProvidedPressedDeleteKeyForEmptyUnsubmitted_DoesNothing()
    {
        ConsoleInput input = new();
        ConsoleKeyInput keyInput = new();
        keyInput.PressedKeys.Add(new ConsoleKeyInfo('\b', ConsoleKey.Delete,
            false, false, false));

        _reading.ReadKeyInput(input, keyInput);

        Assert.IsEmpty(input.Submitted.Elements);
        Assert.AreEqual(string.Empty, input.Unsubmitted.Value);
    }

    [Test]
    public void ProvidedPressedEnterKey_SubmitsUnsubmitted()
    {
        string expectedText = "a";
        ConsoleInput input = new()
        {
            Unsubmitted = expectedText
        };
        ConsoleKeyInput keyInput = new();
        keyInput.PressedKeys.Add(new ConsoleKeyInfo('\n', ConsoleKey.Enter,
            false, false, false));

        _reading.ReadKeyInput(input, keyInput);

        Assert.AreEqual(1, input.Submitted.Count);
        Assert.AreEqual(expectedText, input.Submitted[0]);
        Assert.AreEqual(string.Empty, input.Unsubmitted.Value);
    }

    [Test]
    public void ProvidedPressedEnterKeyForEmptyUnsubmitted_SubmitsEmptyLine()
    {
        ConsoleInput input = new();
        ConsoleKeyInput keyInput = new();
        keyInput.PressedKeys.Add(new ConsoleKeyInfo('\n', ConsoleKey.Enter,
            false, false, false));

        _reading.ReadKeyInput(input, keyInput);

        Assert.AreEqual(1, input.Submitted.Count);
        Assert.AreEqual(string.Empty, input.Submitted[0]);
        Assert.AreEqual(string.Empty, input.Unsubmitted.Value);
    }

    [Test]
    public void ProvidedPressedKey_RecordsKeyAsUnsubmitted()
    {
        char expectedKey = 'a';

        ConsoleInput input = new();
        ConsoleKeyInput keyInput = new();
        keyInput.PressedKeys.Add(new ConsoleKeyInfo(expectedKey, ConsoleKey.A,
            false, false, false));

        _reading.ReadKeyInput(input, keyInput);

        Assert.IsEmpty(input.Submitted.Elements);
        Assert.AreEqual(expectedKey.ToString(), input.Unsubmitted.Value);
    }

    [Test]
    public void ProvidedPressedKeys_RecordLastKeyAsUnsubmitted()
    {
        char expectedKey1 = 'a';
        char expectedKey2 = 'b';

        ConsoleInput input = new();
        ConsoleKeyInput keyInput = new();
        keyInput.PressedKeys.Add(new ConsoleKeyInfo(expectedKey1, ConsoleKey.A,
            false, false, false));
        keyInput.PressedKeys.Add(new ConsoleKeyInfo(expectedKey2, ConsoleKey.B,
            false, false, false));

        _reading.ReadKeyInput(input, keyInput);

        Assert.IsEmpty(input.Submitted.Elements);
        Assert.AreEqual(expectedKey2.ToString(), input.Unsubmitted.Value);
    }

    [Test]
    public void ProvidedPressedShiftedKey_RecordsShiftedKeyAsUnsubmitted()
    {
        char expectedKey = 'A';

        ConsoleInput input = new();
        ConsoleKeyInput keyInput = new();
        keyInput.PressedKeys.Add(new ConsoleKeyInfo(expectedKey, ConsoleKey.A,
            true, false, false));

        _reading.ReadKeyInput(input, keyInput);

        Assert.IsEmpty(input.Submitted.Elements);
        Assert.AreEqual(expectedKey.ToString(), input.Unsubmitted.Value);
    }

    [Test]
    public void ProvidedReleasedKeyInput_RecordsNothing()
    {
        char expectedKey = 'a';

        ConsoleInput input = new();
        ConsoleKeyInput keyInput = new();
        keyInput.ReleasedKeys.Add(new ConsoleKeyInfo(expectedKey, ConsoleKey.A,
            false, false, false));

        _reading.ReadKeyInput(input, keyInput);

        Assert.IsEmpty(input.Submitted.Elements);
        Assert.AreEqual(string.Empty, input.Unsubmitted.Value);
    }

    [Test]
    public void ProvidedEmptyKeyInput_RecordsNothing()
    {
        ConsoleInput input = new();
        ConsoleKeyInput keyInput = new();

        _reading.ReadKeyInput(input, keyInput);

        Assert.IsEmpty(input.Submitted.Elements);
        Assert.AreEqual(string.Empty, input.Unsubmitted.Value);
    }
}

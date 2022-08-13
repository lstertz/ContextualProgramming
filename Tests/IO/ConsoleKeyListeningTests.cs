using ContextualProgramming.IO;
using ContextualProgramming.IO.Internal;
using NUnit.Framework;

using TestConsole = ConsoleKeyListeningTests.TestableConsoleKeyListening.MockConsole;
using TestInterceptConsole = ConsoleKeyListeningTests
    .TestInterceptConsoleKeyListening.InterceptConsole;

namespace ConsoleKeyListeningTests;

public class TestInterceptConsoleKeyListening : ConsoleKeyListening
{
    public class InterceptConsole : IConsole
    {
        public static bool ReadToIntercept = false;

        private static bool HasAvailable = true;

        public static bool KeyAvailable => HasAvailable;
        public static bool IsKeyPressed(ConsoleKeyInfo keyInfo) => false;
        public static ConsoleKeyInfo ReadKey(bool intercept)
        {
            HasAvailable = false;
            ReadToIntercept = intercept;

            return new ConsoleKeyInfo();
        }

        public static void Reset()
        {
            HasAvailable = true;
            ReadToIntercept = false;
        }
    }

    public override void ReadKeyInput(ConsoleKeyInput keyInput) =>
        ReadKeyInput<TestInterceptConsole>(keyInput);
}

public class TestableConsoleKeyListening : ConsoleKeyListening
{
    public class MockConsole : IConsole
    {
        public static Queue<ConsoleKeyInfo> AvailablePressedKeys = new();
        public static HashSet<ConsoleKeyInfo> PressedKeys = new();

        public static bool KeyAvailable => AvailablePressedKeys.Count > 0;
        public static bool IsKeyPressed(ConsoleKeyInfo keyInfo) => PressedKeys.Contains(keyInfo);
        public static ConsoleKeyInfo ReadKey(bool intercept) => AvailablePressedKeys.Dequeue();

        public static void Reset()
        {
            AvailablePressedKeys.Clear();
            PressedKeys.Clear();
        }
    }

    public override void ReadKeyInput(ConsoleKeyInput keyInput) =>
        ReadKeyInput<TestConsole>(keyInput);
}

public class ReadKeyInput
{
    private static readonly ConsoleKeyInfo TestKeyA = new('a', ConsoleKey.A, false, false, false);
    private static readonly ConsoleKeyInfo TestKeyB = new('b', ConsoleKey.B, false, false, false);
    private static readonly ConsoleKeyInfo TestKeyC = new('c', ConsoleKey.C, false, false, false);
    private static readonly ConsoleKeyInfo TestKeyD = new('d', ConsoleKey.D, false, false, false);

    private TestableConsoleKeyListening _listening = null!;


    [SetUp]
    public void SetUp()
    {
        _listening = new();
    }

    [TearDown]
    public void TearDown()
    {
        TestConsole.Reset();
    }

    [Test]
    public void ChangesPressedKey_AfterConsecutivePresses_ResetsPressedTicks()
    {
        ConsoleKeyInput keyInput = new();
        TestConsole.AvailablePressedKeys.Enqueue(TestKeyA);
        TestConsole.PressedKeys.Add(TestKeyA);

        _listening.ReadKeyInput(keyInput);

        // Trigger KeyAvailable again.
        TestConsole.AvailablePressedKeys.Enqueue(TestKeyA);

        _listening.ReadKeyInput(keyInput);

        TestConsole.PressedKeys.Remove(TestKeyA);
        TestConsole.AvailablePressedKeys.Enqueue(TestKeyB);
        TestConsole.PressedKeys.Add(TestKeyB);

        _listening.ReadKeyInput(keyInput);

        Assert.AreEqual(1, keyInput.PressedKeys.Count);
        Assert.Contains(TestKeyB, keyInput.PressedKeys.Elements);

        Assert.AreEqual(1, keyInput.ReleasedKeys.Count);
        Assert.Contains(TestKeyA, keyInput.ReleasedKeys.Elements);

        Assert.AreEqual(0, keyInput.PressedTicks.Value);
    }

    [Test]
    public void ChangesPressedKey_KeyInputRecordsNewPressedKeyAndReleasedKey()
    {
        ConsoleKeyInput keyInput = new();
        TestConsole.AvailablePressedKeys.Enqueue(TestKeyA);

        _listening.ReadKeyInput(keyInput);

        TestConsole.AvailablePressedKeys.Enqueue(TestKeyB);

        _listening.ReadKeyInput(keyInput);

        Assert.AreEqual(1, keyInput.PressedKeys.Count);
        Assert.Contains(TestKeyB, keyInput.PressedKeys.Elements);

        Assert.AreEqual(1, keyInput.ReleasedKeys.Count);
        Assert.Contains(TestKeyA, keyInput.ReleasedKeys.Elements);

        Assert.AreEqual(0, keyInput.PressedTicks.Value);
    }

    [Test]
    public void ChangesPressedKeys_KeyInputRecordsNewPressedKeysAndReleasedKeys()
    {
        ConsoleKeyInput keyInput = new();
        TestConsole.AvailablePressedKeys.Enqueue(TestKeyA);
        TestConsole.AvailablePressedKeys.Enqueue(TestKeyB);

        _listening.ReadKeyInput(keyInput);

        TestConsole.AvailablePressedKeys.Enqueue(TestKeyC);
        TestConsole.AvailablePressedKeys.Enqueue(TestKeyD);

        _listening.ReadKeyInput(keyInput);

        Assert.AreEqual(2, keyInput.PressedKeys.Count);
        Assert.Contains(TestKeyC, keyInput.PressedKeys.Elements);
        Assert.Contains(TestKeyD, keyInput.PressedKeys.Elements);

        Assert.AreEqual(2, keyInput.ReleasedKeys.Count);
        Assert.Contains(TestKeyA, keyInput.ReleasedKeys.Elements);
        Assert.Contains(TestKeyB, keyInput.ReleasedKeys.Elements);

        Assert.AreEqual(0, keyInput.PressedTicks.Value);
    }

    [Test]
    public void PressesKey_KeyInputRecordsPressedKey()
    {
        ConsoleKeyInput keyInput = new();
        TestConsole.AvailablePressedKeys.Enqueue(TestKeyA);

        _listening.ReadKeyInput(keyInput);

        Assert.AreEqual(1, keyInput.PressedKeys.Count);
        Assert.Contains(TestKeyA, keyInput.PressedKeys.Elements);

        Assert.AreEqual(0, keyInput.ReleasedKeys.Count);
        Assert.AreEqual(0, keyInput.PressedTicks.Value);
    }

    [Test]
    public void PressesKeyConsecutively_KeyInputRecordsPressedKeyWithTickIncrease()
    {
        ConsoleKeyInput keyInput = new();
        TestConsole.AvailablePressedKeys.Enqueue(TestKeyA);
        TestConsole.PressedKeys.Add(TestKeyA);

        _listening.ReadKeyInput(keyInput);

        // Trigger KeyAvailable again.
        TestConsole.AvailablePressedKeys.Enqueue(TestKeyA);

        _listening.ReadKeyInput(keyInput);

        Assert.AreEqual(1, keyInput.PressedKeys.Count);
        Assert.Contains(TestKeyA, keyInput.PressedKeys.Elements);

        Assert.AreEqual(1, keyInput.PressedTicks.Value);

        Assert.AreEqual(0, keyInput.ReleasedKeys.Count);
    }

    [Test]
    public void PressesKeys_KeyInputRecordsPressedKeys()
    {
        ConsoleKeyInput keyInput = new();
        TestConsole.AvailablePressedKeys.Enqueue(TestKeyA);
        TestConsole.AvailablePressedKeys.Enqueue(TestKeyB);

        _listening.ReadKeyInput(keyInput);

        Assert.AreEqual(2, keyInput.PressedKeys.Count);
        Assert.Contains(TestKeyA, keyInput.PressedKeys.Elements);
        Assert.Contains(TestKeyB, keyInput.PressedKeys.Elements);

        Assert.AreEqual(0, keyInput.ReleasedKeys.Count);
        Assert.AreEqual(0, keyInput.PressedTicks.Value);
    }

    [Test]
    public void PressesKeysConsecutively_KeyInputRecordsPressedKeysWithTickIncrease()
    {
        ConsoleKeyInput keyInput = new();
        TestConsole.AvailablePressedKeys.Enqueue(TestKeyA);
        TestConsole.AvailablePressedKeys.Enqueue(TestKeyB);
        TestConsole.PressedKeys.Add(TestKeyA);
        TestConsole.PressedKeys.Add(TestKeyB);

        _listening.ReadKeyInput(keyInput);

        // Trigger KeyAvailable again.
        TestConsole.AvailablePressedKeys.Enqueue(TestKeyA);
        TestConsole.AvailablePressedKeys.Enqueue(TestKeyB);

        _listening.ReadKeyInput(keyInput);

        Assert.AreEqual(2, keyInput.PressedKeys.Count);
        Assert.Contains(TestKeyA, keyInput.PressedKeys.Elements);
        Assert.Contains(TestKeyB, keyInput.PressedKeys.Elements);

        Assert.AreEqual(1, keyInput.PressedTicks.Value);

        Assert.AreEqual(0, keyInput.ReleasedKeys.Count);
    }

    [Test]
    public void PressesNewKey_AfterConsecutiveKeyPressed_KeyInputRecordsPressedKeysAndResetsTicks()
    {
        ConsoleKeyInput keyInput = new();
        TestConsole.AvailablePressedKeys.Enqueue(TestKeyA);
        TestConsole.PressedKeys.Add(TestKeyA);

        _listening.ReadKeyInput(keyInput);

        // Trigger KeyAvailable again.
        TestConsole.AvailablePressedKeys.Enqueue(TestKeyA);

        _listening.ReadKeyInput(keyInput);

        // Trigger KeyAvailable again with new pressed key.
        TestConsole.AvailablePressedKeys.Enqueue(TestKeyA);
        TestConsole.AvailablePressedKeys.Enqueue(TestKeyB);
        TestConsole.PressedKeys.Add(TestKeyB);

        _listening.ReadKeyInput(keyInput);

        Assert.AreEqual(2, keyInput.PressedKeys.Count);
        Assert.Contains(TestKeyA, keyInput.PressedKeys.Elements);
        Assert.Contains(TestKeyB, keyInput.PressedKeys.Elements);

        Assert.AreEqual(0, keyInput.ReleasedKeys.Count);
        Assert.AreEqual(0, keyInput.PressedTicks.Value);
    }

    [Test]
    public void ProvidesNoInput_AfterPressedKey_KeyInputRecordsReleasedKey()
    {
        ConsoleKeyInput keyInput = new();
        TestConsole.AvailablePressedKeys.Enqueue(TestKeyA);

        _listening.ReadKeyInput(keyInput);
        _listening.ReadKeyInput(keyInput);

        Assert.AreEqual(1, keyInput.ReleasedKeys.Count);
        Assert.Contains(TestKeyA, keyInput.ReleasedKeys.Elements);

        Assert.AreEqual(0, keyInput.PressedKeys.Count);
        Assert.AreEqual(0, keyInput.PressedTicks.Value);
    }

    [Test]
    public void ProvidesNoInput_AfterPressedKeys_KeyInputRecordsReleasedKeys()
    {
        ConsoleKeyInput keyInput = new();
        TestConsole.AvailablePressedKeys.Enqueue(TestKeyA);
        TestConsole.AvailablePressedKeys.Enqueue(TestKeyB);

        _listening.ReadKeyInput(keyInput);
        _listening.ReadKeyInput(keyInput);

        Assert.AreEqual(2, keyInput.ReleasedKeys.Count);
        Assert.Contains(TestKeyA, keyInput.ReleasedKeys.Elements);
        Assert.Contains(TestKeyB, keyInput.ReleasedKeys.Elements);

        Assert.AreEqual(0, keyInput.PressedKeys.Count);
        Assert.AreEqual(0, keyInput.PressedTicks.Value);
    }

    [Test]
    public void ProvidesNoInput_AfterPressedKeysConsecutively_PressedTicksResets()
    {
        ConsoleKeyInput keyInput = new();
        TestConsole.AvailablePressedKeys.Enqueue(TestKeyA);
        TestConsole.AvailablePressedKeys.Enqueue(TestKeyB);
        TestConsole.PressedKeys.Add(TestKeyA);
        TestConsole.PressedKeys.Add(TestKeyB);

        _listening.ReadKeyInput(keyInput);

        // Trigger KeyAvailable again.
        TestConsole.AvailablePressedKeys.Enqueue(TestKeyA);
        TestConsole.AvailablePressedKeys.Enqueue(TestKeyB);

        _listening.ReadKeyInput(keyInput);

        TestConsole.PressedKeys.Clear();

        _listening.ReadKeyInput(keyInput);

        Assert.AreEqual(2, keyInput.ReleasedKeys.Count);
        Assert.Contains(TestKeyA, keyInput.ReleasedKeys.Elements);
        Assert.Contains(TestKeyB, keyInput.ReleasedKeys.Elements);

        Assert.AreEqual(0, keyInput.PressedKeys.Count);
        Assert.AreEqual(0, keyInput.PressedTicks.Value);
    }

    [Test]
    public void ProvidesNoInput_AfterReleasedKey_HasNoRecordedKeys()
    {
        ConsoleKeyInput keyInput = new();
        TestConsole.AvailablePressedKeys.Enqueue(TestKeyA);

        _listening.ReadKeyInput(keyInput);
        _listening.ReadKeyInput(keyInput);
        _listening.ReadKeyInput(keyInput);

        Assert.AreEqual(0, keyInput.PressedKeys.Count);
        Assert.AreEqual(0, keyInput.ReleasedKeys.Count);
        Assert.AreEqual(0, keyInput.PressedTicks.Value);
    }

    [Test]
    public void ProvidesNoInput_AfterReleasedKeys_HasNoRecordedKeys()
    {
        ConsoleKeyInput keyInput = new();
        TestConsole.AvailablePressedKeys.Enqueue(TestKeyA);
        TestConsole.AvailablePressedKeys.Enqueue(TestKeyB);

        _listening.ReadKeyInput(keyInput);
        _listening.ReadKeyInput(keyInput);
        _listening.ReadKeyInput(keyInput);

        Assert.AreEqual(0, keyInput.PressedKeys.Count);
        Assert.AreEqual(0, keyInput.ReleasedKeys.Count);
        Assert.AreEqual(0, keyInput.PressedTicks.Value);
    }

    [Test]
    public void ProvidesNoInput_HasNoRecordedKeys()
    {
        ConsoleKeyInput keyInput = new();

        _listening.ReadKeyInput(keyInput);

        Assert.AreEqual(0, keyInput.PressedKeys.Count);
        Assert.AreEqual(0, keyInput.ReleasedKeys.Count);
        Assert.AreEqual(0, keyInput.PressedTicks.Value);
    }

    [Test]
    public void ProvidesNoInputForKey_AfterPressedKeys_KeyInputUpdatesWithReleasedKey()
    {
        ConsoleKeyInput keyInput = new();
        TestConsole.AvailablePressedKeys.Enqueue(TestKeyA);
        TestConsole.PressedKeys.Add(TestKeyA);
        TestConsole.AvailablePressedKeys.Enqueue(TestKeyB);

        _listening.ReadKeyInput(keyInput);
        _listening.ReadKeyInput(keyInput);

        Assert.AreEqual(1, keyInput.PressedKeys.Count);
        Assert.Contains(TestKeyA, keyInput.PressedKeys.Elements);

        Assert.AreEqual(1, keyInput.ReleasedKeys.Count);
        Assert.Contains(TestKeyB, keyInput.ReleasedKeys.Elements);

        Assert.AreEqual(0, keyInput.PressedTicks.Value);
    }

    [Test]
    public void ProvidesNoInputForKey_AfterPressedKeysAndReleasedKey_KeyInputRecordsPressedKeys()
    {
        ConsoleKeyInput keyInput = new();
        TestConsole.AvailablePressedKeys.Enqueue(TestKeyA);
        TestConsole.PressedKeys.Add(TestKeyA);
        TestConsole.AvailablePressedKeys.Enqueue(TestKeyB);

        _listening.ReadKeyInput(keyInput);
        _listening.ReadKeyInput(keyInput);
        _listening.ReadKeyInput(keyInput);

        Assert.AreEqual(1, keyInput.PressedKeys.Count);
        Assert.Contains(TestKeyA, keyInput.PressedKeys.Elements);

        Assert.AreEqual(0, keyInput.ReleasedKeys.Count);
        Assert.AreEqual(0, keyInput.PressedTicks.Value);
    }

    [Test]
    public void ReadKey_InterceptsInput()
    {
        TestInterceptConsoleKeyListening listening = new();

        listening.ReadKeyInput(new());

        Assert.IsTrue(TestInterceptConsole.ReadToIntercept);

        TestInterceptConsole.Reset();
    }
}
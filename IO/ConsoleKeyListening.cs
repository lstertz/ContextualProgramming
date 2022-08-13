using ContextualProgramming.Internal;


namespace ContextualProgramming.IO.Internal;

/// <summary>
/// Reads and records console key input.
/// </summary>
[Behavior]
[Dependency<ConsoleKeyInput>(Binding.Unique, Fulfillment.Existing, KeyInput)]
public class ConsoleKeyListening
{
    private const string KeyInput = "keyInput";


    /// <summary>
    /// Defines a console whose static functions will serve as the means 
    /// to receive key input.
    /// </summary>
    protected interface IConsole
    {
        /// <summary>
        /// Whether the console has a key available to be read from the buffer.
        /// </summary>
        public static abstract bool KeyAvailable { get; }


        /// <summary>
        /// Whether the specified key is currently pressed.
        /// </summary>
        /// <param name="keyInfo">The info specifying the key to be checked.</param>
        /// <returns>Whether the specific key is currently pressed.</returns>
        public static abstract bool IsKeyPressed(ConsoleKeyInfo keyInfo);

        /// <summary>
        /// Reads the available key from the console buffer.
        /// </summary>
        /// <param name="intercept">Whether the key should be intercepted, 
        /// preventing it from propagating to the console on its own.</param>
        public static abstract ConsoleKeyInfo ReadKey(bool intercept);
    }

    /// <summary>
    /// Wrapper for <see cref="System.Console"/>.
    /// </summary>
    private class Console : IConsole
    {
        /// <summary>
        /// The bit flag indicating the part of a key state denoting whether a key is pressed.
        /// </summary>
        private const int KeyPressed = 0x8000;


        /// <summary>
        /// <see cref="System.Console.KeyAvailable"/>
        /// </summary>
        public static bool KeyAvailable => System.Console.KeyAvailable;


        /// <inheritdoc/>
        public static bool IsKeyPressed(ConsoleKeyInfo keyInfo) =>
            (GetKeyState((int)keyInfo.Key) & KeyPressed) != 0;

        /// <summary>
        /// <see cref="System.Console.ReadKey(bool)"/>
        /// </summary>
        public static ConsoleKeyInfo ReadKey(bool intercept) => System.Console.ReadKey(intercept);


        /// <summary>
        /// Gets the key state of a key.
        /// </summary>
        /// <remarks>
        /// http://msdn.microsoft.com/en-us/library/ms646301%28v=VS.85%29.aspx
        /// </remarks>
        /// <param name="key">Virtual key code for the key to be checked.</param>
        /// <returns>The state of the key.</returns>
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern short GetKeyState(int key);
    }


    /// <summary>
    /// <see cref="ReadKeyInput(ConsoleKeyInput)"/>
    /// </summary>
    /// <typeparam name="TConsole">The static console that will provide 
    /// the key input.</typeparam>
    protected static void ReadKeyInput<TConsole>(ConsoleKeyInput keyInput)
        where TConsole : IConsole
    {
        keyInput.ReleasedKeys.Clear();

        if (TConsole.KeyAvailable && keyInput.PressedKeys.Count > 0)
            keyInput.PressedTicks.Value++;

        for (int c = keyInput.PressedKeys.Count - 1; c >= 0; c--)
            if (!TConsole.IsKeyPressed(keyInput.PressedKeys[c]))
            {
                keyInput.ReleasedKeys.Add(keyInput.PressedKeys[c]);
                keyInput.PressedKeys.RemoveAt(c);
                keyInput.PressedTicks.Value = 0;
            }

        while (TConsole.KeyAvailable)
        {
            bool skip = false;

            // TODO :: StateList Contains method.
            // TODO :: Test in project for actual functionality.
            ConsoleKeyInfo keyInfo = TConsole.ReadKey(true);
            for (int c = 0, count = keyInput.PressedKeys.Count; c < count; c++)
                if (keyInput.PressedKeys[c] == keyInfo)
                {
                    skip = true;
                    break;
                }

            if (skip)
                continue;

            keyInput.PressedKeys.Add(keyInfo);
            keyInput.PressedTicks.Value = 0;

        }
    }


    /// <summary>
    /// Reads any available key input from the console and updates the 
    /// provided <see cref="ConsoleKeyInput"/> to reflect that input.
    /// </summary>
    /// <param name="keyInput">The record of key input.</param>
    [Operation]
    [OnUpdate]
    public virtual void ReadKeyInput(ConsoleKeyInput keyInput) =>
        ReadKeyInput<Console>(keyInput);
}
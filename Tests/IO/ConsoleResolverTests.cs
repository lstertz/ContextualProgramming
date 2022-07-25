using ContextualProgramming.IO;
using ContextualProgramming.IO.Internal;
using NUnit.Framework;

namespace ConsoleResolverTests;

public class Resolving
{
    private ConsoleInput? _input;
    private ConsoleOutput? _output;

    private ConsoleResolver? _resolver;


    [SetUp]
    public void SetUp()
    {
        _input = new();
        _output = new();

        _resolver = new();
    }

    [Test]
    public void PostTeardown_UponInput_DoesNotClearInputTextLines()
    {
        Assert.Fail();
    }
}

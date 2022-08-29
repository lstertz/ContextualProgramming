using ContextualProgramming.IO;
using ContextualProgramming.IO.Internal;
using NUnit.Framework;

namespace ConsoleResolvingTests;

public class Construction
{
    private ConsoleInput _input = null!;
    private ConsoleOutput _output = null!;


    [SetUp]
    public void SetUp()
    {
        _input = new();
        _output = new();
    }

    [Test]
    public void InputExistingSubmissions_DoNotAlterOutputLines()
    {
        string expected = "Pre-existing";

        _output.Lines.Add(expected);
        _input.Submitted.Add("Test");

        _ = new ConsoleResolving(_input, _output);

        Assert.AreEqual(1, _output.Lines.Count);
        Assert.AreEqual(expected, _output.Lines[0]);
    }

    [Test]
    public void InputExistingSubmissions_NotCopiedToOutputLines()
    {
        _input.Submitted.Add("Test");

        _ = new ConsoleResolving(_input, _output);

        Assert.IsEmpty(_output.Lines.Elements);
    }

    [Test]
    public void InputUnsubmitted_CopiedToOutputActiveText()
    {
        string expected = "Test";

        _input.Unsubmitted.Value = expected;

        _ = new ConsoleResolving(_input, _output);

        Assert.AreEqual(expected, _output.ActiveText.Value);
    }
}

public class ResolveInput
{
    private ConsoleInput _input = null!;
    private ConsoleOutput _output = null!;

    private ConsoleResolving _resolving = null!;


    [SetUp]
    public void SetUp()
    {
        _input = new();
        _output = new();

        _resolving = new(_input, _output);
    }

    [Test]
    public void InputNewSubmissions_CopiedToOutputLines()
    {
        string expected1 = "Test1";
        string expected2 = "Test2";

        _input.Submitted.Add(expected1);
        _input.Submitted.Add(expected2);

        _resolving.ResolveInput(_input, _output);

        Assert.AreEqual(2, _output.Lines.Count);
        Assert.AreEqual(expected1, _output.Lines[0]);
        Assert.AreEqual(expected2, _output.Lines[1]);
    }

    [Test]
    public void InputNewSubmissions_DoNotOverrideExistingOutputLines()
    {
        string expected1 = "Test1";
        string expected2 = "Test2";

        _output.Lines.Add(expected1);
        _input.Submitted.Add(expected2);

        _resolving.ResolveInput(_input, _output);

        Assert.AreEqual(2, _output.Lines.Count);
        Assert.AreEqual(expected1, _output.Lines[0]);
        Assert.AreEqual(expected2, _output.Lines[1]);
    }

    [Test]
    public void InputNewSubmissionsAfterConstructionSubmissions_CopiedToOutputLines()
    {
        string expected1 = "Test1";
        string expected2 = "Test2";

        _input.Submitted.Add(expected1);
        ConsoleResolving resolving = new(_input, _output);
        _input.Submitted.Add(expected2);

        resolving.ResolveInput(_input, _output);

        Assert.AreEqual(1, _output.Lines.Count);
        Assert.AreEqual(expected2, _output.Lines[0]);
    }

    [Test]
    public void InputNewSubmissionsAfterRemovedSubmissions_CopiedToOutputLines()
    {
        string expected1 = "Test1";
        string expected2 = "Test2";
        string expected3 = "Test3";

        _input.Submitted.Add(expected1);
        _input.Submitted.Add(expected2);
        _resolving.ResolveInput(_input, _output);

        _input.Submitted.RemoveAt(0);
        _resolving.ResolveInput(_input, _output);

        _input.Submitted.Add(expected3);
        _resolving.ResolveInput(_input, _output);

        Assert.AreEqual(3, _output.Lines.Count);
        Assert.AreEqual(expected1, _output.Lines[0]);
        Assert.AreEqual(expected2, _output.Lines[1]);
        Assert.AreEqual(expected3, _output.Lines[2]);
    }

    [Test]
    public void InputNewSubmissionsWithRemovedSubmissions_CopiedToOutputLines()
    {
        string expected1 = "Test1";
        string expected2 = "Test2";
        string expected3 = "Test3";

        _input.Submitted.Add(expected1);
        _input.Submitted.Add(expected2);
        _resolving.ResolveInput(_input, _output);

        _input.Submitted.RemoveAt(0);
        _input.Submitted.Add(expected3);
        _resolving.ResolveInput(_input, _output);

        // WORKAROUND : 30 :: Ignore for now, since this is known to fail due to SDK limitations.
        Assert.Ignore();

        Assert.AreEqual(3, _output.Lines.Count);
        Assert.AreEqual(expected1, _output.Lines[0]);
        Assert.AreEqual(expected2, _output.Lines[1]);
        Assert.AreEqual(expected3, _output.Lines[2]);
    }

    [Test]
    public void InputUnsubmitted_CopiedToOutputActiveText()
    {
        string expected = "Test";

        _input.Unsubmitted.Value = expected;

        _resolving.ResolveInput(_input, _output);

        Assert.AreEqual(expected, _output.ActiveText.Value);
    }
}

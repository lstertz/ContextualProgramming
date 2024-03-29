﻿global using ContextualProgramming;
using ContextualProgramming.Showcase.Contexts;

App app = new();

/// Initialization.
app.Initialize();
Console.WriteLine("App initialized.");


// Validate initialization created a behavior that created a context.
if (app.GetContext<ContextA>() != null && app.GetContexts<ContextA>().Length == 1)
    Console.WriteLine("Confirmed one Context A exists.");
else
    Console.WriteLine("Failed to validate the existence of one Context A.");

ContextA? initializedContextA = app.GetContext<ContextA>();
if (initializedContextA != null)
    initializedContextA.State.Value = 1;

app.Update();

if (initializedContextA != null)
    initializedContextA.StateList.Add(1);

if (app.Update())
    Console.WriteLine("State list change was recorded.");

// Validate contextualization of a second context.
app.Contextualize(new ContextA());
Console.WriteLine("Contextualized an instance of Context A.");

if (app.GetContexts<ContextA>().Length == 2)
    Console.WriteLine("Confirmed contextualization of a second Context A.");
else
    Console.WriteLine("Failed to validate the contextualization of a second Context A.");


// Validate the decontextualization of the first context, and destruction of its behavior.
if (initializedContextA != null)
    app.Decontextualize(initializedContextA);
Console.WriteLine("Decontextualized the initialized instance of Context A.");

if (app.GetContext<ContextA>() != null && app.GetContexts<ContextA>().Length == 1)
    Console.WriteLine("Confirmed only one Context A exists.");
else
    Console.WriteLine("Failed to validate the existence of a single Context A.");

if (initializedContextA != null)
    initializedContextA.State.Value = 2;

app.Update();
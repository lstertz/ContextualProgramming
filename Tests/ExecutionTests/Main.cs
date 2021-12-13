global using ContextualProgramming;
global using static ContextualProgramming.App;

using Tests.Contexts;


// Pre-Initialization.
if (GetContext<ContextA>() == null)
    Console.WriteLine("Confirmed that no Context A exists.");
else
    Console.WriteLine("Failed to validate that there is no Context A.");


/// Initialization.
Initialize();
Console.WriteLine("App initialized.");


// Validate initialization created a behavior that created a context.
if (GetContext<ContextA>() != null && GetContexts<ContextA>().Length == 1)
    Console.WriteLine("Confirmed one Context A exists.");
else
    Console.WriteLine("Failed to validate the existence of one Context A.");


// Validate contextualization of a second context.
Contextualize(new ContextA());
Console.WriteLine("Contextualized an instance of Context A.");

if (GetContexts<ContextA>().Length == 2)
    Console.WriteLine("Confirmed contextualization of a second Context A.");
else
    Console.WriteLine("Failed to validate the contetualization of a second Context A.");

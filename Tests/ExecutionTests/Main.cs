global using ContextualProgramming;
global using static ContextualProgramming.App;

using Tests.Contexts;

Initialize();
Console.WriteLine("App initialized.");

Contextualize(new ContextA());
Console.WriteLine("Contextualized instance of Context A.");

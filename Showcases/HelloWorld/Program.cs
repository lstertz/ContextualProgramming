global using ContextualProgramming;
using ContextualProgramming.IO;


// Create the Contextual Programming App to manage all aspects of the application.
App app = new();

// Initialize the app so it can process the application's code.
app.Initialize();

// Create the output context that will hold the text to be displayed to the console.
ConsoleOutput output = new();

// Contextualize the output context so it is processed by the app.
app.Contextualize(output);

// Add our "Hello World!" line so it is displayed in the console.
output.Lines.Add("Hello World!");

// Update the app to process the new context.
app.Update();
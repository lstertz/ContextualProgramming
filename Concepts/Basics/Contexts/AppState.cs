﻿namespace ContextualProgramming.Concepts.Contexts;

/// <summary>
/// A context that defines the running state of the app.
/// For a thorough overview of Contexts <see cref="SampleContext"/>
/// </summary>
[Context]
public class AppState
{
    /// <summary>
    /// Whether the app should continue running.
    /// </summary>
    public ContextState<bool> ContinueRunning { get; init; } = true;
}
using ContextualProgramming.Concepts.Basics.Behaviors;
using ContextualProgramming.Concepts.Basics.Contexts;
using ContextualProgramming.Concepts.Basics;

/// <summary>
/// This Concepts project provides code samples as learning materials and 
/// references for the Contextual Programming SDK, as well as 
/// Contextual Programming paradigm in general.
/// 
/// When reviewing these samples and their documentation, keep in mind that 
/// the focus of Contextual Programming is on 'when' functionality should be 
/// performed; where 'when' is defined by the state of the program or the 
/// change in the state of the program. Contexts define the state and behaviors 
/// define the functionality.
/// 
/// Recommended order for learning about the basic concepts is:
/// 1. <see cref="BasicContext"/>
/// 2. <see cref="BasicBehavior"/>
/// 3. <see cref="RunningTheApp"/>
/// 4. <see cref="InitializationBehavior"/>
/// 5. <see cref="DependentBehavior"/>
/// 
/// After reviewing those code samples, one should be able to create their own very 
/// basic Contextual Programming application, albeit without input, output, 
/// or complex relationships. Those concepts are covered as advanced concepts, including:
/// - ... (TODO)
/// - ... (TODO)
/// 
/// Patterns outline how contexts and behaviors can be structured to achieve a certain goal.
/// Currently defined patterns include:
/// - <see cref="SampleConsumer"/>
/// - <see cref="SampleRefiner"/>
/// - ... (TODO)
/// 
/// Strategies outline the types of roles that contexts and behaviors can have in 
/// certain situations and how those roles can be leveraged to achieve a certain goal.
/// Patterns are often applied as part of the structure that enables a strategy.
/// Currently defines strategies include:
/// - Messaging
/// - - <see cref="SampleMessage"/>
/// - - <see cref="SampleSender"/>
/// - - <see cref="SampleReceiver"/>
/// - ... (TODO)
/// 
/// </summary>
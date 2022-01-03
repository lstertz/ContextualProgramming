# Contextual Programming #
The development project of the Contextual Programming SDK (C#), an SDK to enable programming in accordance to the Contextual Programming paradigm.
Current functionality is limited to behaviors that create their own dependencies (contexts) and perform operations for specified context state changes.

## The Contextual Programming Paradigm ##
A brief in-progress overview:<br>
<br>
Contextual Programming is a programming paradigm that focuses on coding from the perspective of 'when'. 
Code systems are organized around behaviors, their operations, and contexts. 
Compositions are defined by the shared dependencies (contexts) of behaviors. 
Relationships are dynamic and determined by the state of the application, as defined by the contexts. 
Functionality therefore is performed by the qualifying operations of active behaviors based on the current state of existing contexts.<br>

## How to Use ##
Behaviors and contexts are defined by attributes. The default attributes are 'Behavior' and 'Context'.<br>
<br>
Contexts should only have 'ContextState' and 'ContextStateList' properties that are public get and init.<br>
<br>
Behaviors may have dependencies defined by the 'Dependency' attribute on the class. Dependencies must be contexts. The name used for the dependency must be used in all other places the dependency is referenced.<br>
<br>
A Behavior's constructor must have out parameters for all of its created dependencies.<br>
<br>
Behavior methods may be decorated with the 'Operation' attribute and a subsequent 'OnChange' attribute to define when the method should be called.<br>
<br>
Currently, only Behaviors with self-created dependencies are supported. These dependencies must be created in their behavior's constructor, which outputs the created contexts through out parameters.<br>
<br>
An instance of 'App' must be created and initialized at runtime. This will result in the instantiation of all completely self-fulfilled behaviors and their dependencies.<br>
<br>
'App.Update' must be called for value changes to be evaluated.<br>
<br>
Since all contexts are currently only able to be handled by their creating behaviors, 'App.GetContext' must be used for an external system to modify the states of a context.<br>
<br>
<b>Refer to the Showcase project for an example implementation.</b>

## Release ##
The latest release is 1.0.0 (01/02/2022), avialable on NuGet - https://www.nuget.org/packages/ContextualProgramming/

## Follow Us ##
Development - https://trello.com/b/IYk7na3D/contextual-programming
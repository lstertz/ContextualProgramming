# Contextual Programming #
The development project of the Contextual Programming SDK (C#), an SDK to enable programming in 
accordance to the Contextual Programming paradigm.
Current functionality is limited to behaviors that create their own dependencies (contexts), 
behaviors that depend upon the contexts created by other behaviors, and behaviors that perform 
operations for specified context state changes.

## The Contextual Programming Paradigm ##
A brief in-progress overview:<br>
<br>
Contextual Programming is a programming paradigm that focuses on coding from the perspective of 'when'. 
Code systems are organized around behaviors, their operations, and contexts. 
Compositions are defined by the shared dependencies (contexts) of behaviors. 
Relationships are dynamic and determined by the state of the application, as defined by the contexts. 
Functionality therefore is performed by the qualifying operations of active behaviors based on 
the current state of existing contexts.<br>

## How to Use ##
Behaviors and contexts are defined by attributes decorating classes that should take on 
those roles. The default attributes are 'Behavior' and 'Context'.<br>
<br>
Refer to the Concepts project for details of how to use each of the attributes, as well as for 
samples of generalized constructs and functionality.<br>
<br>
<b>Refer to the Showcase project for an example implementation.</b>

## Release ##
The latest release is 1.1.0 (02/15/2022), avialable on NuGet - https://www.nuget.org/packages/ContextualProgramming/

## Follow Us ##
Development - https://trello.com/b/IYk7na3D/contextual-programming
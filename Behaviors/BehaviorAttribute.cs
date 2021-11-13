using System;

namespace ContextualProgramming
{
    /// <summary>
    /// Declares a class as a Behavior, which permits its instances to be managed as 
    /// constructs that depend upon and manipulate, through contextual execution, 
    /// the app's contextual state.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class BehaviorAttribute : Attribute
    {
    }
}

using ContextualProgramming.Concepts.Basics.Contexts;

namespace ContextualProgramming.Concepts.Basics;

public class RunningTheApp
{
    public static void Main()
    {

        #region App Setup

        // To run an application that uses the Contextual Programming SDK, an instance of App must
        // be created and initialized.

        App app = new();
        app.Initialize();

        #endregion

        #region Defining App's Running State

        // If a context state should determine whether the app continues to run, 
        // then the context needs to either be created here or created by an
        // initialization behavior.

        /* If created here. **
        AppState appState = new();
        app.Contextualize(appState);  // The new context must be registered with the app.
        */

        /* If created by an initialization behavior (see InitializationBehavior.cs). */
        AppState appState = app.GetContext<AppState>() ?? throw new InvalidOperationException();

        #endregion

        #region Running the App

        // The app must be updated for any behaviors to perform their operations and 
        // for any contexts to have their state updated. Without calling update, 
        // any non-initialization and non-contextualization related functionality
        // of Contextual Programming will not be performed.

        /* To keep running per a context state updated internally by the app. */
        while (appState.ContinueRunning)
            app.Update();

        /* To keep running until a behavior exits the app. **
        while (true)
            app.Update();
        */

        #endregion

    }
}
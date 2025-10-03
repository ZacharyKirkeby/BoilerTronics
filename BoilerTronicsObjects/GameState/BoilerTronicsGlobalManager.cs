// This will be a singlton that will be accessabel throught the entire program
// Using this manager (or a similar object if we feel it is needed to split into multiple objects) will be used to do the following:
//      - Pass level data into the level scene to define the level
//      - Pass save data into a level (using the same mechanism as mentioned above)
//      - Aquire the user's scores for the UI
//      - Keep track of the user's progression

using Godot;

public partial class BoilerTronicsGlobalManager : Node
{
	public static BoilerTronicsGlobalManager GlobalManager { get; private set; } // This will be the global singelton we interact with throught the program

	public override void _Ready()
	{
		// Make sure there only exists on manager
		if (GlobalManager != null)
		{
			// TODO: make error here   
		}

		GlobalManager = this; // get this as the manager
	}
}

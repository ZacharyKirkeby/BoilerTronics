// This will be a singlton that will be accessabel throught the entire program
// Using this manager (or a similar object if we feel it is needed to split into multiple objects) will be used to do the following:
//      - Pass level data into the level scene to define the level
//      - Pass save data into a level (using the same mechanism as mentioned above)
//      - Aquire the user's scores for the UI
//      - Keep track of the user's progression

using Godot;
using System.Collections;
using System.Collections.Generic;
using BoilerTronicsObjects.Placeable;
using BoilerTronicsObjects.Layers;

public partial class BoilerTronicsGlobalManager : Node
{
	/* Game State Vars */
	private int[] levelIDs = [0, 0]; // sets the range of viable level IDs: [min, max]
	private int levelID = 0;
	private int[] levelLoadSlots = [0, 2]; // sets the range of viable level saves: [min, max]
	private int levelLoadSlot = -1;	// -1 means load actual default level setup, -2 means autosave
	
	// TODO:
	// For current demo, when pressing "New Game" for the very first time, this loads 'level0'
	// But every time after (i.e. return to main menu, click "New Game" again), this will now instead load an autosave
	// With current implementation, exiting from 'level0' will automatically generate an autosave;
	// therefore, while yes save data is saved locally on one's machine, the autosave more or less acts as a session-only save.
	// This is controlled by the above values! (i.e. (0, -1) means that system autoloads level 0)
	// (and after loading a level properly, game manager sets values to (0, -2) i.e. system will load autosaves)
	
	// getters for above; setters handled separately
	public int GetLevelID() { return levelID; }
	public int GetLevelLoadSlot() { return levelLoadSlot; }
	
	/***** Testing vars *****/
	public int currSlection;
	public int placingObject;
	public ObjectPicker picker;
	public Vector2I objectToPlace;
	public PlaceableObject objectToMove;
	public BoilerTronicsLevel currLevel;
	/***** End Testing Vars *****/
	
	/* Save Data Vars: */
	private BoilerTronicsSaveState saveState = new BoilerTronicsSaveState();
	
	/* Hold addresses to the layer objects; required for the save function! */
	public Layer 	layerClaw;
	public Layer 	layerFactory;
	public Layer 	layerFloor;
	public Layer 	layerMovement;
	public Layer 	layerRail;

	public static BoilerTronicsGlobalManager GlobalManager { get; private set; } // This will be the global singelton we interact with throught the program

	public override void _Ready()
	{
		// Make sure there only exists one manager
		if (GlobalManager != null)
		{
			// TODO: make error here   
		}

		GlobalManager = this; // get this as the manager
	}

	public BoilerTronicsGlobalManager GetManager()
	{
		return GlobalManager;
	}
	
	
	/* SAVE STUFF */
	
	// set level IDs and etc
	// returns if operation was successful (TODO error handling by user functions)
	// example calls:
	// autosave: SetTargetLevelSave(0, -2);
	// level 0, default level: SetTargetLevelSave(0, -1);
	// level 0, save slot 0: SetTargetLevelSave(0, 0);
	public bool SetTargetLevelSave(int inId, int inSlot) {
		// check if save slot is in bounds
		// -1 is default level save
		if (inSlot != -1 && inSlot != -2 &&
			(inSlot < levelLoadSlots[0] || inSlot > levelLoadSlots[1])) {
			return false;
		}
		
		// check if level Id is in bounds
		// -1 is autosave
		if (inId < levelIDs[0] || inId > levelIDs[1]) {
			return false;
		}
		
		// update internal variables
		levelID = inId;
		levelLoadSlot = inSlot;
		return true;
	}
	
	// trigger basic autosave
	public void SaveAutosave() {
		saveState.SaveAutosave(this);
	}
	
	// saves level based off the currently set global manager values
	public void SaveLevel() {
		string saveLocation;
		
		// if trying to save onto an actual level slot, error!
		if (levelLoadSlot == -1) {
			GD.Print("ERROR: trying to save over a base level!");
			return;
		}
		
		// determine save load locations
		// for actual levels, load levels progamatically!
		if (levelLoadSlot == -2) {
			saveLocation = "auto";
		} else {
			// level save location example:
			// dir/level0/save0.sav
			saveLocation = "level" + levelID + "/save" + levelLoadSlot;
		}
		saveState.SaveDataTo(this, saveLocation);
	}

	// loads level based off the currently set global manager values
	// returns if save was successful or not
	public bool LoadLevel() {
		string saveLocation;
		
		// determine save load locations
		// for actual levels, load levels progamatically!
		if (levelLoadSlot == -2) {
			// autosave
			saveLocation = "auto";
		} else {
			// level save location example:
			// dir/level0/save0.sav
			saveLocation = "level" + levelID + "/save" + levelLoadSlot;
		}
		bool res;
		if (levelLoadSlot != -1) {
			// load save data
			res = saveState.LoadSaveData(this, saveLocation);
		} else {
			// load level data
			GD.Print("attempting to load level data, level#: " + levelID);
			res = saveState.LoadLevelData(this, levelID);
			// default back to autosave after loading a level
			levelLoadSlot = -2;
		}
		if (res) {
			// if load was successful, then update global manager if needed
			// TODO
		}
		return res;
	}
	
	// get loading-specific data per layer
	// YES I KNOW that using strings to determine layer is not nice
	// but this was very much "hacked up" in the moment and I was moderately concerned with
	// keeping values properly hidden at the time. Oh well.
	// TODO: revisit entire save system sometime later.
	public ArrayList GetSaveObjectList(string layer) {
		//GD.Print("get save objlist: " + layer);
		return saveState.GetObjectList(layer);
	}
	public Vector2I[] GetSaveProtectedTiles(string layer) {
		//GD.Print("get save proctiles: " + layer);
		return saveState.GetProtectedTiles(layer);
	}
	
	// set/get level dimensions
	public void SetLevelDimensions(Vector2I max) {
		saveState.SetLevelDimensions(max);
	}
	public Vector2I GetLevelDimensions() {
		return saveState.GetLevelDimensions();
	}
}

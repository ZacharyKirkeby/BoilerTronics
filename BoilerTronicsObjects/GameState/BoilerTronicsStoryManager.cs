using Godot;
using System;

/*
	TO USE (quick summary):
	- make this manager load story progress from local file
	- then use "HasStoryPlayed(int number)" to confirm if story was accessed or not.
	- use "MarkStoryPlayed(int number)" to mark a story as "played"
	- "SaveData()" and "LoadData()" update this manager accordingly
*/
public partial class BoilerTronicsStoryManager : Node
{
	public static BoilerTronicsStoryManager StoryManager;
	
	private Godot.Collections.Dictionary<int, bool> playedLevels = new();
	
	public BoilerTronicsStoryManager() {
		Startup();
	}
	
	public override void _Ready() {
		Startup();
	}
	
	private void Startup() {
		if (StoryManager != null) {
			return;
		}
		
		StoryManager = this;
		
		// bool loadedData = LoadData();
	}
	
	// check: has story been played yet?
	public bool HasStoryPlayed(int input) {
		if (playedLevels.ContainsKey(input)) {
			return playedLevels[input];
		} else {
			return false;
		}
	}
	
	// mark story as having been "played"
	public void MarkStoryPlayed(int input) {
		if (playedLevels.ContainsKey(input)) {
			playedLevels[input] = true;
		} else {
			playedLevels.Add(input, true);
		}
	}
	
	// Save achievements and easter eggs to file
	public void SaveData() {
		string SavePath = "user://story.ach";
		// GD.Print("Leaderboard: SaveScore: path: ", SavePath);
		GD.Print("BoilerTronicsStoryManager: Saving: Saving Data");
		
		
		var saveFile = FileAccess.Open(SavePath, FileAccess.ModeFlags.Write);
		// 'using' keyword means that this is automatically disposed of when going out of scope
		
		// if we can't open the file, then try and make the directory
		// and then try to open the file again
		if (saveFile == null) {
			GD.Print("BoilerTronicsStoryManager: Could not save, err: ", FileAccess.GetOpenError());
			GD.Print("BoilerTronicsStoryManager: Trying to create (recursive) directory(s) instead:");
			
			var dirSuccess = DirAccess.MakeDirRecursiveAbsolute("user://Leaderboard/leaderboard");
			
			// if 'ERROR' == 0, then good. else, not so good.
			if (dirSuccess != 0) {
				GD.Print("BoilerTronicsStoryManager: Failed to make recursive directory(s): " + "user://Leaderboard/leaderboard");
				return;
			}
			
			// try again
			saveFile = FileAccess.Open(SavePath, FileAccess.ModeFlags.Write);
			
			if (saveFile == null) {
				GD.Print("BoilerTronicsStoryManager: Could not save, err: ", FileAccess.GetOpenError());
				GD.Print("BoilerTronicsStoryManager: Aborting save process.");
				return;
			} else {
				GD.Print("BoilerTronicsStoryManager: Successfully created recursive directories and save file. Continue saving process now.");
			}
		}
		
		Godot.Collections.Dictionary<string, Variant> data = 
			new Godot.Collections.Dictionary<string, Variant>()
			{
				{ "playedLevels", playedLevels }
			};
		
		saveFile.StoreLine(Json.Stringify(data));
		((FileAccess) saveFile).Close();
		
		GD.Print("BoilerTronicsStoryManager: Saving: Successfully save story progress to local.");
	}
	
	// Load the user's played levels and etc
	public bool LoadData() {
		string SavePath = "user://story.ach";
		GD.Print("BoilerTronicsStoryManager: Loading: Loading Data");
		
		if (!FileAccess.FileExists(SavePath)) {
			GD.PrintErr("BoilerTronicsStoryManager: Loading: LoadData failed to find file: ", SavePath);
			return false;
		} // not valid save location
		
		// open up save data
		using var saveFile = FileAccess.Open(SavePath, FileAccess.ModeFlags.Read);
		
		// much copied from Godot's documentation
		// while (saveFile.GetPosition() < saveFile.GetLength()) {
			var jsonString = saveFile.GetLine();

			// Creates the helper class to interact with JSON.
			var json = new Json();
			var parseResult = json.Parse(jsonString);
			if (parseResult != Error.Ok)
			{
				GD.PrintErr("BoilerTronicsStoryManager: Loading: JSON Parse Error: {json.GetErrorMessage()} in {jsonString} at line {json.GetErrorLine()}");
				// continue;
				return false;
			}
			

			// Get the data from the JSON object.
			// TODO: advanced error checking
			var nodeData = new Godot.Collections.Dictionary<string, Variant>((Godot.Collections.Dictionary)json.Data);
			
			GD.Print("BoilerTronicsStoryManager: Loading: Loaded dictionary:", nodeData);
			Godot.Collections.Dictionary<int, bool> progressLoaded = (Godot.Collections.Dictionary<int, bool>) nodeData["playedLevels"];
			GD.Print("BoilerTronicsStoryManager: Loading: Loaded story progress data.");
			
			foreach(var item in progressLoaded) {
				int key = item.Key;
				bool val = item.Value;
				
				// if loaded progress has a value, update
				// minor optimization: only do so if save data and local data differ
				if (playedLevels.ContainsKey(key)) {	// check: does local data have key?
					if (playedLevels[key] == val) {
						// if local data has same value as imported data, skip
						continue;
					}
					// if local data and imported data differ in values, update
					playedLevels[key] = progressLoaded[key];
				} else {
					// if local data is missing new data, update local data.
					playedLevels.Add(key, val);
				}
			}
			
			// continue;
		// }
		
		GD.Print("BoilerTronicsStoryManager: Loading: Successfully Loaded Data");
		return true;
	}
	
}

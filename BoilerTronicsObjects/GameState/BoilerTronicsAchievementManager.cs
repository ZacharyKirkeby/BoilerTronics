using Godot;
using System;
using System.Collections.Generic;

/* 
	TO USE (quick summary):

	- pull the achievements manager from 'BoilerTronicsAchievementManager.AchievementManager'
	- call "TryAchievementUnlock(string key)"
			- TODO: similar script for easter eggs required or? will let Abhi decide
			
	- achievements should be saved to file when an easter egg/achievement is successfully unlocked
	- on class initialization, system should correctly load achievements from file (if such achievements exist)
		- note: save system loads based off whatever key/value pairs are defined in "InitializeAchievements()", should be able to scale appropriately
*/
public partial class BoilerTronicsAchievementManager : Node
{
	public static BoilerTronicsAchievementManager AchievementManager { get; private set; }
	
	private Godot.Collections.Dictionary<string, bool> achievements = new();
	private Godot.Collections.Dictionary<string, bool> eastereggs = new();
	
	public override void _Ready() {
		if (AchievementManager != null) {
			return;
		}

		AchievementManager = this;
		
		InitializeAchievements();
		
		// load data from file
		bool loadedData = LoadData();
		
		GD.Print("BoilerTronicsAchievement: Ready: Displaying achievement data");
		// DEBUG: verify achievement data
		foreach(var item in achievements) {
			string key = item.Key;
			bool val = item.Value;
			GD.Print("BoilerTronicsAchievement: Ready: achievement: '", key, "', value: '", val, "'");
		}
		GD.Print("BoilerTronicsAchievement: Ready: Displaying easter egg data");
		// iterate through eastereggs dictionaries and compare
		foreach(var item in eastereggs) {
			string key = item.Key;
			bool val = item.Value;
			GD.Print("BoilerTronicsAchievement: Ready: easter egg: '", key, "', value: '", val, "'");
		}
		
		GD.Print("BoilerTronicsAchievement: Ready: Finished initialization"); 
	}

	private void InitializeAchievements() {
		achievements["Achievement1"] = false;
		achievements["Achievement2"] = false;
		achievements["Achievement3"] = false;
		achievements["Achievement4"] = false;
		
		eastereggs["EasterEgg1"] = false;
		eastereggs["EasterEgg2"] = false;
		eastereggs["EasterEgg3"] = false;
	}

	public void UnlockAchievement(string name) {
		if (!achievements.ContainsKey(name)) {
			GD.PrintErr("Achievement \"", name, "\" does not exist!");
			return;
		}

		if(achievements[name]) {
			GD.Print("Achievement already unlocked");
			return;
		}

		achievements[name] = true;
		
		// save to file immediately
		SaveData();
	}

	public bool AchievementIsUnlocked(string name) {
		return achievements.ContainsKey(name) && achievements[name];
	}
	
	public Godot.Collections.Dictionary<string, bool> GetAllAchievements() {
		return achievements;
	}
	
	public void UnlockEasterEgg(string name) {
		if (!eastereggs.ContainsKey(name)) {
			GD.PrintErr("Easter Egg did not unlock");
			return;
		}

		if(eastereggs[name]) {
			GD.Print("Easter Egg already unlocked");
			return;
		}
		eastereggs[name] = true;
		
		// save to file immediately
		SaveData();
	}

	public bool EasterEggIsUnlocked(string name) {
		return eastereggs.ContainsKey(name) && eastereggs[name];
	}
	
	public Godot.Collections.Dictionary<string, bool> GetAllEasterEggs() {
		return eastereggs;
	}
	
	public void TryAchievementUnlock(string name) {
		if(AchievementIsUnlocked(name)) {
			return;
		}
		UnlockAchievement(name);
		ShowAchievementPopup(name);
	}
	
	public void ShowAchievementPopup(string name) {
		var root = GetTree().CurrentScene;
		var notice = root.GetNodeOrNull<Window>("Achievement Notice");
		if (notice != null) {
			notice.Visible = true;
		}
	}

	public void TryEasterEggUnlock(string name) {
		if(EasterEggIsUnlocked(name)) {
			return;
		}
		UnlockEasterEgg(name);
		ShowEasterEggPopup(name);
	}

	public void ShowEasterEggPopup(string name) {
		var root = GetTree().CurrentScene;
		var notice = root.GetNodeOrNull<Window>("Easter Egg");
		if(name==("EasterEgg1")) {
			notice = root.GetNodeOrNull<Window>("Time Easter Egg");
		}
		else if(name==("EasterEgg2")) {
			notice = root.GetNodeOrNull<Window>("Cost Easter Egg");
		}
		else if(name==("EasterEgg3")) {
			notice = root.GetNodeOrNull<Window>("Mystery Level Easter Egg");
		}
		if (notice != null) {
			notice.Visible = true;
		}
	}
	
	
	// Save achievements and easter eggs to file
	private void SaveData() {
		string SavePath = "user://achievements.ach";
		// GD.Print("Leaderboard: SaveScore: path: ", SavePath);
		GD.Print("BoilerTronicsAchievements: Saving: Saving Data");
		
		
		var saveFile = FileAccess.Open(SavePath, FileAccess.ModeFlags.Write);
		// 'using' keyword means that this is automatically disposed of when going out of scope
		
		// if we can't open the file, then try and make the directory
		// and then try to open the file again
		if (saveFile == null) {
			GD.Print("BoilerTronicsAchievement: Could not save, err: ", FileAccess.GetOpenError());
			GD.Print("BoilerTronicsAchievement: Trying to create (recursive) directory(s) instead:");
			
			var dirSuccess = DirAccess.MakeDirRecursiveAbsolute("user://Leaderboard/leaderboard");
			
			// if 'ERROR' == 0, then good. else, not so good.
			if (dirSuccess != 0) {
				GD.Print("BoilerTronicsAchievement: Failed to make recursive directory(s): " + "user://Leaderboard/leaderboard");
				return;
			}
			
			// try again
			saveFile = FileAccess.Open(SavePath, FileAccess.ModeFlags.Write);
			
			if (saveFile == null) {
				GD.Print("BoilerTronicsAchievement: Could not save, err: ", FileAccess.GetOpenError());
				GD.Print("BoilerTronicsAchievement: Aborting save process.");
				return;
			} else {
				GD.Print("BoilerTronicsAchievement: Successfully created recursive directories and save file. Continue saving process now.");
			}
		}
		
		Godot.Collections.Dictionary<string, Variant> data = 
			new Godot.Collections.Dictionary<string, Variant>()
			{
				{ "achievements", achievements },
				{ "eastereggs", eastereggs }
			};
		
		saveFile.StoreLine(Json.Stringify(data));
		((FileAccess) saveFile).Close();
		
		GD.Print("BoilerTronicsAchievements: Saving: Successfully saved achievements and easter eggs to local.");
	}
	
	// Load the user's achievements and easter eggs
	// Returns if successfully retreived or not
	private bool LoadData() {
		string SavePath = "user://achievements.ach";
		GD.Print("BoilerTronicsAchievements: Loading: Loading Data");
		
		if (!FileAccess.FileExists(SavePath)) {
			GD.PrintErr("BoilerTronicsAchievements: Loading: LoadScore failed to find file: ", SavePath);
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
				GD.PrintErr("BoilerTronicsAchievements: Loading: JSON Parse Error: {json.GetErrorMessage()} in {jsonString} at line {json.GetErrorLine()}");
				// continue;
				return false;
			}
			

			// Get the data from the JSON object.
			// TODO: advanced error checking
			var nodeData = new Godot.Collections.Dictionary<string, Variant>((Godot.Collections.Dictionary)json.Data);
			
			GD.Print("BoilerTronicsAchievements: Loading: Loaded dictionary:", nodeData);
			Godot.Collections.Dictionary<string, bool> achievementsLoaded = (Godot.Collections.Dictionary<string, bool>) nodeData["achievements"];
			Godot.Collections.Dictionary<string, bool> eastereggsLoaded= (Godot.Collections.Dictionary<string, bool>) nodeData["eastereggs"];
			GD.Print("BoilerTronicsAchievements: Loading: Loaded achievement, easter egg data");
			
			// iterate through achievement dictionaries and compare
			foreach(var item in achievements) {
				string key = item.Key;
				bool val = item.Value;
				
				// if loaded achievements has key, then update local with loaded key
				// minor optimization: only do so if save data and local data differ
				GD.Print("BoilerTronicsAchievements: Loading: Local has key: '", key, "', val: '", val, "'");
				if (achievementsLoaded.ContainsKey(key)) {
					if (achievementsLoaded[key] == val) {
						GD.Print("BoilerTronicsAchievements: Loading: Local val '", val, "' and save val '", achievementsLoaded[key], "' are identical, skipping.");
						continue;
					}
					GD.Print("BoilerTronicsAchievements: Loading: Overriding achievement, key: ", key);
					achievements[key] = achievementsLoaded[key];
				}
			}
			
			// iterate through eastereggs dictionaries and compare
			foreach(var item in eastereggs) {
				string key = item.Key;
				bool val = item.Value;
				
				// if loaded achievements has key, then update local with loaded key
				// minor optimization: only do so if save data and local data differ
				if (eastereggsLoaded.ContainsKey(key)) {
					if (eastereggsLoaded[key] == val) {
						GD.Print("BoilerTronicsAchievements: Loading: Local val '", val, "' and save val '", eastereggsLoaded[key], "' are identical, skipping.");
						continue;
					}
					GD.Print("BoilerTronicsAchievements: Loading: Overriding easter egg, key: ", key);
					eastereggs[key] = eastereggsLoaded[key];
				}
			} 
			
			// continue;
		// }
		
		GD.Print("BoilerTronicsAchievements: Loading: Successfully Loaded Data");
		return true;
	}
}

using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class Leaderboard : CenterContainer
{
	static private Label firstName;
	static private Label firstScore;
	static private Label secondName;
	static private Label secondScore;
	static private Label thirdName;
	static private Label thirdScore;
	static private Label fourthName;
	static private Label fourthScore;
	static private Label fifthName;
	static private Label fifthScore;
	static private Label sixthName;
	static private Label sixthScore;
	static private Label extraName;
	static private Label extraScore;

	private List<(string Name, float Score)> leaderboard = new();

	public override void _Ready() {
		//get labels
		firstName = GetNode<Label>("%firstName");
		firstScore = GetNode<Label>("%firstScore");
		secondName = GetNode<Label>("%secondName");
		secondScore = GetNode<Label>("%secondScore");
		thirdName = GetNode<Label>("%thirdName");
		thirdScore = GetNode<Label>("%thirdScore");
		fourthName = GetNode<Label>("%fourthName");
		fourthScore = GetNode<Label>("%fourthScore");
		fifthName = GetNode<Label>("%fifthName");
		fifthScore = GetNode<Label>("%fifthScore");
		sixthName = GetNode<Label>("%sixthName");
		sixthScore = GetNode<Label>("%sixthScore");
		
		//leaderboard default values
		
		// TEST: saves "You" with score 10.0f for level 0
		// SaveScore(0, "You", 10.0f);
		
		
		
		/*
		leaderboard = new List<(string, float)>
		{
			("You", 5),
			("Ethan", 99),
			("Abhi", 80),
			("Keenan", 85),
			("Zach", 60),
			("Ethen", 50),
			("Bob", 200)
		};
		*/
		
		// TODO: instead, load leaderboard data from a local save!
		// i.e. construct the leaderboard (only top 6 scores?) after loading a local save
		/*
			First: if no local leaderboard save exists, "request from server"
			- for now, load from game files

			If the user ("You") has a locally saved statistic, load and attempt to "insert" into the list, and update the leaderboard accordingly
			
			TODO: what about if the user is way below the leaderboard?
			- discussed with Abhi, will be implemented soon
		
		*/
		HandleLeaderboard(0);
		UpdateLeaderboard();
		// UpdateDisplay();
	}
	private void _on_option_button_item_selected(int index) {
		
		// TODO: implement loading system for these cases, i.e. these are the actually relevant cases that spawn/register
		// on level leaderboard select
		switch (index) {
			case 0:
				/*
				leaderboard = new List<(string, float)>
				{
					("You", 5),
					("Ethan", 99),
					("Abhi", 80),
					("Keenan", 85),
					("Zach", 60),
					("Ethen", 50),
					("Bob", 200)
				};
				*/
				HandleLeaderboard(0);
				break;
			case 1:
				/*
				leaderboard = new List<(string, float)>
				{
					("Keenan", 100),
					("Ethen", 99),
					("Zach", 90),
					("You", 87),
					("Abhi", 85),
					("Ethan", 82)
				};
				*/
				HandleLeaderboard(1);
				break;
		}
		UpdateLeaderboard();
	}
	
	public void UpdateLeaderboard() {
		BoilerTronicsGlobalManager manager = BoilerTronicsGlobalManager.GlobalManager;
		// if (manager == null || manager.currLevel == null) {
			leaderboard = leaderboard.OrderByDescending(entry => entry.Score).ToList();
			UpdateDisplay();
			// return;
		// }
		/*
		float score = manager.currLevel.bestScore;
		for (int i = 0; i < leaderboard.Count; i++) {
			if (leaderboard[i].Name == "You") {
				leaderboard[i] = ("You", score);
				break;
			}
		}
		leaderboard = leaderboard.OrderByDescending(entry => entry.Score).ToList();
		UpdateDisplay();
		*/
	}
	
	private void UpdateDisplay() {
		GD.Print("updating leadboard display");
		var labels = new (Label name, Label score)[]
		{
			(firstName, firstScore),
			(secondName, secondScore),
			(thirdName, thirdScore),
			(fourthName, fourthScore),
			(fifthName, fifthScore),
			(sixthName, sixthScore)
		};

		for (int i = 0; i < labels.Length; i++) {
			if (i < leaderboard.Count) {
				labels[i].name.Text = leaderboard[i].Name;
				labels[i].score.Text = leaderboard[i].Score.ToString("F2");
			}
			else {
				labels[i].name.Text = "-";
				labels[i].score.Text = "-";
			}
		}
		
		var topEntries = leaderboard.Take(6).ToList();
		var playerEntry = leaderboard.FirstOrDefault(entry => entry.Name == "You");
		bool playerInTop = topEntries.Any(entry => entry.Name == "You");
		
		Label extraNameLabel = GetNodeOrNull<Label>("%extraName");
		Label extraScoreLabel = GetNodeOrNull<Label>("%extraScore");

		if (extraNameLabel != null && extraScoreLabel != null) {
			if (!playerInTop && playerEntry.Name != null) {
				extraNameLabel.Text = playerEntry.Name;
				extraScoreLabel.Text = playerEntry.Score.ToString("F2");

				extraNameLabel.Visible = true;
				extraScoreLabel.Visible = true;

				extraNameLabel.AddThemeColorOverride("font_color", new Color(0.3f, 0.3f, 0.3f));
				extraScoreLabel.AddThemeColorOverride("font_color", new Color(0.3f, 0.3f, 0.3f));
			}
			else {
				extraNameLabel.Visible = false;
				extraScoreLabel.Visible = false;
			}
	}
	}
	
	
	// LEADERBOARD SAVING STUFF
	// TODO: create system that properly merges local scores and "server" leaderboard saves
	// or "submits" the local scores to the server to handle and etc
	
	// given a level ID, handle all the loading/saving and etc
	private void HandleLeaderboard(int levelId) {
		// "load from server"
		leaderboard = new List<(string, float)>();
		LoadLeaderboard(levelId, leaderboard);
		
		// "retrieve local high score"
		bool retrievedLocalScore = LoadScore(levelId, leaderboard);
		if (!retrievedLocalScore) {
			// if local high score doesn't exist, give user a score of 0.00
			leaderboard.Add(("You", 0.0f));
		}
	}
	
	// Save leaderboard according to the level id
	// TODO: save/merge entire leaderboard, only loads user's scores per-level atm
	public static void SaveScore(int levelId, string scoreName, float scoreValue) {
		string SavePath = "user://Leaderboard/leaderboard" + levelId + ".leaderboard";
		// GD.Print("Leaderboard: SaveScore: path: ", SavePath);
		
		
		var saveFile = FileAccess.Open(SavePath, FileAccess.ModeFlags.Write);
		// 'using' keyword means that this is automatically disposed of when going out of scope
		
		// if we can't open the file, then try and make the directory
		// and then try to open the file again
		if (saveFile == null) {
			GD.Print("Leaderboard: Could not save, err: ", FileAccess.GetOpenError());
			GD.Print("Leaderboard: Trying to create (recursive) directory(s) instead:");
			
			var dirSuccess = DirAccess.MakeDirRecursiveAbsolute("user://Leaderboard/leaderboard");
			
			// if 'ERROR' == 0, then good. else, not so good.
			if (dirSuccess != 0) {
				GD.Print("Leaderboard: Failed to make recursive directory(s): " + "user://Leaderboard/leaderboard");
				return;
			}
			
			// try again
			saveFile = FileAccess.Open(SavePath, FileAccess.ModeFlags.Write);
			
			if (saveFile == null) {
				GD.Print("Leaderboard: Could not save, err: ", FileAccess.GetOpenError());
				GD.Print("Leaderboard: Aborting save process.");
				return;
			} else {
				GD.Print("Leaderboard: Successfully created recursive directories and save file. Continue saving process now.");
			}
		}
		
		// BoilerTronicsGlobalManager manager = BoilerTronicsGlobalManager.GlobalManager;
		
		Godot.Collections.Dictionary<string, Variant> data = 
			new Godot.Collections.Dictionary<string, Variant>()
			{
				{ "username", scoreName },
				{ "score", scoreValue }
			};
		
		saveFile.StoreLine(Json.Stringify(data));
		((FileAccess) saveFile).Close();
		
		GD.Print("Leaderboard: Successfully saved to local.");
	}
	
	// Load leaderboard according to the level id
	public void LoadLeaderboard(int levelId, List<(string, float)> scoreList) {
		string SavePath = "res://Resources/Levels/leaderboard" + levelId + ".leaderboard";
		// GD.Print("Leaderboard: Trying to load file from ", SavePath);
		
		if (!FileAccess.FileExists(SavePath)) {
			GD.Print("Leaderboard: Loading: File does not exist!");
			return;
		} // not valid save location
		
		// open up save data
		using var saveFile = FileAccess.Open(SavePath, FileAccess.ModeFlags.Read);
		
		// much copied from Godot's documentation
		while (saveFile.GetPosition() < saveFile.GetLength()) {
			var jsonString = saveFile.GetLine();

			// Creates the helper class to interact with JSON.
			var json = new Json();
			var parseResult = json.Parse(jsonString);
			if (parseResult != Error.Ok)
			{
				GD.Print("Leaderboard: JSON Parse Error: {json.GetErrorMessage()} in {jsonString} at line {json.GetErrorLine()}");
				continue;
			}
			

			// Get the data from the JSON object.
			// TODO: advanced error checking
			var nodeData = new Godot.Collections.Dictionary<string, Variant>((Godot.Collections.Dictionary)json.Data);
			
			// GD.Print("Leaderboard: Loaded dictionary:", nodeData);

			string name = (string) nodeData["username"];
			float score = (float) nodeData["score"];
			
			// GD.Print("Leaderboard: username: ", name, ", score: ", score);
			// GD.Print("SaveState: metadata: level name: ", levelName);
			scoreList.Add((name, score));
			
			continue;
		}
		
	}
	
	// Load the user's score (just one!)
	// Returns if successfully retreived or not
	public static bool LoadScore(int levelId, List<(string, float)> scoreList) {
		string SavePath = "user://Leaderboard/leaderboard" + levelId + ".leaderboard";
		
		if (!FileAccess.FileExists(SavePath)) {
			GD.Print("Leaderboard: LoadScore failed to find file: ", SavePath);
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
				GD.Print("Leaderboard: JSON Parse Error: {json.GetErrorMessage()} in {jsonString} at line {json.GetErrorLine()}");
				// continue;
				return false;
			}
			

			// Get the data from the JSON object.
			// TODO: advanced error checking
			var nodeData = new Godot.Collections.Dictionary<string, Variant>((Godot.Collections.Dictionary)json.Data);
			
			// GD.Print("Leaderboard: Loaded dictionary:", nodeData);

			string name = (string) nodeData["username"];
			float score = (float) nodeData["score"];
			
			// GD.Print("Leaderboard: username: ", name, ", score: ", score);
			// GD.Print("SaveState: metadata: level name: ", levelName);
			scoreList.Add((name, score));
			
			// continue;
		// }
		
		return true;
	}
}

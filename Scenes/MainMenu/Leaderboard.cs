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
		
		// TODO: instead, load leaderboard data from a local save!
		// i.e. construct the leaderboard (only top 6 scores?) after loading a local save
		/*
			First: if no local leaderboard save exists, "request from server"
			- for now, load from game files

			If the user ("You") has a locally saved statistic, load and attempt to "insert" into the list, and update the leaderboard accordingly
			
			TODO: what about if the user is way below the leaderboard?
			- discussed with Abhi, will be implemented soon
		
		*/
		
		UpdateLeaderboard();
		// UpdateDisplay();
	}
	private void _on_option_button_item_selected(int index) {
		
		// TODO: implement loading system for these cases, i.e. these are the actually relevant cases that spawn/register
		// on level leaderboard select
		switch (index) {
			case 0:
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
				break;
			case 1:
				leaderboard = new List<(string, float)>
				{
					("Keenan", 100),
					("Ethen", 99),
					("Zach", 90),
					("You", 87),
					("Abhi", 85),
					("Ethan", 82)
				};
				break;
		}
		UpdateLeaderboard();
	}
	
	public void UpdateLeaderboard() {
		BoilerTronicsGlobalManager manager = BoilerTronicsGlobalManager.GlobalManager;
		if(manager == null || manager.currLevel == null) {
			leaderboard = leaderboard.OrderByDescending(entry => entry.Score).ToList();
			UpdateDisplay();
			return;
		}
		float score = manager.currLevel.bestScore;
		for (int i = 0; i < leaderboard.Count; i++) {
			if (leaderboard[i].Name == "You") {
				leaderboard[i] = ("You", score);
				break;
			}
		}
		leaderboard = leaderboard.OrderByDescending(entry => entry.Score).ToList();
		UpdateDisplay();
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
}

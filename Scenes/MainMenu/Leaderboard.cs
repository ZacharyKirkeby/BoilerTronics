using Godot;
using System;
using System.Collections.Generic;

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
		firstName.Text = ("You");
		firstScore.Text = ("100");
		secondName.Text = ("Ethan");
		secondScore.Text = ("99");
		thirdName.Text = ("Abhi");
		thirdScore.Text = ("80");
		fourthName.Text = ("Keenan");
		fourthScore.Text = ("70");
		fifthName.Text = ("Zach");
		fifthScore.Text = ("60");
		sixthName.Text = ("Ethen");
		sixthScore.Text = ("50");
		
		UpdateLeaderboard();
	}
	private void _on_option_button_item_selected(int index) {
		switch (index) {
			case 0:
				firstName.Text = ("You");
				firstScore.Text = ("100");
				secondName.Text = ("Ethan");
				secondScore.Text = ("99");
				thirdName.Text = ("Abhi");
				thirdScore.Text = ("80");
				fourthName.Text = ("Keenan");
				fourthScore.Text = ("70");
				fifthName.Text = ("Zach");
				fifthScore.Text = ("60");
				sixthName.Text = ("Ethen");
				sixthScore.Text = ("50");
				break;
			case 1:
				firstName.Text = ("Keenan");
				firstScore.Text = ("100");
				secondName.Text = ("Ethen");
				secondScore.Text = ("99");
				thirdName.Text = ("Zach");
				thirdScore.Text = ("90");
				fourthName.Text = ("You");
				fourthScore.Text = ("87");
				fifthName.Text = ("Abhi");
				fifthScore.Text = ("85");
				sixthName.Text = ("Ethan");
				sixthScore.Text = ("82");
				break;
		}
	}
	
	public void UpdateLeaderboard() {
		BoilerTronicsGlobalManager manager = BoilerTronicsGlobalManager.GlobalManager;
		if(manager == null || manager.currLevel == null) {
			return;
		}
		int rc = manager.currLevel.minRC;
		float pps = manager.currLevel.minPPS;
		float cps = manager.currLevel.minCPS;
		float averageScore = pps + cps + (float)rc;
		averageScore /= 3.0f;
		firstScore.Text = averageScore.ToString("F2");
	}
}

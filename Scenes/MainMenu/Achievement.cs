using Godot;
using System;

public partial class Achievement : Window
{
	static private Label Achievement1Desc;
	static private Label Achievement2Desc;
	static private Label Achievement3Desc;
	static private Label Achievement4Desc;
	
	static private TextureRect Achievement1Image;
	static private TextureRect Achievement2Image;
	static private TextureRect Achievement3Image;
	static private TextureRect Achievement4Image;
	
	public override void _Ready() {
		Achievement1Desc = GetNode<Label>("%Achievement1 Description");
		Achievement2Desc = GetNode<Label>("%Achievement2 Description");
		Achievement3Desc = GetNode<Label>("%Achievement3 Description");
		Achievement4Desc = GetNode<Label>("%Achievement4 Description");
		
		Achievement1Image = GetNode<TextureRect>("%Achievement1 Image");
		Achievement2Image = GetNode<TextureRect>("%Achievement2 Image");
		Achievement3Image = GetNode<TextureRect>("%Achievement3 Image");
		Achievement4Image = GetNode<TextureRect>("%Achievement4 Image");
		
		UpdateUI();
	}
	
	private void UpdateUI() {
		var achievementManager = BoilerTronicsAchievementManager.AchievementManager;
		var allAchievements = achievementManager.GetAllAchievements();
		
		foreach(var pair in allAchievements) {
			string name = pair.Key;
			bool unlocked = pair.Value;
			UpdateAchievement(name, unlocked);
		}
	}
	
	private void UpdateAchievement(string name, bool unlocked) {
		Label desc = null;
		TextureRect icon = null;
		switch (name)
		{
			case "Achievement1":
				desc = Achievement1Desc;
				icon = Achievement1Image;
				break;
			case "Achievement2":
				desc = Achievement2Desc;
				icon = Achievement2Image;
				break;
			case "Achievement3":
				desc = Achievement3Desc;
				icon = Achievement3Image;
				break;
			case "Achievement4":
				desc = Achievement3Desc;
				icon = Achievement3Image;
				break;
			default:
				GD.PrintErr($"Unknown achievement: {name}");
				return;
		}
		if(unlocked) {
			desc.Text = "Completed: " + desc.Text;
			icon.Texture = GD.Load<Texture2D>("res://Resources/Icons/star.png");
		}
	}
}

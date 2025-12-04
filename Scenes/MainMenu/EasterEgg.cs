using Godot;
using System;

public partial class EasterEgg : Window
{
	static private Label EasterEgg1Desc;
	static private Label EasterEgg2Desc;
	static private Label EasterEgg3Desc;
	
	static private TextureRect EasterEgg1Image;
	static private TextureRect EasterEgg2Image;
	static private TextureRect EasterEgg3Image;
	
	public override void _Ready() {
		EasterEgg1Desc = GetNode<Label>("%EasterEgg1 Description");
		EasterEgg2Desc = GetNode<Label>("%EasterEgg2 Description");
		EasterEgg3Desc = GetNode<Label>("%EasterEgg3 Description");
		
		EasterEgg1Image = GetNode<TextureRect>("%EasterEgg1 Image");
		EasterEgg2Image = GetNode<TextureRect>("%EasterEgg2 Image");
		EasterEgg3Image = GetNode<TextureRect>("%EasterEgg3 Image");
		
		UpdateUI();
	}
	
	private void UpdateUI() {
		var achievementManager = BoilerTronicsAchievementManager.AchievementManager;
		var allEasterEggs = achievementManager.GetAllEasterEggs();
		
		foreach(var pair in allEasterEggs) {
			string name = pair.Key;
			bool unlocked = pair.Value;
			UpdateEasterEgg(name, unlocked);
		}
	}
	
	private void UpdateEasterEgg(string name, bool unlocked) {
		Label desc = null;
		TextureRect icon = null;
		switch (name)
		{
			case "EasterEgg1":
				desc = EasterEgg1Desc;
				icon = EasterEgg1Image;
				if(unlocked) {
					desc.Text = "Found: Solved a solution with 100+ time steps over the cutoff.";
				}
				break;
			case "EasterEgg2":
				desc = EasterEgg2Desc;
				icon = EasterEgg2Image;
				if(unlocked) {
					desc.Text = "Found: Solved a solution $5,000+ over the cutoff.";
				}
				break;
			case "EasterEgg3":
				desc = EasterEgg3Desc;
				icon = EasterEgg3Image;
				if(unlocked) {
					desc.Text = "Found: You found the secret level!";
				}
				break;
			default:
				GD.PrintErr($"Unknown easter egg: {name}");
				return;
		}
		if(unlocked) {
			//desc.Text = "Found: " + desc.Text;
			icon.Texture = GD.Load<Texture2D>("res://Resources/Icons/star.png");
		}
	}
}

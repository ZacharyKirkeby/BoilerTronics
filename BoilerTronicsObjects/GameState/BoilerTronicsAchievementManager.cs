using Godot;
using System;
using System.Collections.Generic;

public partial class BoilerTronicsAchievementManager : Node
{
		public static BoilerTronicsAchievementManager AchievementManager { get; private set; }
		
		private Dictionary<string, bool> achievements = new();
		private Dictionary<string, bool> eastereggs = new();
		
		public override void _Ready() {
			if (AchievementManager != null) {
				return;
			}

			AchievementManager = this;
			
			//eventually load in from file
			InitializeAchievements();
		}

		private void InitializeAchievements() {
			achievements["Achievement1"] = false;
			achievements["Achievement2"] = false;
			achievements["Achievement3"] = false;
			achievements["Achievement4"] = false;
			
			eastereggs["EasterEgg1"] = false;
			eastereggs["EasterEgg2"] = false;
			eastereggs["EasterEgg3"] = false;
			eastereggs["EasterEgg4"] = false;
		}

		public void UnlockAchievement(string name) {
			if (!achievements.ContainsKey(name)) {
				GD.PrintErr("Achievment did not unlock");
				return;
			}

			if(achievements[name]) {
				GD.Print("Achievement already unlocked");
				return;
			}

			achievements[name] = true;
		}

		public bool AchievementIsUnlocked(string name) {
			return achievements.ContainsKey(name) && achievements[name];
		}
		
		public Dictionary<string, bool> GetAllAchievements() {
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
		}

		public bool EasterEggIsUnlocked(string name) {
			return eastereggs.ContainsKey(name) && eastereggs[name];
		}
		
		public Dictionary<string, bool> GetAllEasterEggs() {
			return eastereggs;
		}
}

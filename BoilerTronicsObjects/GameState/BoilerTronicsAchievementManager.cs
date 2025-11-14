using Godot;
using System;
using System.Collections.Generic;

public partial class BoilerTronicsAchievementManager : Node
{
		public static BoilerTronicsAchievementManager AchievementManager { get; private set; }
		
		private Dictionary<string, bool> achievements = new();
		
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

		public bool IsUnlocked(string name) {
			return achievements.ContainsKey(name) && achievements[name];
		}
		
		public Dictionary<string, bool> GetAllAchievements() {
			return achievements;
		}
}

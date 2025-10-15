using Godot;
using System;
using BoilerTronicsObjects.Layers;
using BoilerTronicsObjects.Objects.ClawLayerObjects;
using BoilerTronicsObjects.Placeable;

namespace BoilerTronicsObjects.Objects.ClawLayerObjects {

	public class TrackObject : ClawLayerObjects {
		
		int direction; // 0 = left; 1 = right;

		public const int Left = 0;
		public const int Right = 1;

		static Vector2I LeftObjectAtlasPos = new Vector2I(0, 1);
		static Vector2I RightObjectAtlasPos = new Vector2I(0, 2);

		public TrackObject(int OGX, int OGY, int dir, int altTitle = 0) 
		: base(OGX, OGY, TrackObject.RightObjectAtlasPos, altTitle) {
			if (dir != TrackObject.Right || dir != TrackObject.Left) return; // Error

			if (dir == TrackObject.Right) {
				this.SetAtlasPos(RightObjectAtlasPos);
			} else {
				this.SetAtlasPos(LeftObjectAtlasPos);
			}

			direction = dir;
			
			UpdateSprite();
		}
		private void UpdateSprite() {
			BoilerTronicsGlobalManager manager = BoilerTronicsGlobalManager.GlobalManager;

			manager.currLevel.mLayer.SetCell(this.GetCurrPos(), this.GetSourceID(), this.GetAtlasPos()); // Set the new sprite
		}

		public void ChangeDir(int newDir) {
			if (newDir != TrackObject.Right || newDir != TrackObject.Left) return;
			direction = newDir;

			UpdateSprite();
		}

		public int GetDir() {
			return direction;
		}
	}
}

using Godot;
using System;
using BoilerTronicsObjects.Layers;
using BoilerTronicsObjects.Objects.ClawLayerObjects;
using BoilerTronicsObjects.Placeable;

namespace BoilerTronicsObjects.Objects.ClawLayerObjects {

	public class TrackObject : PlaceableObject {
		
		public override int GetCost() { return 100; }
		public new static int GetCostStatic() { return 100; }
		
		static int layerSourceId = 1;
		// reminder that the sourceID corresponds to the sprite sheet for a given layer
		// and every layer will have their own sprite sheet. Consequently, layer-specific
		// objects will have identical sourceIds.
		// "atlasPos" corresponds to the location on a given sprite sheet that a specific object
		// (i.e. "claw", "factory", "floor tile", "leftrail") will correspond to.
		
		int direction; // 0 = left; 1 = right;
		int OGdir;

		public const int Left = 0;
		public const int Right = 1;

		static Vector2I LeftObjectAtlasPos = new Vector2I(0, 1);
		static Vector2I RightObjectAtlasPos = new Vector2I(0, 2);

		public TrackObject(int OGX, int OGY, int dir, int altTitle = 0) 
		: base(OGX, OGY, layerSourceId, dir == TrackObject.Right ? TrackObject.RightObjectAtlasPos : TrackObject.LeftObjectAtlasPos, altTitle) {
			if (dir != TrackObject.Right && dir != TrackObject.Left) return; // Error

			direction = dir;
			OGdir = dir;
		}

		private void UpdateSprite() {
			BoilerTronicsGlobalManager manager = BoilerTronicsGlobalManager.GlobalManager;

			manager.currLevel.rLayer.SetCell(this.GetCurrPos(), this.GetSourceID(), this.GetAtlasPos()); // Set the new sprite
		}

		private void updateAtlas() {
			if (direction == TrackObject.Right) {
				this.SetAtlasPos(TrackObject.RightObjectAtlasPos);
			} else {
				this.SetAtlasPos(TrackObject.LeftObjectAtlasPos);
			}

			UpdateSprite();
		}

		public void ChangeDir(int newDir) {
			if (newDir != TrackObject.Right && newDir != TrackObject.Left) return;
			this.direction = newDir;
			updateAtlas();
		}

		public int GetDir() {
			return direction;
		}

		public override void ResetPos() {
			this.direction = this.OGdir;
			base.ResetPos();
			updateAtlas();
		}
	}
}

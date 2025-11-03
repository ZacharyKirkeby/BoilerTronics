using Godot;
using System;
using BoilerTronicsObjects.Layers;
using BoilerTronicsObjects.Objects.FactoryLayerObjects;
using BoilerTronicsObjects.Placeable;

// NOTE: Largely copied from TrackObject.cs
namespace BoilerTronicsObjects.Objects.FactoryLayerObjects {

	public class PipeBrokenFloorObject : PlaceableObject {
		
		static int layerSourceId = 5;
		// reminder that the sourceID corresponds to the sprite sheet for a given layer
		// and every layer will have their own sprite sheet. Consequently, layer-specific
		// objects will have identical sourceIds.
		// "atlasPos" corresponds to the location on a given sprite sheet that a specific object
		// (i.e. "claw", "factory", "floor tile", "leftrail") will correspond to.
		
		int direction; // 0 = left; 1 = right;
		int OGdir;

		public const int Left = 0;
		public const int Right = 1;

		static Vector2I LeftObjectAtlasPos = new Vector2I(0, 3);
		static Vector2I RightObjectAtlasPos = new Vector2I(1, 3);

		public PipeBrokenFloorObject(int OGX, int OGY, int dir, int altTitle = 0) 
		: base(OGX, OGY, layerSourceId, dir == PipeBrokenFloorObject.Right ? PipeBrokenFloorObject.RightObjectAtlasPos : PipeBrokenFloorObject.LeftObjectAtlasPos, altTitle) {
			if (dir != PipeBrokenFloorObject.Right && dir != PipeBrokenFloorObject.Left) return; // Error

			direction = dir;
			OGdir = dir;
		}

		private void UpdateSprite() {
			BoilerTronicsGlobalManager manager = BoilerTronicsGlobalManager.GlobalManager;

			manager.currLevel.rLayer.SetCell(this.GetCurrPos(), this.GetSourceID(), this.GetAtlasPos()); // Set the new sprite
		}

		private void updateAtlas() {
			if (direction == PipeBrokenFloorObject.Right) {
				this.SetAtlasPos(PipeBrokenFloorObject.RightObjectAtlasPos);
			} else {
				this.SetAtlasPos(PipeBrokenFloorObject.LeftObjectAtlasPos);
			}

			UpdateSprite();
		}

		public void ChangeDir(int newDir) {
			if (newDir != PipeBrokenFloorObject.Right && newDir != PipeBrokenFloorObject.Left) return;
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

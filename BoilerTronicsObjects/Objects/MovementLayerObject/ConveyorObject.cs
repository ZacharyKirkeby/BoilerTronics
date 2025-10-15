using Godot;
using System;
using BoilerTronicsObjects.Layers;
using BoilerTronicsObjects.Objects.FactoryLayerObjects;
using BoilerTronicsObjects.Placeable;
using BoilerTronicsObjects.Interfaces;

namespace BoilerTronicsObjects.Objects.MovementLayerObjects {

	public class ConveyorObject : MovementLayerObjects {

		int direction; // 0 = left; 1 = right;

		public const int Left = 0;
		public const int Right = 1;

		static Vector2I LeftObjectAtlasPos = new Vector2I(0, 0);
		static Vector2I RightObjectAtlasPos = new Vector2I(0, 1);

		private void UpdateSprite() {
			BoilerTronicsGlobalManager manager = BoilerTronicsGlobalManager.GlobalManager;

			manager.currLevel.mLayer.SetCell(this.GetCurrPos(), this.GetSourceID(), this.GetAtlasPos()); // Set the new sprite
		}

		public ConveyorObject(int OGX, int OGY, int dir, int altTitle = 0) : base(OGX, OGY, dir == ConveyorObject.Right ? ConveyorObject.RightObjectAtlasPos : ConveyorObject.LeftObjectAtlasPos, altTitle) {
			if (dir != ConveyorObject.Right || dir != ConveyorObject.Left) return; // Error

			if (dir == ConveyorObject.Right) {
				this.SetAtlasPos(RightObjectAtlasPos);
			} else {
				this.SetAtlasPos(LeftObjectAtlasPos);
			}

			direction = dir;
			
			UpdateSprite();
		}

		public void ChangeDir(int newDir) {
			if (newDir != ConveyorObject.Right || newDir != ConveyorObject.Left) return;
			direction = newDir;

			if (newDir == ConveyorObject.Right) {
				this.SetAtlasPos(RightObjectAtlasPos);
			} else {
				this.SetAtlasPos(LeftObjectAtlasPos);
			}

			UpdateSprite();
		}

		public int GetDir() {
			return direction;
		}
	}
}

using Godot;
using System;
using System.Collections;
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
			if (dir != ConveyorObject.Right && dir != ConveyorObject.Left) return; // Error

			direction = dir;
		}

		public void ChangeDir(int newDir) {
			if (newDir != ConveyorObject.Right && newDir != ConveyorObject.Left) return;
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

		public void Move(Vector2I vec) {
			// Get the rail below us
			// If there is none return
			// Otherwise move it based on the input vector
		}

		public ArrayList getConnections() {
			BoilerTronicsGlobalManager manager = BoilerTronicsGlobalManager.GlobalManager;
			Vector2I v1;
			Vector2I v2;

			if (this.direction == ConveyorObject.Right) {
				v1 = new Vector2I(1, -1);
				v2 = new Vector2I(-1, 1);
			} else {
				v1 = new Vector2I(0, 1);
				v2 = new Vector2I(1, 0);
			}

			ArrayList retList = new ArrayList();
			
			PlaceableObject obj1 = manager.currLevel.mLayer.FindObject(this.GetCurrPos() + v1);
			if (obj1 != null) retList.Add(obj1);

			PlaceableObject obj2 = manager.currLevel.mLayer.FindObject(this.GetCurrPos() + v2);
			if (obj2 != null) retList.Add(obj2);

			return retList;
		}
	}
}

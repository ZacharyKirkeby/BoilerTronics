using Godot;
using System;
using System.Collections;
using BoilerTronicsObjects.Layers;
using BoilerTronicsObjects.Objects.ClawLayerObjects;
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
			BoilerTronicsGlobalManager manager = BoilerTronicsGlobalManager.GlobalManager;
			PlaceableObject obj = manager.currLevel.rLayer.FindObject(this.GetCurrPos());
			// If there is none return
			if (obj == null) return;
			if (!(obj is TrackObject tObj)) return;

			// Otherwise move it based on the input vector
			MovingObject mObj = new MovingObject(tObj, vec, manager.currLevel.rLayer, 1);
			manager.currLevel.cLayer.GetParent().AddChild(mObj);
		}

		public ArrayList GetConnections() {
			BoilerTronicsGlobalManager manager = BoilerTronicsGlobalManager.GlobalManager;
			Vector2I v1;
			Vector2I v2;

			if (this.direction == ConveyorObject.Left) {
				v1 = new Vector2I(1, -1);
				v2 = new Vector2I(-1, 1);
			} else {
				v1 = new Vector2I(1, 0);
				v2 = new Vector2I(-1, 0);
			}

			ArrayList retList = new ArrayList();
			
			PlaceableObject obj1 = manager.currLevel.mLayer.FindObject(this.GetCurrPos() + v1);
			GD.Print(obj1);
			if (obj1 != null && obj1 is ConveyorObject cObj1 && cObj1.GetDir() == this.GetDir()) retList.Add(obj1);

			PlaceableObject obj2 = manager.currLevel.mLayer.FindObject(this.GetCurrPos() + v2);
			GD.Print(obj2);
			if (obj2 != null && obj2 is ConveyorObject cObj2 && cObj2.GetDir() == this.GetDir()) retList.Add(obj2);

			return retList;
		}
	}
}

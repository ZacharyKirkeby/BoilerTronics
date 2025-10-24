using Godot;
using System;
using System.Collections;
using BoilerTronicsObjects.Layers;
using BoilerTronicsObjects.Objects.ClawLayerObjects;
using BoilerTronicsObjects.Placeable;
using BoilerTronicsObjects.Interfaces;

namespace BoilerTronicsObjects.Objects.MovementLayerObjects {

	public class ConveyorObject : PlaceableObject {

		int direction; // 0 = left; 1 = right;

		public const int Left = 0;
		public const int Right = 1;
		
		static int layerSourceId = 2;
		// reminder that the sourceID corresponds to the sprite sheet for a given layer
		// and every layer will have their own sprite sheet. Consequently, layer-specific
		// objects will have identical sourceIds.

		static Vector2I LeftObjectAtlasPos = new Vector2I(0, 0);
		static Vector2I RightObjectAtlasPos = new Vector2I(0, 1);
		
		// For saving purposes, the "head" of a ConveyorGroup should also point to the ConveyorGroup's terminal
		// Therefore, we must track it here!
		// This should be handled on ConveyorGroup creation/merging/editing in MovementLayer.cs
		// Otherwise, only saving functionality should interact with this system
		private CodeEdit E;
		
		// For loading purposes, have a specific string that will override its parent's group contents
		private string toLoadText;

		private void UpdateSprite() {
			BoilerTronicsGlobalManager manager = BoilerTronicsGlobalManager.GlobalManager;

			manager.currLevel.mLayer.SetCell(this.GetCurrPos(), this.GetSourceID(), this.GetAtlasPos()); // Set the new sprite
		}

		public ConveyorObject(int OGX, int OGY, int dir, int altTitle = 0) : base(OGX, OGY, layerSourceId, dir == ConveyorObject.Right ? ConveyorObject.RightObjectAtlasPos : ConveyorObject.LeftObjectAtlasPos, altTitle) {
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

			// Move track
			MovingObject mObj = new MovingObject(tObj, vec, manager.currLevel.rLayer, 1);
			manager.currLevel.cLayer.GetParent().AddChild(mObj);

			// Move claw if there exists one
			obj = manager.currLevel.cLayer.FindObject(this.GetCurrPos());
			if (obj == null) return;
			if (!(obj is ClawObject cObj)) return;
			if (cObj.moving) manager.currLevel.MovingCollisionReport(null);

			// Move track
			MovingObject mcObj = new MovingObject(cObj, vec, manager.currLevel.cLayer, 1);
			manager.currLevel.cLayer.GetParent().AddChild(mcObj);
			cObj.moving = true;
		}
		
		// Methods to deal with terminals (inherit from the parent ConveyorGroup)
		// This should mostly only be used by MovementLayer.cs
		public void SetTerminal(CodeEdit input) {
			E = input;
		}	
		public CodeEdit GetTerminal() {
			return E;
		}
		public string GetScript() {
			if (E == null) { return null; }
			return E.Text;
		}
		
		// For loading purposes
		public void SetToLoadText(string input) {
			toLoadText = input;
		}
		// For loading purposes
		public string GetToLoadText() {
			return toLoadText;
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
		
		// Override 'save' function to also return a script's information
		// CONDITIONAL: Only adds anything
		public override Godot.Collections.Dictionary<string, Variant> Save()
		{
			Godot.Collections.Dictionary<string, Variant> res = base.Save();
			// GD.Print("TODO: override per-object serialization to also include corresponding CodeEdit information");
			
			if (E != null) {
				res["conveyorCode"] = GetScript();
			}
			return res;
		}
	}
}

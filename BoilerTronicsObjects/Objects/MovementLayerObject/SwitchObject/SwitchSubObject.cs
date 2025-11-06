using Godot;
using System;
using System.Collections;
using Godot.Collections;
using BoilerTronicsObjects.Layers;
using BoilerTronicsObjects.Objects.ClawLayerObjects;
using BoilerTronicsObjects.Placeable;
using BoilerTronicsObjects.Interfaces;

namespace BoilerTronicsObjects.Objects.MovementLayerObjects {

	public class SwitchSubObject : PlaceableObject {

		int direction; // 0 = left; 1 = right;

		public const int Left = 0;
		public const int Right = 1;
		public const int Up = 2;
		public const int Down = 3;
		
		static int layerSourceId = 2;
		// reminder that the sourceID corresponds to the sprite sheet for a given layer
		// and every layer will have their own sprite sheet. Consequently, layer-specific
		// objects will have identical sourceIds.

		// These will be changed to the actual sprites once the switch is drawn up
		static Vector2I LeftObjectAtlasPos = new Vector2I(0, 3);
		static Vector2I RightObjectAtlasPos = new Vector2I(0, 3);
		static Vector2I UpObjectAtlasPos = new Vector2I(0, 3);
		static Vector2I DownObjectAtlasPos = new Vector2I(0, 3);

		static Dictionary<int, Vector2I> atlasMap = new Dictionary<int, Vector2I>()
		{
			{Left, LeftObjectAtlasPos},
			{Right, RightObjectAtlasPos},
			{Up, UpObjectAtlasPos},
			{Down, DownObjectAtlasPos},
		};
		
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

		public SwitchSubObject(int OGX, int OGY, int dir, int altTitle = 0) : base(OGX, OGY, layerSourceId, atlasMap[dir], altTitle) {
			direction = dir;
		}

		public void ChangeDir(int newDir) {
			direction = newDir;

			this.SetAtlasPos(atlasMap[newDir]); // Set the new atlas pos

			UpdateSprite();
		}

		// We don't need a lot of the connections and such because these are static length groupings of objects

		public int GetDir() {
			return direction;
		}

		// Override 'save' function to also return a script's information
		// CONDITIONAL: Only adds anything
		public override Godot.Collections.Dictionary<string, Variant> Save()
		{
			Godot.Collections.Dictionary<string, Variant> res = base.Save();
			// GD.Print("TODO: override per-object serialization to also include corresponding CodeEdit information");
			
			return res;
		}
	}
}


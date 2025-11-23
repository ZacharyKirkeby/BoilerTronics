using Godot;
using System;
using System.Collections.Generic;
using BoilerTronicsObjects.Layers;
using BoilerTronicsObjects.Objects.ClawLayerObjects;
using BoilerTronicsObjects.Placeable;
using BoilerTronicsObjects.Interfaces;

namespace BoilerTronicsObjects.Objects.FactoryLayerObjects {

	public class PipeObject : PlaceableObject, GroupedSubObject {

		public override int GetCost() { return 100; }
		public new static int GetCostStatic() { return 100; }

		// We'll need a better way to detect this
		// TODO: make some enum or something to store the direction of the pipe
		// (each bit is a dir, 0 = not connected, 1 = connected)
		// public const int Left = 0;
		// public const int Right = 1;

		int direction;
		
		static int layerSourceId = 13;

		// reminder that the sourceID corresponds to the sprite sheet for a given layer
		// and every layer will have their own sprite sheet. Consequently, layer-specific
		// objects will have identical sourceIds.

		private PipeGroup group;

		// For loading purposes, have a specific string that will override its parent's group contents
		private string internalText = null;


		// We'll need to store these in a list, we can more than likely have these at the index of the corresponding enum
		static Vector2I LeftObjectAtlasPos = new Vector2I(0, 0);
		static Vector2I RightObjectAtlasPos = new Vector2I(0, 1);
		
		// Gets the group that this object belongs to
		public GroupedObject getGroup() {
			return group;
		}

		// Sets the group of the object
		public void setGroup(GroupedObject gObj) {
			if (gObj is PipeGroup cgObj) group = cgObj;
		}

		// Removes object from group (sets some internal var to NULL)
		public void removeFromGroup() {
			group = null;
		}

		// True if in group | False if not in group
		public bool inGroup() {
			return (!(group == null));
		}

		public GroupedObject createGroup() {
			return new PipeGroup(this.GetCurrPos().X, this.GetCurrPos().Y) as GroupedObject;
		}

		private void UpdateSprite() {
			BoilerTronicsGlobalManager manager = BoilerTronicsGlobalManager.GlobalManager;

			manager.currLevel.mLayer.SetCell(this.GetCurrPos(), this.GetSourceID(), this.GetAtlasPos()); // Set the new sprite
		}

		public PipeObject(int OGX, int OGY, int altTitle = 0) : base(OGX, OGY, layerSourceId, new Vector2I(0,0), altTitle) {
		}

		// TODO: this should update based on the connections instead of a passed in val
		public void ChangeDir(int newDir) {
			direction = newDir;

			UpdateSprite();
		}

		public int GetDir() {
			return direction;
		}

		public List<PipeObject> GetConnections() {
			BoilerTronicsGlobalManager manager = BoilerTronicsGlobalManager.GlobalManager;

			Vector2I v1 = new Vector2I(1, -1);
			Vector2I v2 = new Vector2I(-1, 1);
			Vector2I v3 = new Vector2I(1, 0);
			Vector2I v4 = new Vector2I(-1, 0);

			List<PipeObject> retList = new List<PipeObject>();
			
			PlaceableObject obj1 = manager.currLevel.mLayer.FindObject(this.GetCurrPos() + v1);
			if (obj1 != null && obj1 is PipeObject pObj1) retList.Add(pObj1);

			PlaceableObject obj2 = manager.currLevel.mLayer.FindObject(this.GetCurrPos() + v2);
			if (obj2 != null && obj2 is PipeObject pObj2) retList.Add(pObj2);

			PlaceableObject obj3 = manager.currLevel.mLayer.FindObject(this.GetCurrPos() + v3);
			if (obj3 != null && obj3 is PipeObject pObj3) retList.Add(pObj3);

			PlaceableObject obj4 = manager.currLevel.mLayer.FindObject(this.GetCurrPos() + v2);
			if (obj4 != null && obj4 is PipeObject pObj4) retList.Add(pObj4);

			return retList;
		}

		// These are just needed for the conv groups, pipes are not scriptable, so they don't need to actually do anything with these
		public void setText(string T) {
		}

		public string getText() {
			return null;
		}
	}
}

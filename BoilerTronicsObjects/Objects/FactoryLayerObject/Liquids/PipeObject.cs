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

		static int layerSourceId = 13;

		// bit 0 = up conn
		// bit 1 = right conn
		// bit 2 = down conn
		// bit 3 = left conn
		public enum Direction {
			NOCONN = 	0b0000,
			UCONN =		0b0001,
			RCONN =		0b0010,
			URCONN = 	0b0011,
			DCONN =		0b0100,
			UDCONN = 	0b0101,
			RDCONN = 	0b0110,
			URDCONN = 	0b0111,
			LCONN =		0b1000,
			ULCONN = 	0b1001,
			RLCONN = 	0b1010,
			URLCONN = 	0b1011,
			DLCONN = 	0b1100,
			UDLCONN = 	0b1101,
			RDLCONN = 	0b1110,
			ALLCONN = 	0b1111
		};

		Direction direction;

		private static Vector2I []sprites = [
			new Vector2I(0,0), // Default
			new Vector2I(0,0), // Up
			new Vector2I(1,0), // Right
			new Vector2I(1,2), // Up, Right
			new Vector2I(0,0), // Down
			new Vector2I(0,0), // Up, Down
			new Vector2I(0,2), // Right, Down
			new Vector2I(1,1), // Up, Right, Down
			new Vector2I(1,0), // Left
			new Vector2I(2,2), // Up, Left
			new Vector2I(1,0), // Right, Left
			new Vector2I(3,0), // Up, Right, Left
			new Vector2I(3,2), // Down, Left
			new Vector2I(0,1), // Up, Donw. Left
			new Vector2I(2,0), // Right, Down, Left
			new Vector2I(2,1)  // Up, Right, Down, Left
		];

		// We'll need to store these in a list, we can more than likely have these at the index of the corresponding enum
		static Vector2I LeftObjectAtlasPos = new Vector2I(0, 0);
		static Vector2I RightObjectAtlasPos = new Vector2I(0, 1);

		private PipeGroup group;

		// For loading purposes, have a specific string that will override its parent's group contents
		private string internalText = null;

		
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

		public void UpdateSprite() {
			BoilerTronicsGlobalManager manager = BoilerTronicsGlobalManager.GlobalManager;

			int u = 0;
			int d = 0;
			int l = 0;
			int r = 0;

			Vector2I uv = new Vector2I(1, -1);
			Vector2I dv = new Vector2I(-1, 1);
			Vector2I lv = new Vector2I(-1, 0);
			Vector2I rv = new Vector2I(1, 0);

			PlaceableObject obju = manager.currLevel.fLayer.FindObject(this.GetCurrPos() + uv);
			if (obju != null && obju is PipeObject) u = 1;

			PlaceableObject objd = manager.currLevel.fLayer.FindObject(this.GetCurrPos() + dv);
			if (objd != null && objd is PipeObject) d = 1;
			
			PlaceableObject objr = manager.currLevel.fLayer.FindObject(this.GetCurrPos() + rv);
			if (objr != null && objr is PipeObject) r = 1;

			PlaceableObject objl = manager.currLevel.fLayer.FindObject(this.GetCurrPos() + lv);
			if (objl != null && objl is PipeObject) l = 1;

			direction = (Direction) ((u) | (r << 1) | (d << 2) | (l << 3));

			SetAtlasPos(sprites[(int) direction]);

			manager.currLevel.fLayer.SetCell(this.GetCurrPos(), this.GetSourceID(), this.GetAtlasPos()); // Set the new sprite
		}

		public PipeObject(int OGX, int OGY, int altTitle = 0) : base(OGX, OGY, layerSourceId, new Vector2I(0,0), altTitle) {
		}

		public List<PipeObject> GetConnections() {
			BoilerTronicsGlobalManager manager = BoilerTronicsGlobalManager.GlobalManager;

			Vector2I v1 = new Vector2I(1, -1);
			Vector2I v2 = new Vector2I(-1, 1);
			Vector2I v3 = new Vector2I(1, 0);
			Vector2I v4 = new Vector2I(-1, 0);

			List<PipeObject> retList = new List<PipeObject>();
			
			PlaceableObject obj1 = manager.currLevel.fLayer.FindObject(this.GetCurrPos() + v1);
			if (obj1 != null && obj1 is PipeObject pObj1) retList.Add(pObj1);

			PlaceableObject obj2 = manager.currLevel.fLayer.FindObject(this.GetCurrPos() + v2);
			if (obj2 != null && obj2 is PipeObject pObj2) retList.Add(pObj2);

			PlaceableObject obj3 = manager.currLevel.fLayer.FindObject(this.GetCurrPos() + v3);
			if (obj3 != null && obj3 is PipeObject pObj3) retList.Add(pObj3);

			PlaceableObject obj4 = manager.currLevel.fLayer.FindObject(this.GetCurrPos() + v4);
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

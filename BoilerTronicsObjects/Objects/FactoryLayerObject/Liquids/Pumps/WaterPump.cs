using Godot;
using System;
using System.Collections.Generic;
using BoilerTronicsObjects.Layers;
using BoilerTronicsObjects.Objects.ClawLayerObjects;
using BoilerTronicsObjects.Placeable;
using BoilerTronicsObjects.Interfaces;

namespace BoilerTronicsObjects.Objects.FactoryLayerObjects {

	/*
	 * The purpose of this class is to provide a base for the pump objects for the seperate liquids.
	 * The default pump object should not bu used, but instead it should be inherited by the specific object
	 */
	public class WaterPump : PumpObject {

		public override int GetCost() { return 100; }
		public new static int GetCostStatic() { return 100; }

		static int layerSourceId = 14;

		Direction direction; // This will determine the direction the the pump will be facing

		// Vectors for the object we are connected to given our connection
		Vector2I up_v = new Vector2I(1, -1);
		Vector2I down_v = new Vector2I(-1, 1);
		Vector2I left_v = new Vector2I(-1, 0);
		Vector2I right_v = new Vector2I(1, 0);

		// We'll need to store these in a list, we can more than likely have these at the index of the corresponding enum
		static Vector2I LeftObjectAtlasPos = new Vector2I(0, 0);
		static Vector2I RightObjectAtlasPos = new Vector2I(0, 1);

		private PipeGroup group;

		Quality Q;

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

		public PumpObject(int OGX, int OGY, int altTitle = 0) : base(OGX, OGY, layerSourceId, new Vector2I(0,0), altTitle) {
		}

		public List<GroupedSubObject> GetConnections() {
			BoilerTronicsGlobalManager manager = BoilerTronicsGlobalManager.GlobalManager;

			List<GroupedSubObject> retList = new List<GroupedSubObject>();
			PlaceableObject obj = null;

			switch (direction) {
				case Direction.UP:
					obj = manager.currLevel.fLayer.FindObject(this.GetCurrPos() + up_v);
					break;
				case Direction.RIGHT:
					obj = manager.currLevel.fLayer.FindObject(this.GetCurrPos() + right_v);
					break;
				case Direction.DOWN:
					obj = manager.currLevel.fLayer.FindObject(this.GetCurrPos() + down_v);
					break;
				case Direction.LEFT:
					obj = manager.currLevel.fLayer.FindObject(this.GetCurrPos() + left_v);
					break;
			}

			if (obj != null && obj is PipeObject pObj) retList.Add(pObj);

			return retList;
		}

		/*
		 * This function will be used from pipe objects,
		 * The purpose of this function is to allow for pipes to see if they are able to connect to the pipe
		 * The only way for the pipe to be able to be connected is if it is on the correct direction of the pipe
		 */
		public bool ValidConnect(PipeObject p) {
			BoilerTronicsGlobalManager manager = BoilerTronicsGlobalManager.GlobalManager;

			PlaceableObject obj = null;

			switch (direction) {
				case Direction.UP:
					obj = manager.currLevel.fLayer.FindObject(this.GetCurrPos() + up_v);
					break;
				case Direction.RIGHT:
					obj = manager.currLevel.fLayer.FindObject(this.GetCurrPos() + right_v);
					break;
				case Direction.DOWN:
					obj = manager.currLevel.fLayer.FindObject(this.GetCurrPos() + down_v);
					break;
				case Direction.LEFT:
					obj = manager.currLevel.fLayer.FindObject(this.GetCurrPos() + left_v);
					break;
			}

			return (obj != null && obj is PipeObject pObj && pObj == p);
		}

		// These are just needed for the conv groups, pipes are not scriptable, so they don't need to actually do anything with these
		public void setText(string T) {
		}

		public string getText() {
			return null;
		}

		// Quality interface
		public void SetQuality(Quality newQuality) {
			Q = newQuality;
		}

		public Quality GetQuality() {
			return Q;
		}
	}
}

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
	public class PumpObject : PipeObject, QualityObject {

		public override int GetCost() { return 100; }
		public new static int GetCostStatic() { return 100; }

		static int layerSourceId = 14;

		Placeable.Direction direction; // This will determine the direction the the pump will be facing

		// Vectors for the object we are connected to given our connection
		Vector2I up_v = new Vector2I(1, -1);
		Vector2I down_v = new Vector2I(-1, 1);
		Vector2I right_v = new Vector2I(-1, 0);
		Vector2I left_v = new Vector2I(1, 0);

		// We'll need to store these in a list, we can more than likely have these at the index of the corresponding enum
		public static Vector2I []atPosArr = {
			new Vector2I(0,0),
			new Vector2I(0,0),
			new Vector2I(0,0),
			new Vector2I(0,0)
		};

		private PipeGroup group;

		Quality Q;

		// For loading purposes, have a specific string that will override its parent's group contents
		private string internalText = null;

		// This constructor shouldn't be used as we will never use the base pump
		public PumpObject(int OGX, int OGY, Quality Q, Placeable.Direction D, int altTitle = 0) : base(OGX, OGY, layerSourceId + (int) Q, atPosArr[(int) D], altTitle) {
			this.direction = D;
		}

		public PumpObject(int OGX, int OGY, int layerSourceId, Vector2I atPos, Quality Q, Placeable.Direction D, int altTitle = 0) : base(OGX, OGY, layerSourceId, atPos, altTitle) {
			this.direction = D;
		}

		public override List<PipeObject> GetConnections() {
			BoilerTronicsGlobalManager manager = BoilerTronicsGlobalManager.GlobalManager;

			List<PipeObject> retList = new List<PipeObject>();
			PlaceableObject obj = null;
			Vector2I vec = new Vector2I(-1, -1);

			switch (direction) {
				case Placeable.Direction.UP:
					vec = this.GetCurrPos() + up_v;
					break;
				case Placeable.Direction.RIGHT:
					vec = this.GetCurrPos() + right_v;
					break;
				case Placeable.Direction.DOWN:
					vec = this.GetCurrPos() + down_v;
					break;
				case Placeable.Direction.LEFT:
					vec = this.GetCurrPos() + left_v;
					break;
			}

			if (vec != new Vector2I(-1, -1)) obj = manager.currLevel.fLayer.FindObject(vec);

			if (obj != null && obj is PipeObject pObj) retList.Add(pObj);
			else if (obj != null && obj is BigGroupedSubObject bgsO) {
				PipeObject spO = bgsO.getGroupedObject(vec) as PipeObject;
				if (spO != null && spO.CanConnect(this)) retList.Add(spO);
			}

			return retList;
		}

		/*
		 * This function will be used from pipe objects,
		 * The purpose of this function is to allow for pipes to see if they are able to connect to the pipe
		 * The only way for the pipe to be able to be connected is if it is on the correct direction of the pipe
		 */
		public override bool CanConnect(PipeObject p) {
			BoilerTronicsGlobalManager manager = BoilerTronicsGlobalManager.GlobalManager;

			PlaceableObject obj = null;

			switch (direction) {
				case Placeable.Direction.UP:
					obj = manager.currLevel.fLayer.FindObject(this.GetCurrPos() + up_v);
					break;
				case Placeable.Direction.RIGHT:
					obj = manager.currLevel.fLayer.FindObject(this.GetCurrPos() + right_v);
					break;
				case Placeable.Direction.DOWN:
					obj = manager.currLevel.fLayer.FindObject(this.GetCurrPos() + down_v);
					break;
				case Placeable.Direction.LEFT:
					obj = manager.currLevel.fLayer.FindObject(this.GetCurrPos() + left_v);
					break;
			}

			return (obj != null && obj is PipeObject pObj && pObj == p);
		}
		
		public override void UpdateSprite() {
			// We never want to update the sprite
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

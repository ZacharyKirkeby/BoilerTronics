using Godot;
using System;
using BoilerTronicsObjects.Layers;
using BoilerTronicsObjects.Objects.FactoryLayerObjects;
using BoilerTronicsObjects.Placeable;
using BoilerTronicsObjects.Interfaces;

namespace BoilerTronicsObjects.Objects.FactoryLayerObjects {

	public class FactoryBigObjectInput : PlaceableObject, Movable {
		
		static Vector2I objectAtlasPos = new Vector2I(0, 0);
		// "atlasPos" corresponds to the location on a given sprite sheet that a specific object
		// (i.e. "claw", "factory", "floor tile", "leftrail") will correspond to.
		
		static int layerSourceId = 0;
		private PlaceableBig parent;
		private int _AcceptedObjectID = -1;

		// reminder that the sourceID corresponds to the sprite sheet for a given layer
		// and every layer will have their own sprite sheet. Consequently, layer-specific
		// objects will have identical sourceIds.
		
		public PlaceableObject PickUp() {
			return null; // We can't pick something up from an input
		}
		
		public bool Place(PlaceableObject obj) {
			if (parent is BigMovable bmP) return bmP.GiveObject(obj, this);
			return false;
		}

		public void SetParent(PlaceableBig newParent) {
			parent = newParent;
		}

		public void SetAcceptedObjectID(PlaceableBig newParent) {
			parent = newParent;
		}
		
		public FactoryBigObjectInput(int OGX, int OGY, int altTitle = 0)
		: base(OGX, OGY, layerSourceId, objectAtlasPos, altTitle) {
		}
	}
}

using Godot;
using System;
using BoilerTronicsObjects.Layers;
using BoilerTronicsObjects.Objects.FactoryLayerObjects;
using BoilerTronicsObjects.Placeable;
using BoilerTronicsObjects.Interfaces;

namespace BoilerTronicsObjects.Objects.FactoryLayerObjects {

	public class FactoryBigObjectOutput : PlaceableObject, Movable {
		
		static Vector2I objectAtlasPos = new Vector2I(0, 0);
		// "atlasPos" corresponds to the location on a given sprite sheet that a specific object
		// (i.e. "claw", "factory", "floor tile", "leftrail") will correspond to.
		
		static int layerSourceId = 0;
		private PlaceableBig parent;
		private int _ValidObj = 0;

		// reminder that the sourceID corresponds to the sprite sheet for a given layer
		// and every layer will have their own sprite sheet. Consequently, layer-specific
		// objects will have identical sourceIds.
		
		public PlaceableObject PickUp() {
			if (parent is BigMovable bmP) return bmP.RequestObject(_ValidObj, this);
			return null;
		}
		
		public bool Place(PlaceableObject obj) {
			return false;
		}

		public void SetParent(PlaceableBig newParent) {
			parent = newParent;
		}

		public void SetValidObj(int V) {
			_ValidObj = V;
		}
		
		public FactoryBigObjectOutput(int OGX, int OGY, int altTitle = 0)
		: base(OGX, OGY, layerSourceId, objectAtlasPos, altTitle) {
		}
	}
}

using Godot;
using System;
using BoilerTronicsObjects.Layers;
using BoilerTronicsObjects.Objects.FactoryLayerObjects;
using BoilerTronicsObjects.Placeable;
using BoilerTronicsObjects.Interfaces;

// We proably want to hace a refrence to the parent object so that we can feed the item input to the press

namespace BoilerTronicsObjects.Objects.FactoryLayerObjects {

	public class FactoryPressInput : PlaceableObject, Movable {
		
		private static Vector2I objectAtlasPos = new Vector2I(0, 3); // This is a dummy sprinte | TODO: Change this (not for this tesing object but for the actual object)
		private static int layerSourceId = 0;
		private FactoryPress parent;

		// "atlasPos" corresponds to the location on a given sprite sheet that a specific object
		// (i.e. "claw", "factory", "floor tile", "leftrail") will correspond to.
		
		// Moveable, this will allow us to pickup and drop off items
		public PlaceableObject PickUp() {
			return null; // we can't pickup from this
		}
		
		public bool Place(PlaceableObject obj) {
			// This will need to check to make sure that the object being passed in is able to actually be able to be 
			return false; // This will be true if we accept the material
		}

		public FactoryPressInput(int OGX, int OGY, int altTitle = 0) 
		: base(OGX, OGY, layerSourceId, objectAtlasPos, altTitle) {
		}
	}
}


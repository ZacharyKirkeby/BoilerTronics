using Godot;
using System;
using BoilerTronicsObjects.Layers;
using BoilerTronicsObjects.Objects.FactoryLayerObjects;
using BoilerTronicsObjects.Placeable;

namespace BoilerTronicsObjects.Objects.FactoryLayerObjects {

	public class FloorCrackedTileObject : PlaceableObject {
		
		static int layerSourceId = 5;
		// reminder that the sourceID corresponds to the sprite sheet for a given layer
		// and every layer will have their own sprite sheet. Consequently, layer-specific
		// objects will have identical sourceIds.
		
		static Vector2I objectAtlasPos = new Vector2I(0, 0);
		// "atlasPos" corresponds to the location on a given sprite sheet that a specific object
		// (i.e. "claw", "factory", "floor tile", "leftrail") will correspond to.
		
		public FloorCrackedTileObject(int OGX, int OGY, int altTitle = 0) 
		: base(OGX, OGY, layerSourceId, objectAtlasPos, altTitle) {}
	}
}

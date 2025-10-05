// TODO: implement in more detail
using Godot;
using System;
using BoilerTronicsObjects.Layers;
using BoilerTronicsObjects.Placeable;

namespace BoilerTronicsObjects.Objects.MovementLayerObjects {
	
	public abstract class MovementLayerObjects : PlaceableObject {
		
		static int layerSourceId = 3;
		// reminder that the sourceID corresponds to the sprite sheet for a given layer
		// and every layer will have their own sprite sheet. Consequently, layer-specific
		// objects will have identical sourceIds.
		// "atlasPos" corresponds to the location on a given sprite sheet that a specific object
		// (i.e. "claw", "factory", "floor tile", "leftrail") will correspond to.
		
		public MovementLayerObjects(int OGX, int OGY, Vector2I atlasPos, int altTitle) 
		: base(OGX, OGY, layerSourceId, atlasPos, altTitle) {}
	}
}

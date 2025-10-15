// TODO: implement in more detail
using Godot;
using System;
using BoilerTronicsObjects.Layers;
using BoilerTronicsObjects.Placeable;

namespace BoilerTronicsObjects.Objects.FactoryLayerObjects {
	
	public abstract class FactoryLayerObjects : PlaceableObject {
		
		static int layerSourceId = 0;
		// reminder that the sourceID corresponds to the sprite sheet for a given layer
		// and every layer will have their own sprite sheet. Consequently, layer-specific
		// objects will have identical sourceIds.
		// "atlasPos" corresponds to the location on a given sprite sheet that a specific object
		// (i.e. "claw", "factory", "floor tile", "leftrail") will correspond to.
		
		public FactoryLayerObjects(int OGX, int OGY, Vector2I atlasPos, int altTitle) 
		: base(OGX, OGY, layerSourceId, atlasPos, altTitle) {}
	}
}

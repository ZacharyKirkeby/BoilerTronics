// TODO: implement in more detail
using Godot;
using System;
using BoilerTronicsObjects.Layers;
using BoilerTronicsObjects.Placeable;

namespace BoilerTronicsObjects.Objects.ClawLayerObjects {
	
	public abstract class ClawLayerObjects : PlaceableObject {
		
		static int layerSourceId = 5;
		// note: the '5' is a placeholder, but reminder that the sourceID corresponds to the sprite sheet
		// and every layer will have their own sprite sheet. Consequently, layer-specific
		// objects will have identical sourceIds.
		// "atlasPos" corresponds to the location on a given sprite sheet that a specific object
		// (i.e. "claw", "factory", "floor tile", "leftrail") will correspond to.
		
		public ClawLayerObjects(int OGX, int OGY, Vector2I atlasPos, int altTitle) 
		: base(OGX, OGY, layerSourceId, atlasPos, altTitle) {}
	}
}

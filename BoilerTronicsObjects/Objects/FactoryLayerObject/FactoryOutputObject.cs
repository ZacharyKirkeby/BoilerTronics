// TODO: implement in more detail
using Godot;
using System;
using BoilerTronicsObjects.Layers;
using BoilerTronicsObjects.Objects.FactoryLayerObjects;
using BoilerTronicsObjects.Placeable;

namespace BoilerTronicsObjects.Objects.FactoryLayerObjects {

	public class FactoryOutputObject : FactoryLayerObjects {
		
		static Vector2I objectAtlasPos = new Vector2I(0, 1);
		// "atlasPos" corresponds to the location on a given sprite sheet that a specific object
		// (i.e. "claw", "factory", "floor tile", "leftrail") will correspond to.
		
		public FactoryOutputObject(int OGX, int OGY, int altTitle = 0) 
		: base(OGX, OGY, objectAtlasPos, altTitle) {}
	}
}

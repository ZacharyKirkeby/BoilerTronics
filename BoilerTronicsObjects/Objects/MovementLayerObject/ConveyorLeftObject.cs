// TODO: implement in more detail
using Godot;
using System;
using BoilerTronicsObjects.Layers;
using BoilerTronicsObjects.Objects.FactoryLayerObjects;
using BoilerTronicsObjects.Placeable;
using BoilerTronicsObjects.Interfaces;

namespace BoilerTronicsObjects.Objects.MovementLayerObjects {

	public class ConveyorLeftObject : MovementLayerObjects {

		static Vector2I objectAtlasPos = new Vector2I(0, 0);

		public ConveyorLeftObject(int OGX, int OGY, int altTitle = 0) 
		: base(OGX, OGY, objectAtlasPos, altTitle) {}
	}
}

// TODO: implement in more detail
using Godot;
using System;
using BoilerTronicsObjects.Layers;
using BoilerTronicsObjects.Objects.FactoryLayerObjects;
using BoilerTronicsObjects.Placeable;
using BoilerTronicsObjects.Interfaces;

namespace BoilerTronicsObjects.Objects.FactoryLayerObjects {

	public class FactoryInputObject : PlaceableObject, Movable {
		
		public override int GetCost() { return 0; }
		public new static int GetCostStatic() { return 0; }
		
		// default visuals
		static int layerSourceId = 0;
		static Vector2I objectAtlasPos = new Vector2I(0, 0);
		// "atlasPos" corresponds to the location on a given sprite sheet that a specific object
		// (i.e. "claw", "factory", "floor tile", "leftrail") will correspond to.
		
		// internal input object
		private int _objectID;

		// reminder that the sourceID corresponds to the sprite sheet for a given layer
		// and every layer will have their own sprite sheet. Consequently, layer-specific
		// objects will have identical sourceIds.
		
		public PlaceableObject PickUp() {
			PlaceableObject obj = ObjectFactory.GenerateObject(_objectID);
			GD.Print("Generating obj: ", obj);
			obj.SetGarbage(true); // Tell the layer to throw it away on reset
			return obj;
		}
		
		public bool Place(PlaceableObject obj) {
			return false;
		}
		
		public FactoryInputObject(int OGX, int OGY, int altTitle = 0, int objectID = 200)
		: base(OGX, OGY, layerSourceId, objectAtlasPos, altTitle) {
			_objectID = objectID;
			
			switch(_objectID) {
				case 200:
					// coal variant
					SetAtlasPos(new Vector2I(1, 0));
					break;
				case 201:
					// iron ore variant
					SetAtlasPos(new Vector2I(2, 0));
					break;
				case 202:
					// iron bar variant
					SetAtlasPos(new Vector2I(3, 0));
					break;
				case 203:
					// iron plate variant
					SetAtlasPos(new Vector2I(0, 2));
					break;
				case 204:
					// iron rod variant
					SetAtlasPos(new Vector2I(1, 2));
					break;
				default:
					SetAtlasPos(new Vector2I(2, 2)); // Place holder
					GD.Print("FactoryInputObject: Warning: InputObject does not have visual variant for object id: ", _objectID);
					break;;
			}
		}
	}
}

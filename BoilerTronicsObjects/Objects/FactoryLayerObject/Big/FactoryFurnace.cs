// TODO: implement in more detail
using Godot;
using System;
using System.Collections.Generic;
using BoilerTronicsObjects.Layers;
using BoilerTronicsObjects.Objects.FactoryLayerObjects;
using BoilerTronicsObjects.Placeable;
using BoilerTronicsObjects.Interfaces;

namespace BoilerTronicsObjects.Objects.FactoryLayerObjects {

	public class FactoryFurnace : PlaceableBig, Movable {
		
		static Vector2I objectAtlasPos = new Vector2I(2, 0);
		// "atlasPos" corresponds to the location on a given sprite sheet that a specific object
		// (i.e. "claw", "factory", "floor tile", "leftrail") will correspond to.
		
		static int layerSourceId = 3;
		private int _objectID;
		
		private PlaceableObject insertObj;
		private PlaceableObject outputObj;

		// reminder that the sourceID corresponds to the sprite sheet for a given layer
		// and every layer will have their own sprite sheet. Consequently, layer-specific
		// objects will have identical sourceIds.
		
		public PlaceableObject PickUp() {
			
			PlaceableObject obj = ObjectFactory.GenerateObject(_objectID);
			GD.Print("Generating obj: ", obj);
			return obj;
		}
		
		public bool Place(PlaceableObject obj) {
			return false;
		}
		
		public FactoryFurnace(int OGX, int OGY, int altTitle = 0, int objectID = 200)
		: base(OGX, OGY, layerSourceId, objectAtlasPos, altTitle) {
			_objectID = objectID;
			
			/*
			
			// Demo of creating specific slots to have specific behaviors
			PlaceableObject insertionPoint = ObjectFactory.GenerateObject(int objectId, 0, 0);
			
			dir0.Add(new PlaceableBigData(
				new Vector2I(0, 0),		// offset from object's origin
				new TileTex(0, 0, 3),	// atlasX, atlasY, sourceId
				insertionPoint
			));
			*/
			
			// internal insert, output objects
			insertObj = new FactoryFurnaceInput(0, 0, 0);
			outputObj = new FactoryFurnaceOutput(0, 0, 0);
			
			/*
			  []			 
			[]{}
			*/
			List<PlaceableBigData> dir0 = new List<PlaceableBigData>();
			// update the list
			dir0.Add(new PlaceableBigData(
				new Vector2I(0, 0),		// offset from object's origin
				new TileTex(0, 0, 3),	// atlasX, atlasY, sourceId
				insertObj
			));
			dir0.Add(new PlaceableBigData(
				new Vector2I(-1, 0),		// offset from object's origin
				new TileTex(0, 0, 3),	// atlasX, atlasY, sourceId
				insertObj
			));
			dir0.Add(new PlaceableBigData(
				new Vector2I(1, -1),		// offset from object's origin
				new TileTex(0, 1, 3),	// atlasX, atlasY, sourceId
				outputObj
			));
			
			// update the texture grid
			SetTextureGrid(dir0, 0);
			
			/*
			[]{}
			  []
			*/
			List<PlaceableBigData> dir1 = new List<PlaceableBigData>();
			// update the list
			dir1.Add(new PlaceableBigData(
				new Vector2I(0, 0),		// offset from object's origin
				new TileTex(1, 0, 3),	// atlasX, atlasY, sourceId
				insertObj
			));
			dir1.Add(new PlaceableBigData(
				new Vector2I(-1, 1),		// offset from object's origin
				new TileTex(1, 0, 3),	// atlasX, atlasY, sourceId
				insertObj
			));
			dir1.Add(new PlaceableBigData(
				new Vector2I(-1, 0),		// offset from object's origin
				new TileTex(0, 1, 3),	// atlasX, atlasY, sourceId
				outputObj
			));
			// update the texture grid
			SetTextureGrid(dir1, 1);
			
			/*
			{}[]
			[]
			*/
			List<PlaceableBigData> dir2 = new List<PlaceableBigData>();
			// update the list
			dir2.Add(new PlaceableBigData(
				new Vector2I(0, 0),		// offset from object's origin
				new TileTex(2, 0, 3),	// atlasX, atlasY, sourceId
				insertObj
			));
			dir2.Add(new PlaceableBigData(
				new Vector2I(1, 0),		// offset from object's origin
				new TileTex(2, 0, 3),	// atlasX, atlasY, sourceId
				insertObj
			));
			dir2.Add(new PlaceableBigData(
				new Vector2I(-1, 1),		// offset from object's origin
				new TileTex(0, 1, 3),	// atlasX, atlasY, sourceId
				outputObj
			));
			// update the texture grid
			SetTextureGrid(dir2, 2);
			
			/*
			[]
			{}[]
			*/
			List<PlaceableBigData> dir3 = new List<PlaceableBigData>();
			// update the list
			dir3.Add(new PlaceableBigData(
				new Vector2I(0, 0),		// offset from object's origin
				new TileTex(2, 0, 3),	// atlasX, atlasY, sourceId
				insertObj
			));
			dir3.Add(new PlaceableBigData(
				new Vector2I(1, -1),		// offset from object's origin
				new TileTex(2, 0, 3),	// atlasX, atlasY, sourceId
				insertObj
			));
			dir3.Add(new PlaceableBigData(
				new Vector2I(1, 0),		// offset from object's origin
				new TileTex(0, 1, 3),	// atlasX, atlasY, sourceId
				outputObj
			));
			// update the texture grid
			SetTextureGrid(dir3, 3);
			
			
			SetDir(0);
		}
	}
}

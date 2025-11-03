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
		private List<PlaceableBigData>[] objectData;

		/*
		 * This is a static data structure that stores just the structure and texture data of the big object
		 * This will be deep copied on instantiating an object
		 * Other wise this will be used to stically create textures without instanciating an object
		 */
		private static List<PlaceableBigData>[] textureGrid = new List<PlaceableBigData>[] {
			// Up direction
			new List<PlaceableBigData> {
				// update the list
				new PlaceableBigData(
						new Vector2I(0, 0),	// offset from object's origin
						new TileTex(0, 0, 3),	// atlasX, atlasY, sourceId
						null
						),
				new PlaceableBigData(
						new Vector2I(-1, 0),	// offset from object's origin
						new TileTex(0, 0, 3),	// atlasX, atlasY, sourceId
						null
						),
				new PlaceableBigData(
						new Vector2I(1, -1),	// offset from object's origin
						new TileTex(0, 1, 3),	// atlasX, atlasY, sourceId
						null
						),
				new PlaceableBigData(
						new Vector2I(0, -1),	// offset from object's origin
						new TileTex(3, 1, 3),	// atlasX, atlasY, sourceId
						null
						),
			},
			// Down direction
			new List<PlaceableBigData> {
				new PlaceableBigData(
						new Vector2I(0, 0),	// offset from object's origin
						new TileTex(1, 0, 3),	// atlasX, atlasY, sourceId
						null
						),
				new PlaceableBigData(
						new Vector2I(-1, 1),	// offset from object's origin
						new TileTex(1, 0, 3),	// atlasX, atlasY, sourceId
						null
						),
				new PlaceableBigData(
						new Vector2I(-1, 0),	// offset from object's origin
						new TileTex(0, 1, 3),	// atlasX, atlasY, sourceId
						null
						),
				new PlaceableBigData(
						new Vector2I(0, 1),	// offset from object's origin
						new TileTex(3, 1, 3),	// atlasX, atlasY, sourceId
						null
						)
			},
			// Left direction
			new List<PlaceableBigData> {
				new PlaceableBigData(
						new Vector2I(0, 0),	// offset from object's origin
						new TileTex(2, 0, 3),	// atlasX, atlasY, sourceId
						null
						),
				new PlaceableBigData(
						new Vector2I(1, 0),	// offset from object's origin
						new TileTex(2, 0, 3),	// atlasX, atlasY, sourceId
						null
						),
				new PlaceableBigData(
						new Vector2I(-1, 1),	// offset from object's origin
						new TileTex(0, 1, 3),	// atlasX, atlasY, sourceId
						null
						),
				new PlaceableBigData(
						new Vector2I(0, 1),	// offset from object's origin
						new TileTex(3, 1, 3),	// atlasX, atlasY, sourceId
						null
						),
			},
			// Right direction
			new List<PlaceableBigData> {
				new PlaceableBigData(
						new Vector2I(0, 0),	// offset from object's origin
						new TileTex(2, 0, 3),	// atlasX, atlasY, sourceId
						null
						),
				new PlaceableBigData(
						new Vector2I(1, -1),	// offset from object's origin
						new TileTex(2, 0, 3),	// atlasX, atlasY, sourceId
						null
						),
				new PlaceableBigData(
						new Vector2I(1, 0),	// offset from object's origin
						new TileTex(0, 1, 3),	// atlasX, atlasY, sourceId
						null
						),
				new PlaceableBigData(
						new Vector2I(0, -1),	// offset from object's origin
						new TileTex(3, 1, 3),	// atlasX, atlasY, sourceId
						null
						),
			},
		};

		// reminder that the sourceID corresponds to the sprite sheet for a given layer
		// and every layer will have their own sprite sheet. Consequently, layer-specific
		// objects will have identical sourceIds.
		
		// TODO: implement 'GetDataAtPos(int x, int y)' or 'GetDataAtPos(Vector2I)'
		// i.e. this object must somehow get the reference of the claw that is interacting
		// with this object, then return "GetDataAtPos(ClawObject.GetCurrPos()).GetInternalObj()"
		public PlaceableObject PickUp() {
			
			// PlaceableObject obj = ObjectFactory.GenerateObject(_objectID);
			// GD.Print("Generating obj: ", obj);
			// return obj;
			
			return null;
		}
		
		// TODO: implement 'GetDataAtPos(int x, int y)' or 'GetDataAtPos(Vector2I)'
		// i.e. this object must somehow get the reference of the claw that is interacting
		// with this object, then get "GetDataAtPos(ClawObject.GetCurrPos()).GetInternalObj()"
		// as a PlaceableObject (i.e. 'dataObj')
		// then return the output of "dataObj.Place(obj)"
		public bool Place(PlaceableObject obj) {
			return false;
		}
		
		// REMINDER:
		// the entirety of the object's visuals/internal objects are generated here!
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

			/** The internal refrence to the input nad output objects must be set here **/

			/*
			||[]			 
			[]{}
			*/

			// in
			// in
			// out
			// null
			
			/*
			[]{}
			||[]
			*/

			// in
			// in
			// out
			// null
			
			/*
			{}[]
			[]||
			*/

			// in
			// in
			// out
			// null
			
			/*
			[]||
			{}[]
			*/

			// in
			// in
			// out
			// null
			
			SetDir(Direction.UP);
		}

		public override Texture GetTexture() {
			return GetBigTexture(GetTextureGrid());
		}
		
		public override List<PlaceableBigData> GetTextureGrid() {
			return GetTextureGrid(this.GetDir());
		}

		public override List<PlaceableBigData> GetTextureGrid(Direction inDir) {
			switch (inDir) {
				case Direction.UP:
					return textureGrid[0];
				case Direction.DOWN:
					return textureGrid[1];
				case Direction.LEFT:
					return textureGrid[2];
				case Direction.RIGHT:
					return textureGrid[3];
			}
			return null;
		}
	}
}

using Godot;
using System;
using System.Collections.Generic;
using BoilerTronicsObjects.Layers;
using BoilerTronicsObjects.Objects.FactoryLayerObjects;
using BoilerTronicsObjects.Placeable;
using BoilerTronicsObjects.Interfaces;

namespace BoilerTronicsObjects.Objects.FactoryLayerObjects {

	public class FactoryRoller : PlaceableBig, BigMovable {
		
		static Vector2I objectAtlasPos = new Vector2I(0, 2);
		// "atlasPos" corresponds to the location on a given sprite sheet that a specific object
		// (i.e. "claw", "factory", "floor tile", "leftrail") will correspond to.
		
		static int layerSourceId = 3;
		private int _objectID;
		
		private FactoryBigObjectInput Input;
		private FactoryBigObjectOutput Output;
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
						new TileTex(1, 2, 3),	// atlasX, atlasY, sourceId
						null
						),
				new PlaceableBigData(
						new Vector2I(-1, 0),	// offset from object's origin
						new TileTex(1, 2, 3),	// atlasX, atlasY, sourceId
						null
						),
				new PlaceableBigData(
						new Vector2I(1, -1),	// offset from object's origin
						new TileTex(2, 2, 3),	// atlasX, atlasY, sourceId
						null
						),
			},
			// Down direction
			new List<PlaceableBigData> {
				new PlaceableBigData(
						new Vector2I(0, 0),	// offset from object's origin
						new TileTex(0, 2, 3),	// atlasX, atlasY, sourceId
						null
						),
				new PlaceableBigData(
						new Vector2I(1, -1),	// offset from object's origin
						new TileTex(0, 2, 3),	// atlasX, atlasY, sourceId
						null
						),
				new PlaceableBigData(
						new Vector2I(0, -1),	// offset from object's origin
						new TileTex(2, 2, 3),	// atlasX, atlasY, sourceId
						null
						),
			},
			// Left direction
			new List<PlaceableBigData> {
				new PlaceableBigData(
						new Vector2I(0, 0),	// offset from object's origin
						new TileTex(0, 2, 3),	// atlasX, atlasY, sourceId
						null
						),
				new PlaceableBigData(
						new Vector2I(2, -1),	// offset from object's origin
						new TileTex(2, 2, 3),	// atlasX, atlasY, sourceId
						null
						),
				new PlaceableBigData(
						new Vector2I(1, -1),	// offset from object's origin
						new TileTex(0, 2, 3),	// atlasX, atlasY, sourceId
						null
						),
			},
			// Right direction
			new List<PlaceableBigData> {
				new PlaceableBigData(
						new Vector2I(0, 0),	// offset from object's origin
						new TileTex(1, 2, 3),	// atlasX, atlasY, sourceId
						null
						),
				new PlaceableBigData(
						new Vector2I(-1, 0),	// offset from object's origin
						new TileTex(1, 2, 3),	// atlasX, atlasY, sourceId
						null
						),
				new PlaceableBigData(
						new Vector2I(0, -1),	// offset from object's origin
						new TileTex(2, 2, 3),	// atlasX, atlasY, sourceId
						null
						),
			},
		};

		public PlaceableObject PickUp(Vector2I pos) {
			return null;
		}
		
		public bool Place(PlaceableObject obj, Vector2I pos) {
			return false;
		}

		public bool GiveObject(PlaceableObject obj, PlaceableObject childObj) {
			return false; // We do not want this object
		}

		public PlaceableObject RequestObject(int requestId, PlaceableObject childObj) {
			return null; // We don't ahve that object
		}
		
		// REMINDER:
		// the entirety of the object's visuals/internal objects are generated here!
		public FactoryRoller(int OGX, int OGY, int altTitle = 0, int objectID = 200)
		: base(OGX, OGY, layerSourceId, objectAtlasPos, altTitle) {
			_objectID = objectID;
			
			// Deep copy static data to object data
			objectData = PlaceableBigData.Copy2DList(textureGrid);
			
			// internal insert, output objects
			Input = new FactoryBigObjectInput(0, 0, 0);
			Output = new FactoryBigObjectOutput(0, 0, 0);

			/** The internal refrence to the input nad output objects must be set here **/

			// TODO: Verify these 

			// UP

			objectData[0][0].SetInternalObj(null);
			objectData[0][1].SetInternalObj(Input);
			objectData[0][2].SetInternalObj(Output);

			// DOWN
			objectData[1][0].SetInternalObj(Input);
			objectData[1][1].SetInternalObj(null);
			objectData[1][2].SetInternalObj(Output);

			// LEFT
			objectData[2][0].SetInternalObj(Input);
			objectData[2][1].SetInternalObj(Output);
			objectData[2][2].SetInternalObj(null);

			// RIGHT
			objectData[3][0].SetInternalObj(Input);
			objectData[3][1].SetInternalObj(null);
			objectData[3][2].SetInternalObj(Output);
			
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

		public static List<PlaceableBigData> StaticGetTextureGrid(Direction dir) {
			switch (dir) {
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

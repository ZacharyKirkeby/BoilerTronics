// TODO: implement in more detail
using Godot;
using System;
using System.Collections.Generic;
using BoilerTronicsObjects.Layers;
using BoilerTronicsObjects.Objects.FactoryLayerObjects;
using BoilerTronicsObjects.Placeable;
using BoilerTronicsObjects.Interfaces;
using BoilerTronicsObjects.Data;

namespace BoilerTronicsObjects.Objects.FactoryLayerObjects {

	public class FactoryFurnace : PlaceableBig, BigMovable, Runnable {
		
		static Vector2I objectAtlasPos = new Vector2I(0, 0);
		// "atlasPos" corresponds to the location on a given sprite sheet that a specific object
		// (i.e. "claw", "factory", "floor tile", "leftrail") will correspond to.
		
		static int layerSourceId = 3;
		private int _objectID;

		private int _Fule;
		private PlaceableObject _Inv;
		private bool _Working;
		private int _StepsTillCompletion;
		
		private FactoryBigObjectInput materialIn;
		private FactoryBigObjectInput coalIn;
		private FactoryBigObjectOutput materialOut;
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
						new Vector2I(-1, 0),	// offset from object's origin
						new TileTex(3, 1, 3),	// atlasX, atlasY, sourceId
						null
						),
				new PlaceableBigData(
						new Vector2I(1, -1),	// offset from object's origin
						new TileTex(1, 0, 3),	// atlasX, atlasY, sourceId
						null
						),
				new PlaceableBigData(
						new Vector2I(0, -1),	// offset from object's origin
						new TileTex(0, 1, 3),	// atlasX, atlasY, sourceId
						null
						)
			},
			// Left direction
			new List<PlaceableBigData> {
				new PlaceableBigData(
						new Vector2I(0, 0),	// offset from object's origin
						new TileTex(3, 1, 3),	// atlasX, atlasY, sourceId
						null
						),
				new PlaceableBigData(
						new Vector2I(-1, 0),	// offset from object's origin
						new TileTex(0, 1, 3),	// atlasX, atlasY, sourceId
						null
						),
				new PlaceableBigData(
						new Vector2I(1, -1),	// offset from object's origin
						new TileTex(2, 0, 3),	// atlasX, atlasY, sourceId
						null
						),
				new PlaceableBigData(
						new Vector2I(0, -1),	// offset from object's origin
						new TileTex(2, 0, 3),	// atlasX, atlasY, sourceId
						null
						),
			},
			// Right direction
			new List<PlaceableBigData> {
				new PlaceableBigData(
						new Vector2I(0, 0),	// offset from object's origin
						new TileTex(0, 1, 3),	// atlasX, atlasY, sourceId
						null
						),
				new PlaceableBigData(
						new Vector2I(1, -1),	// offset from object's origin
						new TileTex(3, 1, 3),	// atlasX, atlasY, sourceId
						null
						),
				new PlaceableBigData(
						new Vector2I(-1, 0),	// offset from object's origin
						new TileTex(3, 0, 3),	// atlasX, atlasY, sourceId
						null
						),
				new PlaceableBigData(
						new Vector2I(0, -1),	// offset from object's origin
						new TileTex(3, 0, 3),	// atlasX, atlasY, sourceId
						null
						),
			},
		};

		public FactoryFurnace(int OGX, int OGY, int altTitle = 0, int objectID = 200)
		: base(OGX, OGY, layerSourceId, objectAtlasPos, altTitle) {
			_objectID = objectID;
			
			// Deepcopy grid
			objectData = PlaceableBigData.Copy2DList(textureGrid);
			
			// internal insert, output objects
			materialIn = new FactoryBigObjectInput(0, 0, 0);
			coalIn = new FactoryBigObjectInput(0, 0, 0);
			materialOut = new FactoryBigObjectOutput(0, 0, 0);

			// Set the parent to this object so that our ins and outs can make cbs
			materialOut.SetParent(this);
			materialIn.SetParent(this);
			coalIn.SetParent(this);

			// Set values we use to do the prcess
			_Fule = 0;
			_Inv = null;
			_Working = false;
			_StepsTillCompletion = 0;

			RegisterSteppable();

			/** The internal refrence to the input nad output objects must be set here **/

			// UP

			objectData[0][0].SetInternalObj(materialOut);
			objectData[0][1].SetInternalObj(materialIn);
			objectData[0][2].SetInternalObj(coalIn);
			objectData[0][3].SetInternalObj(null);

			// DOWN
			objectData[1][0].SetInternalObj(materialIn);
			objectData[1][1].SetInternalObj(null);
			objectData[1][2].SetInternalObj(materialOut);
			objectData[1][3].SetInternalObj(coalIn);

			// LEFT
			objectData[2][0].SetInternalObj(null);
			objectData[2][1].SetInternalObj(coalIn);
			objectData[2][2].SetInternalObj(materialIn);
			objectData[2][3].SetInternalObj(materialOut);

			// RIGHT
			objectData[3][0].SetInternalObj(coalIn);
			objectData[3][1].SetInternalObj(null);
			objectData[3][2].SetInternalObj(materialOut);
			objectData[3][3].SetInternalObj(materialIn);

			
			SetDir(Direction.UP);
		}

		// reminder that the sourceID corresponds to the sprite sheet for a given layer
		// and every layer will have their own sprite sheet. Consequently, layer-specific
		// objects will have identical sourceIds.
		
		private PlaceableBigData findDataAtPos(List<PlaceableBigData> D, Vector2I P) {
			foreach (PlaceableBigData BD in D) {
				if (BD.GetPosition(this.GetCurrPos()) == P) return BD;
			}

			return null;
		}

		public PlaceableObject PickUp(Vector2I pos) {
			// Get our data at our current dir
			List<PlaceableBigData> D = GetTextureGrid();
			// Get the internal obj at this pos
			PlaceableBigData BD = findDataAtPos(D, pos);
			PlaceableObject obj = BD.GetInternalObj();
			// Get the obj if we can
			PlaceableObject ret = null;
			// We don't really care what the object is, the actual checking for materials will be done in the call back functions that the child object will make to the parent
			// These call will also provide a refrence to the child objct. Throught this refrence we can checl what specific IN/OUT it is and act accordlingly
			if (obj != null && obj is Movable mObj) ret = mObj.PickUp();
			// Return the obj
			return ret;
		}
		
		public bool Place(PlaceableObject obj, Vector2I pos) {
			// Get our data at our current dir
			List<PlaceableBigData> D = GetTextureGrid();
			// Get the internal obj at this pos
			PlaceableBigData BD = findDataAtPos(D, pos);
			PlaceableObject iObj = BD.GetInternalObj();
			// Place in the obj if we can
			if (obj != null && iObj is Movable mObj) return mObj.Place(obj);
			// Otherwise we don't want that shit
			return false;
		}

		public bool GiveObject(PlaceableObject obj, PlaceableObject childObj) {
			if (childObj == coalIn) {
				// We need to check and see if the object coming in is coal
				// If so we wnat to do somthing and return true to accept it
				if (obj is CoalObject) {
					GD.Print("We go fule");
					_Fule += 5;
					return true;
				}
			}
			else if (childObj == materialIn) {
				// We need to check and see if the object coming in is a smealtable material
				// If so we wnat to do somthing and return true to accept it
				if ((!_Working) && (obj is IronOreObject) && (_Inv == null)) {
					GD.Print("We go ore");
					_StepsTillCompletion = 2;
					_Inv = new IronBarObject(0, 0, 0) as PlaceableObject;
					_Inv.SetGarbage(true);
					_Working = true;
					materialOut.SetValidObj(BoilerTronicsData.objectMap[BoilerTronicsData.hashCoords(_Inv.GetSourceID(), _Inv.GetAtlasPos())]);
					return true;
				} else {
					GD.Print(_Working);
					GD.Print(obj);
					GD.Print(_Inv);
				}
			}
			else if (childObj == materialOut) {
				return false; // Why the fuck is out output trying to give us something
			}

			return false; // WTF is this shit, fuck you
		}

		public PlaceableObject RequestObject(int requestId, PlaceableObject childObj) {
			if (childObj == coalIn) {
				return null; // Why is someone trying to take from our input?
			}
			else if (childObj == materialIn) {
				return null; // Why is someone trying to take from our input?
			}
			else if (childObj == materialOut) {
				// We need to check and see if we have the object that they are  requesting ready to return
				// If so we wnat to retunr that obj and remove it from our inv
				if ((_Inv != null) && (BoilerTronicsData.objectMap[BoilerTronicsData.hashCoords(_Inv.GetSourceID(), _Inv.GetAtlasPos())] == requestId) && !_Working) {
					PlaceableObject tmp = _Inv;
					_Inv = null;
					return tmp;
				}
			}

			return null; // Invalid childObject
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
					return objectData[0];
				case Direction.DOWN:
					return objectData[1];
				case Direction.LEFT:
					return objectData[2];
				case Direction.RIGHT:
					return objectData[3];
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

		// Runnable Interface
		public void Step()
		{
			// If we are working and have fule
			if (_Working && _Fule > 0) {
				// Then we tak a step to completion
				_StepsTillCompletion--;
				// And use some fule
				_Fule--;

				// Once we are done
				if (_StepsTillCompletion == 0) {
					GD.Print("We done");
					// Stop working
					_Working = false;
				}
			}
		}

		public void RegisterSteppable()
		{
			BoilerTronicsGlobalManager manager = BoilerTronicsGlobalManager.GlobalManager;
			manager.currLevel.RegisterRunnable(this);
		}

		public void UnRegisterSteppable()
		{
			BoilerTronicsGlobalManager manager = BoilerTronicsGlobalManager.GlobalManager;
			manager.currLevel.UnRegisterRunnable(this);
		}

		public void Reset() {
			_Working = false;
			_Fule = 0;
			_StepsTillCompletion = 0;

			if (_Inv != null) _Inv.ResetPos();
			_Inv = null;
		}

	}
}

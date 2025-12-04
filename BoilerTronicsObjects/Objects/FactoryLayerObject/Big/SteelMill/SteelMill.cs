using Godot;
using System;
using System.Collections.Generic;
using BoilerTronicsObjects.Layers;
using BoilerTronicsObjects.Objects.FactoryLayerObjects;
using BoilerTronicsObjects.Placeable;
using BoilerTronicsObjects.Interfaces;
using BoilerTronicsObjects.Data;

namespace BoilerTronicsObjects.Objects.FactoryLayerObjects {

	public class SteelMill : PlaceableBig, BigMovable, Runnable, BigGroupedSubObject {
		
		public override int GetCost() { return 100; }
		public new static int GetCostStatic() { return 100; }
		
		static Vector2I objectAtlasPos = new Vector2I(0, 2);
		// "atlasPos" corresponds to the location on a given sprite sheet that a specific object
		// (i.e. "claw", "factory", "floor tile", "leftrail") will correspond to.
		
		static int layerSourceId = 3;
		private int _objectID;
		
		private FactoryBigObjectInput Input;
		private FactoryBigObjectOutput Output;
		private SteelLubeIntake LubeIntake;
		private List<PlaceableBigData>[] objectData;

		private PlaceableObject _Inv;
		private bool _Working;
		private int _StepsTillCompletion;

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
						new TileTex(0, 6, 3),	// atlasX, atlasY, sourceId
						null
						),
				new PlaceableBigData(
						new Vector2I(-1, 0),	// offset from object's origin
						new TileTex(1, 7, 3),	// atlasX, atlasY, sourceId
						null
						),
				new PlaceableBigData(
						new Vector2I(1, -1),	// offset from object's origin
						new TileTex(1, 7, 3),	// atlasX, atlasY, sourceId
						null
						),
				new PlaceableBigData(
						new Vector2I(0, -1),	// offset from object's origin
						new TileTex(0, 7, 3),	// atlasX, atlasY, sourceId
						null
						),
			},
			// Down direction
			new List<PlaceableBigData> {
				new PlaceableBigData(
						new Vector2I(0, 0),	// offset from object's origin
						new TileTex(1, 7, 3),	// atlasX, atlasY, sourceId
						null
						),
				new PlaceableBigData(
						new Vector2I(-1, 0),	// offset from object's origin
						new TileTex(0, 7, 3),	// atlasX, atlasY, sourceId
						null
						),
				new PlaceableBigData(
						new Vector2I(1, -1),	// offset from object's origin
						new TileTex(0, 6, 3),	// atlasX, atlasY, sourceId
						null
						),
				new PlaceableBigData(
						new Vector2I(0, -1),	// offset from object's origin
						new TileTex(1, 7, 3),	// atlasX, atlasY, sourceId
						null
						)
			},
			// Left direction
			new List<PlaceableBigData> {
				new PlaceableBigData(
						new Vector2I(0, 0),	// offset from object's origin
						new TileTex(0, 7, 3),	// atlasX, atlasY, sourceId
						null
						),
				new PlaceableBigData(
						new Vector2I(-1, 0),	// offset from object's origin
						new TileTex(1, 7, 3),	// atlasX, atlasY, sourceId
						null
						),
				new PlaceableBigData(
						new Vector2I(1, -1),	// offset from object's origin
						new TileTex(1, 7, 3),	// atlasX, atlasY, sourceId
						null
						),
				new PlaceableBigData(
						new Vector2I(0, -1),	// offset from object's origin
						new TileTex(0, 6, 3),	// atlasX, atlasY, sourceId
						null
						),
			},
			// Right direction
			new List<PlaceableBigData> {
				new PlaceableBigData(
						new Vector2I(0, 0),	// offset from object's origin
						new TileTex(1, 7, 3),	// atlasX, atlasY, sourceId
						null
						),
				new PlaceableBigData(
						new Vector2I(1, -1),	// offset from object's origin
						new TileTex(0, 7, 3),	// atlasX, atlasY, sourceId
						null
						),
				new PlaceableBigData(
						new Vector2I(-1, 0),	// offset from object's origin
						new TileTex(0, 6, 3),	// atlasX, atlasY, sourceId
						null
						),
				new PlaceableBigData(
						new Vector2I(0, -1),	// offset from object's origin
						new TileTex(1, 7, 3),	// atlasX, atlasY, sourceId
						null
						),
			},
		};

		public SteelMill(int OGX, int OGY, int altTitle = 0, int objectID = 200)
		: base(OGX, OGY, layerSourceId, objectAtlasPos, altTitle) {
			_objectID = objectID;
			
			// Deep copy static data to object data
			objectData = PlaceableBigData.Copy2DList(textureGrid);
			
			// internal insert, output objects
			Input = new FactoryBigObjectInput(0, 0, 0);
			Output = new FactoryBigObjectOutput(0, 0, 0);
			LubeIntake = new SteelLubeIntake(0, 0, 0); // Lube intake

			// Set the parent object of our in and out, this will allow for cbs
			Input.SetParent(this);
			Output.SetParent(this);

			// Set values we use to do the prcess
			_Inv = null;
			_Working = false;
			_StepsTillCompletion = 0;

			RegisterSteppable();

			/** The internal refrence to the input nad output objects must be set here **/

			// UP

			objectData[0][0].SetInternalObj(Input);
			objectData[0][1].SetInternalObj(null);
			objectData[0][2].SetInternalObj(null);
			objectData[0][3].SetInternalObj(Output);

			// DOWN
			objectData[1][0].SetInternalObj(null);
			objectData[1][1].SetInternalObj(Output);
			objectData[1][2].SetInternalObj(Input);
			objectData[1][3].SetInternalObj(null);

			// LEFT
			objectData[2][0].SetInternalObj(Output);
			objectData[2][1].SetInternalObj(null);
			objectData[2][2].SetInternalObj(null);
			objectData[2][3].SetInternalObj(Input);

			// RIGHT
			objectData[3][0].SetInternalObj(null);
			objectData[3][1].SetInternalObj(Output);
			objectData[3][2].SetInternalObj(Input);
			objectData[3][3].SetInternalObj(null);

			
			SetDir(Direction.UP);
		}

		public override void SetDir(Direction inputDir) {
			Vector2I pos = this.GetOGPos();

			switch (inputDir) {
				case PlaceableBig.Direction.UP:
					LubeIntake.MoveObject(pos.X + 0, pos.Y + 0);
					break;
				case PlaceableBig.Direction.DOWN:
					LubeIntake.MoveObject(pos.X + 1, pos.Y - 1);
					break;
				case PlaceableBig.Direction.LEFT:
					LubeIntake.MoveObject(pos.X + 0, pos.Y - 1);
					break;
				case PlaceableBig.Direction.RIGHT:
					LubeIntake.MoveObject(pos.X - 1, pos.Y + 0);
					break;
			}

			base.SetDir(inputDir);
		}

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
			if (childObj == Input) {
				if ((!_Working) && (obj is SteelPlateObject spO) && (spO.hasHeat()) && (_Inv == null)) {
					_StepsTillCompletion = 1;
					SteelGearObject sgO = new SteelGearObject(0, 0, 0);
					sgO.setHeat(spO.getHeatValue()); // Transfer heat
					_Inv = sgO as PlaceableObject;
					_Inv.SetGarbage(true);
					_Working = true;
					Output.SetValidObj(BoilerTronicsData.objectMap[BoilerTronicsData.hashCoords(_Inv.GetSourceID(), _Inv.GetAtlasPos())]);
					return true;
				}
				// We need to check and see if the object coming in is valid
				// If so we wnat to do somthing and return true to accept it
			}
			else if (childObj == Output) {
				return false; // Why the fuck is out output trying to give us something
			}

			return false; // WTF is this shit, fuck you
		}

		public PlaceableObject RequestObject(int requestId, PlaceableObject childObj) {
			if (childObj == Input) {
				return null; // Why is someone trying to take from our input?
			}
			else if (childObj == Output) {
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
			if (_Working) {
				// Then we tak a step to completion
				if (LubeIntake.ConsumeLube()) _StepsTillCompletion--; // We need lub to mill

				// Once we are done
				if (_StepsTillCompletion == 0) {
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
			_StepsTillCompletion = 0;

			if (_Inv != null) _Inv.ResetPos();
			_Inv = null;
		}

		public List<GroupedSubObject> getObjects() {
			List<GroupedSubObject> ret = new List<GroupedSubObject>();

			// Set coords
			Vector2I Objpos = this.GetOGPos();
			switch (this.GetDir()) {
				case PlaceableBig.Direction.UP:
					LubeIntake.MoveObject(Objpos.X + 0, Objpos.Y + 0);
					break;
				case PlaceableBig.Direction.DOWN:
					LubeIntake.MoveObject(Objpos.X + 1, Objpos.Y - 1);
					break;
				case PlaceableBig.Direction.LEFT:
					LubeIntake.MoveObject(Objpos.X + 0, Objpos.Y - 1);
					break;
				case PlaceableBig.Direction.RIGHT:
					LubeIntake.MoveObject(Objpos.X - 1, Objpos.Y + 0);
					break;
			}

			ret.Add(LubeIntake);

			return ret;
		}

		// Grouped interface
		public GroupedSubObject getGroupedObject(Vector2I pos) {
			GD.Print("requested: ", pos);
			GD.Print("Input Loc: ", LubeIntake.GetOGPos());
			if (LubeIntake.GetOGPos() == pos) return LubeIntake as GroupedSubObject;
			return null;
		}
	}
}

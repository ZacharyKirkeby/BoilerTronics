using Godot;
using System;
using System.Collections.Generic;
using Parsing;
using BoilerTronicsObjects.Layers;
using BoilerTronicsObjects.Objects.MovementLayerObjects;
using BoilerTronicsObjects.Objects.ClawLayerObjects;
using BoilerTronicsObjects.Placeable;
using BoilerTronicsObjects.Interfaces;

namespace BoilerTronicsObjects.Objects.MovementLayerObjects {

	public class  SwitchObject: PlaceableBig, Scriptable, Runnable {
		
		public override int GetCost() { return 100; }
		public new static int GetCostStatic() { return 100; }
		
		static Vector2I objectAtlasPos = new Vector2I(2, 0);
		// "atlasPos" corresponds to the location on a given sprite sheet that a specific object
		// (i.e. "claw", "factory", "floor tile", "leftrail") will correspond to.
		
		static int layerSourceId = 2;
		private int _objectID;
		private Parser _parser;
		private CodeEdit E;
		
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
						new TileTex(2, 0, 2),	// atlasX, atlasY, sourceId
						null
						),
				new PlaceableBigData(
						new Vector2I(1, -1),	// offset from object's origin
						new TileTex(1, 0, 2),	// atlasX, atlasY, sourceId
						null
						),
			},
			// Down direction
			new List<PlaceableBigData> {
				// update the list
				new PlaceableBigData(
						new Vector2I(0, 0),	// offset from object's origin
						new TileTex(2, 1, 2),	// atlasX, atlasY, sourceId
						null
						),
				new PlaceableBigData(
						new Vector2I(-1, 0),	// offset from object's origin
						new TileTex(1, 1, 2),	// atlasX, atlasY, sourceId
						null
						),
			},
			// Left direction
			new List<PlaceableBigData> {
				// update the list
				new PlaceableBigData(
						new Vector2I(0, 0),	// offset from object's origin
						new TileTex(1, 0, 2),	// atlasX, atlasY, sourceId
						null
						),
				new PlaceableBigData(
						new Vector2I(-1, 1),	// offset from object's origin
						new TileTex(2, 0, 2),	// atlasX, atlasY, sourceId
						null
						),
			},
			// Right direction
			new List<PlaceableBigData> {
				// update the list
				new PlaceableBigData(
						new Vector2I(0, 0),	// offset from object's origin
						new TileTex(2, 1, 2),	// atlasX, atlasY, sourceId
						null
						),
				new PlaceableBigData(
						new Vector2I(-1, 0),	// offset from object's origin
						new TileTex(1, 1, 2),	// atlasX, atlasY, sourceId
						null
						),
			},
		};
		
		public SwitchObject(int OGX, int OGY, int altTitle = 0, int objectID = 200)
		: base(OGX, OGY, layerSourceId, objectAtlasPos, altTitle) {
			_objectID = objectID;

			_parser = new Parser();
			_parser._Ready();
			CreateTerminal(); // We need to create a terminal so that the user can actually write a script
			RegisterSteppable(); // Registers this as a runnable with the level state
			
			SetDir(Direction.UP);
		}

		// Runnable Interface

		public void Step() {
			// Make a call to the parser
			BoilerTronicsGlobalManager manager = BoilerTronicsGlobalManager.GlobalManager;
			if (_parser == null)
			{
				GD.PrintErr($"{GetType().Name}: Parser not initialized!");
				return;
			}
			int highlight = _parser.ParseGetLine(this, E, E.Text, manager.currLevel.StepCount, E.Name);
			if (highlight >= 0) E.HighlightLine(highlight, new Color(1, 1, 1, 0.3f));
		}

		public void Reset() {
			base.ResetPos();
			_parser.Reset();
			E.ClearAllHighlights();
			var existing = E.GetNodeOrNull<Label>("ErrorLabel");
			if (existing != null)
			{
				existing.QueueFree();
			}
		}

		public void RegisterSteppable() {
			BoilerTronicsGlobalManager manager = BoilerTronicsGlobalManager.GlobalManager;
			manager.currLevel.RegisterRunnable(this);
		}

		public void UnRegisterSteppable() {
			BoilerTronicsGlobalManager manager = BoilerTronicsGlobalManager.GlobalManager;
			manager.currLevel.UnRegisterRunnable(this);
		}

		// Scriptable Interface

		// Methods to deal with terminals
		public CodeEdit GetTerminal() {
			return E;
		}

		public void CreateTerminal() {
			BoilerTronicsGlobalManager manager = BoilerTronicsGlobalManager.GlobalManager;
			Terminals currTerminal = manager.terminalContainer;
			E = manager.terminalContainer.AddEditor();
			E.Name = "Switch";
			
			E.SetCorrespondingObject(this);
		}

		public void DestroyTerminal() {
			BoilerTronicsGlobalManager manager = BoilerTronicsGlobalManager.GlobalManager;
			manager.terminalContainer.RemoveEditor(E);
			E = null;
		}

		public void SetScript(string script) {
			E.Text = script;
		}

		public string GetScript()
		{
			return E.Text;
		}
		
		public void SetParser(Parser parser)
		{
			this._parser = parser;
		}
		
		public Parser GetParser()
		{
			return this._parser;
		}

		// Methods that we can use via commands
		public void Move(string[] args) {
			return; // Throw error
		}

		public void Grab(string[] args) {
			return; // Throw error
		}

		public void Drop(string[] args) {
			return; // Throw error
		}

		public void Rotate(string[] args) {
			return; // Throw error
		}

		public void Switch(string[] args) {
			Vector2I off;

			switch (GetDir()) {
				case Direction.UP:
					off = new Vector2I(1, -1);
					break;
				case Direction.LEFT:
					off = new Vector2I(-1, 1);
					break;
				// Down and Right are the same
				case Direction.DOWN:
				case Direction.RIGHT:
					off = new Vector2I(-1, 0);
					break;
				default:
					return; // Not a valid position ( This should never happen )
			}

			Vector2I pos1 = GetCurrPos();		// Position of orgin
			Vector2I pos2 = GetCurrPos() + off;	// Position of the other spot
			
			// Get manager so that we can access the diffrent layers
			BoilerTronicsGlobalManager manager = BoilerTronicsGlobalManager.GlobalManager;

			// Get rail and claw on pos 1 and remove them from the layer
			ClawObject C1 = manager.currLevel.cLayer.FindObject(pos1) as ClawObject;
			if (C1 != null) manager.currLevel.cLayer.RemoveObject(C1);
			TrackObject R1 = manager.currLevel.rLayer.FindObject(pos1) as TrackObject;
			if (R1 != null) manager.currLevel.rLayer.RemoveObject(R1);

			// Get rail and claw on pos 2 and remove them from the layer
			ClawObject C2 = manager.currLevel.cLayer.FindObject(pos2) as ClawObject;
			if (C2 != null) manager.currLevel.cLayer.RemoveObject(C2);
			TrackObject R2 = manager.currLevel.rLayer.FindObject(pos2) as TrackObject;
			if (R2 != null) manager.currLevel.rLayer.RemoveObject(R2);

			// Place rail and claw from pos 1 on pos 2
			if (C1 != null) {
				C1.MoveCurrPos(pos2.X, pos2.Y);
				manager.currLevel.cLayer.AddObject(C1);
			}

			if (R1 != null) {
				R1.MoveCurrPos(pos2.X, pos2.Y);
				manager.currLevel.rLayer.AddObject(R1);
			}

			// Place rail and claw from pos 2 on pos 1
			if (C2 != null) {
				C2.MoveCurrPos(pos1.X, pos1.Y);
				manager.currLevel.cLayer.AddObject(C2);
			}

			if (R2 != null) {
				R2.MoveCurrPos(pos1.X, pos1.Y);
				manager.currLevel.rLayer.AddObject(R2);
			}
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

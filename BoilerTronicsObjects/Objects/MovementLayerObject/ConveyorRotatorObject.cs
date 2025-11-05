using Godot;
using System;
using Parsing;
using BoilerTronicsObjects.Layers;
using BoilerTronicsObjects.Objects.FactoryLayerObjects;
using BoilerTronicsObjects.Objects.ClawLayerObjects;
using BoilerTronicsObjects.Placeable;
using BoilerTronicsObjects.Interfaces;

namespace BoilerTronicsObjects.Objects.MovementLayerObjects {

	public class ConveyorRotatorObject : MovementLayerObjects, Scriptable, Runnable {

		static Vector2I objectAtlasPos = new Vector2I(0, 2);
		private Parser _parser;
		CodeEdit E;
		// "atlasPos" corresponds to the location on a given sprite sheet that a specific object
		// (i.e. "claw", "factory", "floor tile", "leftrail") will correspond to.
		
		public ConveyorRotatorObject(int OGX, int OGY, int altTitle = 0) : base(OGX, OGY, objectAtlasPos, altTitle) {
			_parser = new Parser();
			_parser._Ready();
			CreateTerminal(); // We need to create a terminal so that the user can actually write a script
			RegisterSteppable(); // Registers this as a runnable with the level state
		} // create object

		// Methods to deal with terminals
		public CodeEdit GetTerminal() {
			return E;
		}

		public void CreateTerminal() {
			BoilerTronicsGlobalManager manager = BoilerTronicsGlobalManager.GlobalManager;
			E = manager.terminalContainer.AddEditor();
			E.Name = "Rotator";
			
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
			// Rotate rail object below us
			BoilerTronicsGlobalManager manager = BoilerTronicsGlobalManager.GlobalManager;
			// Get track obj on rail layer
			PlaceableObject rail = manager.currLevel.rLayer.FindObject(this.GetPos());
			GD.Print("Rail:", rail);
			if (rail == null) return; // Can't rotate if there's no rail
			if (!(rail is TrackObject tObj)) return; // This is not a rail object
			GD.Print("tObj:", tObj);

			// Change its position
			int currdir = tObj.GetDir();
			GD.Print("currDir:", currdir);
			switch (currdir) {
				case 0:
					tObj.ChangeDir(1);
					break;
				case 1:
					tObj.ChangeDir(0);
					break;
			}
			
			BoilerTronicsSoundManager soundManager = BoilerTronicsSoundManager.SoundManager;
			soundManager.PlaySound(SoundType.Rotate);

			GD.Print("newDir:", tObj.GetDir());
		}

		// Override 'save' function to also return a script's information
		public override Godot.Collections.Dictionary<string, Variant> Save()
		{
			Godot.Collections.Dictionary<string, Variant> res = base.Save();
			// GD.Print("TODO: override per-object serialization to also include corresponding CodeEdit information");
			res["terminalCode"] = GetScript();
			return res;
		}
	}
}

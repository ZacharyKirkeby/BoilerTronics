// TODO: implement in more detail
using Godot;
using System;
using System.Linq;
using BoilerTronicsObjects.Layers;
using BoilerTronicsObjects.Objects.ClawLayerObjects;
using BoilerTronicsObjects.Placeable;
using BoilerTronicsObjects.Interfaces;

namespace BoilerTronicsObjects.Objects.ClawLayerObjects {

	public class ClawObject : ClawLayerObjects, Scriptable, Runnable {
		
		static Vector2I objectAtlasPos = new Vector2I(0, 0);
		private PlaceableObject heldObject = null;

		// "atlasPos" corresponds to the location on a given sprite sheet that a specific object
		// (i.e. "claw", "factory", "floor tile", "leftrail") will correspond to.

		// Runnable Interface
		public void Step() {
			BoilerTronicsGlobalManager manager = BoilerTronicsGlobalManager.GlobalManager;

			MovingObject mObj = new MovingObject(this, new Vector2I(1,0), manager.currLevel.cLayer, 1);
			manager.currLevel.cLayer.GetParent().AddChild(mObj);

			return;
			string[] cmdAndArgs = GetNextCommand(); // get command and args from interrupter
			ScriptableCommand cmd = GetCommand(cmdAndArgs[0]); // get command
			string[] cmdArgs = cmdAndArgs.Skip(1).ToArray(); // isolate args
			cmd(cmdArgs); // run command
		}

		public void RegisterSteppable() {
			BoilerTronicsGlobalManager manager = BoilerTronicsGlobalManager.GlobalManager;
			manager.currLevel.RegisterRunnable(this);
		}

		public void UnRegisterSteppable() {
			BoilerTronicsGlobalManager manager = BoilerTronicsGlobalManager.GlobalManager;
			manager.currLevel.UnRegisterRunnable(this);
		}

		public void Reset() {
			base.ResetPos();
			heldObject = null;
			// Maybe need to make a call to our codeEdit/interrputer?
		}

		// Scriptable interface

		// Methods to get commands and execute commands
		public string[] GetNextCommand() {
			// TODO: Implement
			return null;
		}

		public ScriptableCommand GetCommand(string commandWord) {
			// TODO: Implement
			return null;
		}

		// Methods to deal with terminals
		public CodeEdit GetTerminal() {
			// TODO: Implement
			return null;
		}

		public void CreateTerminal() {
			// TODO: Implement
		}

		public void DestroyTerminal() {
			// TODO: Implement
		}

		// functions that we can use via commands

		public void MoveCommand(params object[] args)
		{
		}

		public void GrabCommand(params object[] args)
		{
		}

		public void DropCommand(params object[] args)
		{
		}

		// Command methods
		public ClawObject(int OGX, int OGY, int altTitle = 0) : base(OGX, OGY, objectAtlasPos, altTitle) {
			CreateTerminal(); // We need to create a terminal so that the user can actually write a script
			RegisterSteppable(); // Registers this as a runnable with the level state
		} // create object

		~ClawObject() {
			DestroyTerminal(); // Destries the terminal for this scriptable
		}
		
		// Override 'save' function to also return CodeEdit terminal information
		public override Godot.Collections.Dictionary<string, Variant> Save()
		{
			Godot.Collections.Dictionary<string, Variant> res = Save();
			GD.Print("TODO: override per-object serialization to also include corresponding CodeEdit information");
			// res["terminalCode"] == (CodeEdit.GetText())
			return res;
		}
	}
}

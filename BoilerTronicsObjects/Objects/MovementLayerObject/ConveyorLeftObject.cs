// TODO: implement in more detail
using Godot;
using System;
using BoilerTronicsObjects.Layers;
using BoilerTronicsObjects.Objects.FactoryLayerObjects;
using BoilerTronicsObjects.Placeable;
using BoilerTronicsObjects.Interfaces;

namespace BoilerTronicsObjects.Objects.MovementLayerObjects {

	public class ConveyorLeftObject : MovementLayerObjects, Scriptable, Runnable {

		static Vector2I objectAtlasPos = new Vector2I(0, 0);

		public ConveyorLeftObject(int OGX, int OGY, int altTitle = 0) 
		: base(OGX, OGY, objectAtlasPos, altTitle) {}

		public string[] GetNextCommand() {
			// TODO: Implement
			return null;
		}

		public ScriptableCommand GetCommand(string commandWord) {
			// TODO: Implement
			return null;
		}

		public void CreateTerminal() {
			// TODO: Implement
		}

		public CodeEdit GetTerminal() {
			// TODO: Implement
			return null;
		}

		public void DestroyTerminal() {
			// TODO: Implement
		}

		public void Step() {
			// TODO: Implement
		}

		public void Reset() {
			// TODO: Implement
		}
		
		public void RegisterSteppable() {
			// TODO: Implement
		}

		public void UnRegisterSteppable() {
			// TODO: Implement
		}
	}
}

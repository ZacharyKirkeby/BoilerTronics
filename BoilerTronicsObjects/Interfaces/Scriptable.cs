// TODO: placeholder
using Godot;
using System;

namespace BoilerTronicsObjects.Scriptable {
	
	public interface ScriptableObject {
		// All Scriptable interface methods should return 'bool' for success and etc
		public bool Move(Vector2 movementVector);
		public bool Grab();
	}
}

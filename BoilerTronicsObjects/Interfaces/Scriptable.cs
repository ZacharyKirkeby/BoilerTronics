using Godot;
using System;

namespace BoilerTronicsObjects.Interfaces {
	public delegate void ScriptableCommand(string[] args); // allows us to have method/function pointers

	interface Scriptable {
		// Methods for the commands that the parser will try to run
		void Move(string[] args); // u d l r
		void Grab(string[] args); // should be null
		void Drop(string[] args); // should be null
		void Rotate(string[] args); // l r

		// Methods to deal with terminals
		void CreateTerminal(); // Registers the runnable object with the global manager
		CodeEdit GetTerminal(); // Get the terminal that has all the code for this object
		void DestroyTerminal(); // Unregisters the runnable object with the global manager
	}
}

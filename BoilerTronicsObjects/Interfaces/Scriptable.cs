<<<<<<< HEAD
using Godot;
using System;

namespace BoilerTronicsObjects.Interfaces {
	public delegate void ScriptableCommand(string[] args); // allows us to have method/function pointers

	interface Scriptable {
		// Methods to get commands and execute commands
		string[] GetNextCommand(); // Steps the object
		ScriptableCommand GetCommand(string commandWord); // Gets the function that the command points to

		// Methods to deal with terminals
		void CreateTerminal(); // Registers the runnable object with the global manager
		CodeEdit GetTerminal(); // Get the terminal that has all the code for this object
		void DestroyTerminal(); // Unregisters the runnable object with the global manager
	}
}

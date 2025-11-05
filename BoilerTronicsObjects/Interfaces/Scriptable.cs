using Godot;
using Parsing;
using System;

namespace BoilerTronicsObjects.Interfaces {
	interface Scriptable {
		// Methods for the commands that the parser will try to run
		void Move(string[] args); // u d l r
		void Grab(string[] args); // should be null
		void Drop(string[] args); // should be null
		void Switch(string[] args); // should be null
		void Rotate(string[] args); // l r

		// Methods to deal with terminals
		void CreateTerminal(); // Registers the runnable object with the global manager
		CodeEdit GetTerminal(); // Get the terminal that has all the code for this object
		void DestroyTerminal(); // Unregisters the runnable object with the global manager
		void SetScript(string script); // sets the text of the code edit
		string GetScript(); // sets the text of the code edit
		void SetParser(Parser parser);
		Parser GetParser();
	}
}

using Godot;
using System;

namespace BoilerTronicsObjects.Interfaces {
	interface Runnable {
		void Step(); // Steps the object
		void Reset(); // Resets back to the origonal state
		void RegisterSteppable(); // Registers the runnable object with the global manager
		void UnRegisterSteppable(); // Unregisters the runnable object with the global manager
	}
}

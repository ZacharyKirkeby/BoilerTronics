using Godot;
using System;

namespace BoilerTronicsObjects.Interfaces {
	interface TimeConsumingObject {
		// This function should register the object with the global level and be called when the object is instantiater
		void registerConsumingObject();
		// This function should unregister the object with the global level and be called then the time consuming action is complete
		void unregisterConsumingObject();
		// This funciton should be called to stop the object in it's current state (stay in the same position/ stop same from in animation)
		void haultObject();
	}
}

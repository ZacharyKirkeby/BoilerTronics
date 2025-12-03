using Godot;
using System;
using BoilerTronicsObjects.Placeable;
using BoilerTronicsObjects.Objects.ClawLayerObjects;

namespace BoilerTronicsObjects.Interfaces {
	interface HeatedMaterial {

		// This will return true if the heat value is > 0
		bool hasHeat();

		// Gets the heat value
		int getHeatValue();

		// Sets heat value to the value passed in
		void setHeat(int HV);

		// This is needed so that we can update the claw that is holding us if we change states while being held
		void setHook(ClawObject cObj);
	}
}


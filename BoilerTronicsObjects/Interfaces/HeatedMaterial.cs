using Godot;
using System;
using BoilerTronicsObjects.Placeable;

namespace BoilerTronicsObjects.Interfaces {
	interface HeatedMaterial {

		// This will return true if the heat value is > 0
		bool hasHeat();

		// Gets the heat value
		int getHeatValue();

		// Sets heat value to the value passed in
		void setHeat(int HV);
	}
}


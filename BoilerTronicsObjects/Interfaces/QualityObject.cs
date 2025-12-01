using Godot;
using Parsing;
using System;

namespace BoilerTronicsObjects.Interfaces {
	public enum Quality {
		LOW_QUALITY = 0,
		MID_QUALITY = 1,
		HIGH_QUALITY = 2,
	};

	interface QualityObject {

		void SetQuality(Quality newQuality);
		Quality GetQuality();
	}
}

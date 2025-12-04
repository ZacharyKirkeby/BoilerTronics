using Godot;
using System;
using System.Collections.Generic;
using BoilerTronicsObjects.Layers;
using BoilerTronicsObjects.Objects.ClawLayerObjects;
using BoilerTronicsObjects.Placeable;
using BoilerTronicsObjects.Interfaces;

namespace BoilerTronicsObjects.Objects.FactoryLayerObjects {

	/*
	 * The purpose of this class is to provide a base for the pump objects for the seperate liquids.
	 * The default pump object should not bu used, but instead it should be inherited by the specific object
	 */
	public class SteelLubeIntake : PipeObject {

		private PipeGroup group;

		Quality Q;

		// This constructor shouldn't be used as we will never use the base pump
		public SteelLubeIntake(int OGX, int OGY, int altTitle = 0) : base(OGX, OGY, 0, new Vector2I(0,0), altTitle) {
		}

		public bool ConsumeLube() {
			PipeGroup pg = this.getGroup() as PipeGroup;
			if (pg == null) return false;
			return pg.consumeLiquid(PipeGroup.LiquidType.Lube, 10);
		}

		/*
		 * This function will be used from pipe objects,
		 * The purpose of this function is to allow for pipes to see if they are able to connect to the pipe
		 * The only way for the pipe to be able to be connected is if it is on the correct direction of the pipe
		 */
		public override bool CanConnect(PipeObject p) {
			return true;
		}
		
		public override void UpdateSprite() {
			// We never want to update the sprite bc there is no sprite lol
		}
	}
}

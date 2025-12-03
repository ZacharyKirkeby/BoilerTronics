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
	public class CoolerObject : PipeObject, Movable, Runnable {

		public override int GetCost() { return 100; }
		public new static int GetCostStatic() { return 100; }

		static int layerSourceId = 3;
		static Vector2I atlasPos = new Vector2I(0,5);

		private PlaceableObject obj;

		private PipeGroup group;

		// For loading purposes, have a specific string that will override its parent's group contents
		private string internalText = null;

		// This constructor shouldn't be used as we will never use the base pump
		public CoolerObject(int OGX, int OGY, int altTitle = 0) : base(OGX, OGY, layerSourceId, atlasPos, altTitle) {
			RegisterSteppable();
		}

		public override void UpdateSprite() {
			// We never want to update the sprite
		}

		// Movable Interface

		public PlaceableObject PickUp() {
			PlaceableObject ret = obj;
			obj = null;

			return ret;
		}

		public bool Place(PlaceableObject obj) {
			if (this.obj == null) {
				this.obj = obj;
				return true;
			}

			return false;
		}

		// Runnable interface
		
		public void Step() {
			PipeGroup parent = this.getGroup() as PipeGroup;

			// Cool object if we have one, it's heated, and we have water to cool it
			if (obj is HeatedMaterial hm && hm.hasHeat() && parent.consumeLiquid(PipeGroup.LiquidType.Water, 10)) hm.setHeat(0);
		}

		public void Reset() {
			obj = null;
		}

		public void RegisterSteppable() {
			BoilerTronicsGlobalManager man = BoilerTronicsGlobalManager.GlobalManager;

			man.currLevel.RegisterRunnable(this);
		}

		public void UnRegisterSteppable() {
			BoilerTronicsGlobalManager man = BoilerTronicsGlobalManager.GlobalManager;

			man.currLevel.UnRegisterRunnable(this);
		}
	}
}

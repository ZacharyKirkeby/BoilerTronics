using Godot;
using System;
using BoilerTronicsObjects.Layers;
using BoilerTronicsObjects.Objects.FactoryLayerObjects;
using BoilerTronicsObjects.Placeable;
using BoilerTronicsObjects.Interfaces;

namespace BoilerTronicsObjects.Objects.FactoryLayerObjects {

	public class FactoryTestMaterial : PlaceableObject, Movable, Runnable, HeatedMaterial{
		
		private static Vector2I objectAtlasPos = new Vector2I(0, 4); // This is a dummy sprinte | TODO: Change this (not for this tesing object but for the actual object)
		private static int layerSourceId = 0;

		// Used to keep track of how 'hot' the item is
		private int heatValue = 0;
		
		public override void ResetPos()
		{
			BoilerTronicsGlobalManager manager = BoilerTronicsGlobalManager.GlobalManager;
			manager.currLevel.fLayer.RemoveObject(this);
			UnRegisterSteppable();
			this.MoveObject(-1, -1); // Move to an invalid position
			base.ResetPos();
		}

		// Heated Material interface

		// This will return true if the heat value is > 0
		public bool hasHeat() {
			return heatValue > 0;
		}

		// Gets the heat value
		public int getHeatValue() {
			return heatValue;
		}

		// Sets heat value to the value passed in
		public void setHeat(int HV) {
			heatValue = HV;
		}

		// Moveable interfact

		// Moveable, this will allow us to pickup and drop off items
		public PlaceableObject PickUp() {
			// Remove ourselves from the layer we exist in (factory layer)
			BoilerTronicsGlobalManager manager = BoilerTronicsGlobalManager.GlobalManager;
			manager.currLevel.fLayer.RemoveObject(this);
			return this; // What ever is grabing this should manage this objct and place it again at some point
		}
		
		public bool Place(PlaceableObject obj) {
			return false; // This is a material, you can't place anything in us
		}

		// Runnable interface
		// This is used because we wnat the item to decrease in heat every step

		public void Step() {
			if (heatValue > 0) heatValue--;
		}

		public void Reset() {
			heatValue = 0;
			ResetPos();
		}

		public void RegisterSteppable() {
			BoilerTronicsGlobalManager manager = BoilerTronicsGlobalManager.GlobalManager;

			manager.currLevel.RegisterRunnable(this);
		}

		public void UnRegisterSteppable() {
			BoilerTronicsGlobalManager manager = BoilerTronicsGlobalManager.GlobalManager;

			manager.currLevel.UnRegisterRunnable(this);
		}

		public FactoryTestMaterial(int OGX, int OGY, int altTitle = 0) 
		: base(OGX, OGY, layerSourceId, objectAtlasPos, altTitle) {}
	}
}

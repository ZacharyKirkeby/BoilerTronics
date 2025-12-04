using Godot;
using System;
using BoilerTronicsObjects.Layers;
using BoilerTronicsObjects.Objects.FactoryLayerObjects;
using BoilerTronicsObjects.Objects.ClawLayerObjects;
using BoilerTronicsObjects.Placeable;
using BoilerTronicsObjects.Interfaces;

namespace BoilerTronicsObjects.Objects.FactoryLayerObjects {

	public class SteelPlateObject : PlaceableObject, Movable, Runnable, HeatedMaterial{
		
		private static Vector2I headtedAtlasPos = new Vector2I(3, 2);
		private static Vector2I cooledAtlasPos = new Vector2I(3, 1);

		private static int layerSourceId = 9;

		// Used to keep track of how 'hot' the item is
		private ClawObject holder = null;
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

			if (heatValue > 0) {
				// Set the sprite to the heated atlasp pos
				// Unsure if the following is needed as we will only be heated if we are in a heater or a furnace
				// Tell the hook that is holding us that we got heated (if we are being held)
			}
		}

		// Let's a hook tell us that they are holding us so that we can update them when we change states
		public void setHook(ClawObject cObj) {
			holder = cObj;
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

			if (heatValue == 0) {
				// Change the sprite of the object
				this.SetAtlasPos(cooledAtlasPos);

				// If we have a holder we need to notify them we have a diffrent sprite
				if (holder != null) {
					holder.updateHeld();
				}

				// Tell the hook that is holding us that we got heated (if we are being held)
				BoilerTronicsGlobalManager man = BoilerTronicsGlobalManager.GlobalManager;

				if (man.currLevel.fLayer.FindObject(this.GetCurrPos()) == this) {
					// set tile sprite at curr position
					man.currLevel.fLayer.SetCell(this.GetCurrPos(), this.GetSourceID(), this.GetAtlasPos()); // Set the new sprite
				}
			}
		}

		public void Reset() {
			heatValue = 0;
			ResetPos();
			if (holder != null) holder.deleteHeld();
			UnRegisterSteppable();
		}

		public void RegisterSteppable() {
			BoilerTronicsGlobalManager manager = BoilerTronicsGlobalManager.GlobalManager;

			manager.currLevel.RegisterRunnable(this);
		}

		public void UnRegisterSteppable() {
			BoilerTronicsGlobalManager manager = BoilerTronicsGlobalManager.GlobalManager;

			manager.currLevel.UnRegisterRunnable(this);
		}


		public SteelPlateObject(int OGX, int OGY, int altTitle = 0) 
		: base(OGX, OGY, layerSourceId, cooledAtlasPos, altTitle) {
			RegisterSteppable();
			this.SetGarbage(true); // This will be deleted on reset
		}
	}
}

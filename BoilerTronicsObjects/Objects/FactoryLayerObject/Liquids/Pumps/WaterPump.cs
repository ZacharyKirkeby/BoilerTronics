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
	public class WaterPump : PumpObject, Runnable {

		public static int layerSourceId = 14;

		public static Vector2I []atPosArr = {
			new Vector2I(0,0), // UP
			new Vector2I(1,0), // RIGHT
			new Vector2I(2,0), // DOWN
			new Vector2I(3,0)  // LEFT
		};

		public WaterPump(int OGX, int OGY, Quality Q, Placeable.Direction D, int altTitle = 0) : base(OGX, OGY, layerSourceId + (int) Q, atPosArr[(int) D], Q, D,altTitle) {
			RegisterSteppable();
		}

		public static Texture GetTexture(Quality Q, Placeable.Direction D) {
			// Get the texture based off the quality passed in
			Vector2I atPos = atPosArr[(int) D];

			var tileSet = GD.Load<TileSet>("res://Resources/objects.tres");

			// int sourceid = tileSet.GetSourceId(ID);
			TileSetAtlasSource tileSetSource = tileSet.GetSource(layerSourceId + (int) Q) as TileSetAtlasSource;

			// get the tile
			var tile = tileSetSource.GetTileTextureRegion(atPos);
			var fullTexture = tileSetSource.Texture.GetImage();
			var imageTexture = fullTexture.GetRegion(tile);
			ImageTexture T = new ImageTexture();

			T.SetImage(imageTexture);

			// Insert in list such that it is in the correct order to draw
			// (Figure this out later)

			return T;
		}

		// Runnable Interface

		public void Step()
		{
			PipeGroup PG = this.getGroup() as PipeGroup;

			PG.addLiquid(PipeGroup.LiquidType.Water, 10 * (int) this.GetQuality());
		}

		public void RegisterSteppable()
		{
			BoilerTronicsGlobalManager manager = BoilerTronicsGlobalManager.GlobalManager;
			manager.currLevel.RegisterRunnable(this);
		}

		public void UnRegisterSteppable()
		{
			BoilerTronicsGlobalManager manager = BoilerTronicsGlobalManager.GlobalManager;
			manager.currLevel.UnRegisterRunnable(this);
		}

		public void Reset() {
		}
	}
}

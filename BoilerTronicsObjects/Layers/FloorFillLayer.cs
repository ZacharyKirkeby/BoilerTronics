using Godot;
using System;
using System.Collections;
using BoilerTronicsObjects.Objects;
using BoilerTronicsObjects.Placeable;
using BoilerTronicsObjects.GameCamera;
using BoilerTronicsObjects.Interfaces;




/*
	This object exists solely to nicely generate a border for the play area
*/
namespace BoilerTronicsObjects.Layers
{
	public partial class FloorFillLayer : Godot.TileMapLayer
	{
		static BoilerTronicsGlobalManager manager = BoilerTronicsGlobalManager.GlobalManager;

		// default layer dimensions, if left unspecified
		static int startX = 10;
		static int startY = 10;

		int maxX;
		int maxY;

		public FloorFillLayer() {
			
			// make sure this renders as low as possible!
			this.SetZIndex(-100);
			// RedefineLayer(startX, startY);
		}
		
		// Given the dimensions of the layer to surround, a boundary size ("bufferSize"),
		// and the TileTex of the floor texture, fill this layer accordingly.
		// Note that the caller function should translate this layer appropriately!
		public void GenerateLayer(int newX, int newY, int bufferSize, TileTex floorTex) {
			maxX = newX + bufferSize * 2;
			maxY = newY + bufferSize * 2;
			
			// reset visuals
			// Clear();

			// fills all tiles
			for (int x = 0; x < maxX; x++) {
				for (int y = 0; y < maxY; y++) {
					SetCell(new Vector2I(x, y), floorTex.GetSourceId(), floorTex.GetAtlasPos());
				}
			}
			
			// clear any tiles that's a part of the play area
			for (int x = bufferSize; x < newX + bufferSize; x++) {
				for (int y = bufferSize; y < newY + bufferSize; y++) {
					// editableTiles[x, y] = true;
					SetCell(new Vector2I(x, y), -1, floorTex.GetAtlasPos());
				}
			}
			
		}
		public override void _Input(InputEvent @event)
		{
			base._Input(@event);
		}

	}
}

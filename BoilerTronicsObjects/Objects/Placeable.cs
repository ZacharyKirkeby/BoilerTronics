using Godot;
using System;
using BoilerTronicsObjects.Layers;

namespace BoilerTronicsObjects.Placeable
{
	public class PlaceableObject
	{
		// Values used to keep track of the position of the object and what sprite it is
		int OGX { get; set; }
		int OGY { get; set; }
		private int CurrX;
		private int CurrY;
		int sourceId { get; init; }               // This is the id of the tile map that the sprite belongs to
		Vector2I atlasPos { get; init; }          // Posistion on the atlas that the sprite is at
		int altTitle { get; init; }               // This will allow us to set the sprite to alternative sprites (unsure is this is needed, but we'll leave it here)

		public PlaceableObject(int OGX, int OGY, int sourceId, Vector2I atlasPos, int altTitle = 0)
		{
			this.OGX = OGX;
			this.OGY = OGY;
			this.CurrX = OGX;
			this.CurrY = OGY;
			
			// pass in invalid -1 value to disable this setter
			if (sourceId != -1) {
				this.sourceId = sourceId;
			}

			this.atlasPos = atlasPos;
			this.altTitle = altTitle;
		}

		public Vector2I GetOGPos()
		{
			return new Vector2I(OGX, OGY);
		}

		public Vector2I GetCurrPos()
		{
			return new Vector2I(CurrX, CurrY);
		}

		// move the OG posistion of the object
		public void MoveObject(int newX, int newY)
		{
			OGX = newX;
			OGY = newY;
		}

		public Vector2I GetPos()
		{
			return new Vector2I(CurrX, CurrY);
		}

		public int GetSourceID()
		{
			return sourceId;
		}

		public Vector2I GetAtlasPos()
		{
			return atlasPos;
		}

		public void ResetPos()
		{
			CurrX = OGX;
			CurrY = OGY;
		}
	}
}

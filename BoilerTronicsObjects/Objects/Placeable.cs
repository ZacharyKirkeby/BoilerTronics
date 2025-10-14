using Godot;
using System;
using System.Collections;
using BoilerTronicsObjects.Layers;

namespace BoilerTronicsObjects.Placeable
{
	public abstract class PlaceableObject
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
			this.OGX = newX;
			this.OGY = newY;
			this.CurrX = this.OGX;
			this.CurrY = this.OGY;
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
		public Texture GetTexture()
		{
			var tileSet = GD.Load<TileSet>("res://Resources/objects.tres");
			int sourceid = tileSet.GetSourceId(this.GetSourceID());

			TileSetAtlasSource tileSetSource = tileSet.GetSource(sourceid) as TileSetAtlasSource;

			// get the tile
			var tile = tileSetSource.GetTileTextureRegion(this.atlasPos);
			var fullTexture = tileSetSource.Texture.GetImage();
			var imageTexture = fullTexture.GetRegion(tile);
			var texture = new ImageTexture();

			return texture;
		}
		
		// should always return false, unless overriden by child object
		public bool Scriptable() {
			return false;
		}
		
		// a generic "save" function used to serialize per object information
		// note: this is very "lazy" for now!
		public Godot.Collections.Dictionary<string, Variant> Save()
		{
			// reminder: Vector2 is not supported by json! Must be isolated to composite (x, y) coordinates
			return new Godot.Collections.Dictionary<string, Variant>()
			{
				{ "OGX", OGX },
				{ "OGY", OGY },
				{ "sourceId", sourceId },
				{ "atlasPosX", atlasPos.X },
				{ "atlasPosY", atlasPos.Y },
				{ "altTitle", "null" },
			};
		}
	}
}

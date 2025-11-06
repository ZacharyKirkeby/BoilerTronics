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
		private bool Garbage;
		int sourceId { get; init; }             // This is the id of the tile map that the sprite belongs to
		Vector2I atlasPos;			// Posistion on the atlas that the sprite is at
		int altTitle;				// This will allow us to set the sprite to alternative sprites (unsure is this is needed, but we'll leave it here)

		// store the parent layer
		private Layer parentLayer;

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
			this.Garbage = false;
		}

		public void SetGarbage(bool isGarabage) {
			this.Garbage = isGarabage;
		}

		public bool GetGarbage() {
			return this.Garbage;
		}
		
		// set parent layer info; mainly useful for the terminal highlighting mechanism
		public Layer GetParentLayer() {
			return parentLayer;
		}
		public void SetParentLayer(Layer input) {
			if (input == null) { return; }
			parentLayer = input;
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
		
		public void MoveCurrPos(int newX, int newY) {
			this.CurrX = newX;
			this.CurrY = newY;
		}

		public Vector2I GetPos()
		{
			return new Vector2I(CurrX, CurrY);
		}

		public virtual int GetSourceID()
		{
			return sourceId;
		}

		public virtual Vector2I GetAtlasPos()
		{
			return atlasPos;
		}

		public void SetAtlasPos(Vector2I newAtlas)
		{
			this.atlasPos = newAtlas;
		}

		public virtual void ResetPos()
		{
			CurrX = OGX;
			CurrY = OGY;
		}

		public virtual Texture GetTexture()
		{
			var tileSet = GD.Load<TileSet>("res://Resources/objects.tres");
			int sourceid = tileSet.GetSourceId(this.GetSourceID());

			TileSetAtlasSource tileSetSource = tileSet.GetSource(sourceid) as TileSetAtlasSource;

			// get the tile
			var tile = tileSetSource.GetTileTextureRegion(this.atlasPos);
			var fullTexture = tileSetSource.Texture.GetImage();
			var imageTexture = fullTexture.GetRegion(tile);
			var texture = new ImageTexture();
			texture.SetImage(imageTexture);

			return texture;
		}
		
		// should always return false, unless overriden by child object
		// NOTE: this should be very redundant, given that "is interface" exists!
		// I (Ethen) didn't do enough research at the time;
		// consider this as redundant!
		// DEPRECATED
		public bool Scriptable() {
			return false;
		}
		
		// a generic "save" function used to serialize per object information
		// note: this is very "lazy" for now!
		// TODO: at some point, refactor to "Serialize" or something
		// this is otherwise a poorly named function!
		// "virtual" is used to allow this to be overridden by children methods
		public virtual Godot.Collections.Dictionary<string, Variant> Save()
		{
			// reminder: Vector2 is not supported by json! Must be isolated to composite (x, y) coordinates
			return new Godot.Collections.Dictionary<string, Variant>()
			{
				{ "OGX", OGX },
				{ "OGY", OGY },
				{ "sourceId", sourceId },
				{ "atlasPosX", atlasPos.X },
				{ "atlasPosY", atlasPos.Y },
				{ "altTitle", 0 },
			};
		}
		
		// Used for ArrayList.Contains and etc
		// Not 100% conclusive! Potential edge case is if two objects, identical on the surface level
		// and sharing the same coordinates, but on different Layers, this will
		// incorrectly return 'true'!
		public override bool Equals(object? obj) {
			if (!(obj is PlaceableObject)) return false;
			
			PlaceableObject cObj = (PlaceableObject) obj;
			
			return (
				this.GetOGPos() == cObj.GetOGPos() &&
				this.GetPos() == cObj.GetPos() &&
				this.GetSourceID() == cObj.GetSourceID() &&
				this.GetAtlasPos() == cObj.GetAtlasPos()
			);
		}
	}
}

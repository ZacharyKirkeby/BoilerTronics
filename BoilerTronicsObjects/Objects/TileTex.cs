using Godot;
using System;

// TODO: a basic "texture" object that will hold a tile texture's most important information
// TODO: integrate throughout all proper scripts and etc
// TODO: for now, only applicable for "res://Resources/objects.tres"
namespace BoilerTronicsObjects.Placeable {
	public class TileTex
	{
		private Vector2I atlasPos = new Vector2I();
		private int sourceId;
		
		// generic constructor
		public TileTex() {
			sourceId = -1;
		}
		
		// this is the constructor that *should* be used
		public TileTex(Vector2I atlasPos, int sourceId) {
			
			this.atlasPos.X = atlasPos.X;
			this.atlasPos.Y = atlasPos.Y;
			
			this.sourceId = sourceId;
		}
		
		// this one works too
		public TileTex(int inX, int inY, int sourceId) {
			
			this.atlasPos.X = inX;
			this.atlasPos.Y = inY;
			
			this.sourceId = sourceId;
		}
		
		// updates the values accordingly; does NOT replace this object's internal representation!
		public void SetAtlasPos(Vector2I atlasPos) {
			if (atlasPos == null) {
				return;
			}
			
			this.atlasPos.X = atlasPos.X;
			this.atlasPos.Y = atlasPos.Y;
		}
		
		public void SetSourceId(int input) {
			this.sourceId = input;
		}
		public void SetSourceID(int input) {
			SetSourceId(input);
		}
		
		// returns an independent Vector2I that should have the correct values.
		public Vector2I GetAtlasPos() {
			return new Vector2I(atlasPos.X, atlasPos.Y);
		}
		
		public int GetSourceId() {
			return this.sourceId;
		}
		public int GetSourceID() {
			return GetSourceId();
		}
		
		// TODO: is there something better syntaxically for this sort of operation?
		// i.e. produces a deep copy of the input
		public static TileTex Copy(TileTex input) {
			return new TileTex(input.GetAtlasPos(), input.GetSourceId());
		}
	}
}

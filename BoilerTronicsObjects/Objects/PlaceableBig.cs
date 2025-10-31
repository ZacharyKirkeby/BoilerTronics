using Godot;
using System;
using System.Collections;
using System.Collections.Generic;
using BoilerTronicsObjects.Layers;

namespace BoilerTronicsObjects.Placeable
{
	// child objects of PlaceableBIg should generate this statically (?)
	public class PlaceableBigData {
		private Vector2I offset = new Vector2I(0, 0);
		private TileTex texture;
		
		// generic constructor
		public PlaceableBigData() {
			this.texture = new TileTex();
		}
		
		// constructor that *should* be used
		public PlaceableBigData(Vector2I offset, TileTex texture) {
			this.offset.X = offset.X;
			this.offset.Y = offset.Y;
			
			this.texture = TileTex.Copy(texture);
		}
		
		// getters/setters
		public TileTex GetTileTex() {
			return this.texture;
		}
		public void SetTileTex(TileTex input) {
			this.texture = TileTex.Copy(input);
		}
		
		public Vector2I GetOffset() {
			return new Vector2I(offset.X, offset.Y);
		}
		public void SetOffset(Vector2I input) {
			this.offset.X = input.X;
			this.offset.Y = input.Y;
		}
		
		// given the object's origin, returns this data's position as origin + this.offset
		public Vector2I GetPosition(Vector2I origin) {
			return new Vector2I(origin.X + offset.X, origin.Y + offset.Y);
		}
	}
	
	public abstract class PlaceableBig : PlaceableObject
	{
		// keep track of the object's actual-actual origin
		// note: why does this exist? disabled until an actual reason exists.
		// can assume "origin" to be OGX/OGY
		// private Vector2I origin = new Vector2I(0, 0);
		private int dir; // 0-3, representing the four directions
		
		// "textureGrid" will be a list of (texture, offsetFromPlaceableBig) data
		// this is how we will construct the "big placeable" object
		// an array of size 4, each element of which will be a list of the object's textures corresponding to 'dir'
		private List<PlaceableBigData>[] textureGrid = new List<PlaceableBigData>[4];
		
		// TODO: a PlaceableBig should be given the correct information regarding the offets + textures
		// TODO: a child of a PlaceableBIg should accordingly set up its 'textureGrid' in this constructor
		public PlaceableBig(int OGX, int OGY, int sourceId, Vector2I atlasPos, int altTitle = 0) 
		: base(OGX, OGY, sourceId, atlasPos, altTitle)
		{
			// this.origin.X = OGX;
			// this.origin.Y = OGY;
		}
		
		// get this PlaceableBig's direction (0-3)
		public int GetDir() {
			return dir;
		}
		
		// set this PlaceableBig's direction (0-3)
		// if input is OOB, returns and does not affect 'dir'.
		public void SetDir(int inputDir) {
			if (inputDir < 0 || inputDir > 3) { return; }
			this.dir = inputDir;
		}
		
		// TODO: should be called by child objects to establish a PlaceableBig's textureGrid.
		// given the inputDir and the input list to be replaced, update accordingly
		// should only be "private" as only children objects should ever use this functionalityt!
		private void SetTextureGrid(List<PlaceableBigData> input, int inputDir) {
			this.textureGrid[inputDir] = input;
		}
		
		// TODO: return a deep copy instead of just the reference of this object's textureGrid
		// Returns the textureGrid according to this object's position
		public List<PlaceableBigData> GetTextureGrid() {
			return textureGrid[dir];
		}
		
		// TODO: return a deep copy instead of just the reference of this object's textureGrid
		// Returns the textureGrid according to the position specified by 'inputDir'
		// Returns 'null' if 'inputDir' is OOB.
		public List<PlaceableBigData> GetTextureGrid(int inputDir) {
			if (inputDir < 0 || inputDir > 3) { return null; }
			return textureGrid[inputDir];
		}
		
		// TODO: Keenan work this out!
		public override Texture GetTexture()
		{
			return null;
		}
		
	}
}

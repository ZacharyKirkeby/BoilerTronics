using Godot;
using System;
using System.Collections;
using System.Collections.Generic;
using BoilerTronicsObjects.Layers;

/*
	How this object should be handled:
	
Any PlaceableObject that wants to have a variable dimension (i.e. larger than just a 1x1 tile)
must extend this class. Extend the constructor however you'd like (should only require X, Y, "altTitle"
params) and etc.

In a child's constructor, they should call "SetTextureGrid()" and set up their "texture grids" accordingly.
Reminder that 0-3 represent the three directions (i.e. array of 3 Lists), and that all must be initialized
(to some extent) to function properly.

TODO: make an actual child object to test
*/

namespace BoilerTronicsObjects.Placeable
{
	// child objects of PlaceableBig should generate these in the constructor
	// and said children should update their 'textureGrid' accordingly there
	public class PlaceableBigData {
		private Vector2I offset = new Vector2I(0, 0);
		private TileTex texture;
		
		// Optional stuff, mainly helpful for factories and etc
		// with independent input/output on a per-tile basis
		private int frameIndex = 0;
		private List<TileTex> frames = new List<TileTex>();
		private PlaceableObject internalObject;
		// the "internalObject" should basically be a semi-dummy object that exists
		// only to handle locational input/output functionality
		
		// generic constructor
		public PlaceableBigData() {
			this.texture = new TileTex();
			
			frames.Add(
				new TileTex(texture.GetAtlasPos(), texture.GetSourceId())
			);
		}
		
		// constructor that *should* be used
		public PlaceableBigData(Vector2I offset, TileTex texture) {
			this.offset.X = offset.X;
			this.offset.Y = offset.Y;
			
			this.texture = TileTex.Copy(texture);
			
			// by default, always add the default visuals into frame slot 0
			frames.Add(
				new TileTex(texture.GetAtlasPos(), texture.GetSourceId())
			);
		}
		
		// constructor that automatically inserts an internal PlaceableObject
		public PlaceableBigData(Vector2I offset, TileTex texture, PlaceableObject obj) : this(offset, texture) {
			SetInternalObj(obj);
		}
		
		// getters/setters
		public TileTex GetTileTex() {
			return this.texture;
		}
		public void SetTileTex(TileTex input) {
			this.texture = TileTex.Copy(input);
			
			// update frame 0
			frames[0].SetSourceId(texture.GetSourceId());
			frames[0].SetAtlasPos(texture.GetAtlasPos());
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
		
		// --- OPTIONAL: Internal Object Functionality ---
		// should only be used by objects that require locational functions
		// i.e. locational input/output stuff
		public PlaceableObject GetInternalObj() {
			return internalObject;
		}
		public void SetInternalObj(PlaceableObject obj) {
			if (obj == null) { return; }
			internalObject = obj;
		}
		
		// --- OPTIONAL: Frame Functionality ---
		// (functionally identical to PlaceableFramed.cs)
		public TileTex GetFrame() {
			return frames[frameIndex];
		}
		
		// adds a frame to the internal list of frames
		// should only ever be called by child objects
		protected void AddFrame(TileTex input) {
			if (input == null) { return; }
			frames.Add(input);
		}
		
		// updates the internal frame index to the input
		// does nothing if input is OOB
		public void SetFrameIndex(int index) {
			if (index < 0) { return; }
			if (index >= frames.Count) { return; }
			frameIndex = index;
		}
		
		// resets the frame back to this object's original visuals
		public void ResetFrame() {
			frameIndex = 0;
		}
	}
	
	public abstract class PlaceableBig : PlaceableObject
	{
		public enum Direction {
			UP = 0,
			DOWN = 1,
			LEFT = 2,
			RIGHT = 3
		}

		// keep track of the object's actual-actual origin
		// note: why does this exist? disabled until an actual reason exists.
		// can assume "origin" to be OGX/OGY
		// private Vector2I origin = new Vector2I(0, 0);

		private Direction dir; // This will be the direction the that object is facing
		
		// TODO: a child of a PlaceableBig should accordingly set up its 'textureGrid' in this constructor
		public PlaceableBig(int OGX, int OGY, int sourceId, Vector2I atlasPos, int altTitle = 0) 
		: base(OGX, OGY, sourceId, atlasPos, altTitle)
		{
			// Empty, everything should be taken care of by the parent class
		}
		
		// get this PlaceableBig's direction (0-3)
		public Direction GetDir() {
			return dir;
		}
		
		// set this PlaceableBig's direction (0-3)
		// if input is OOB, returns and does not affect 'dir'.
		public void SetDir(Direction inputDir) {
			if (inputDir < Direction.UP || inputDir > Direction.RIGHT) { return; }
			this.dir = inputDir;
		}

		public PlaceableBigData GetDataAtPos(int x, int y) {
			Vector2I currPos = new Vector2I(x, y);
			
			List<PlaceableBigData> currData = GetTextureGrid(this.dir);

			if (currData == null) return null;
			
			foreach (PlaceableBigData dat in currData) {
				Vector2I datPos = dat.GetPosition(currPos);
				
				if (currPos == datPos) {
					return dat;
				}
			}
			
			return null;
		}

		public PlaceableBigData GetDataAtPos(Vector2I input) {
			return GetDataAtPos(input.X, input.Y);
		}

		/** These should be set in the child, this will be used in the stic methods to construct the stitched texture **/

		public override Texture GetTexture() {
			return null;
		}
		
		public virtual List<PlaceableBigData> GetTextureGrid() {
			return null;
		}

		public virtual List<PlaceableBigData> GetTextureGrid(Direction inDir) {
			return null;
		}

		/** Static methods to stitch together images **/
		
		private static Vector2I GetMaxOffsets(List<PlaceableBigData> data) {
			Vector2I V = new Vector2I(0,0);

			foreach (PlaceableBigData PBD in data) {
				Vector2I off = PBD.GetOffset();	
				if (off.X > V.X) V.X = off.X;
				if (off.Y > V.Y) V.Y = off.Y;
			}
				
			return V;
		}

		private static List<(Image, PlaceableBigData)> GetSortedImgs(List<PlaceableBigData> data) {
			List<(Image, PlaceableBigData)> imgs = new List<(Image, PlaceableBigData)>();

			var tileSet = GD.Load<TileSet>("res://Resources/objects.tres");

			foreach (PlaceableBigData PBD in data) {
				// Get the texture for each PBD

				TileTex TT = PBD.GetTileTex();

				int ID = TT.GetSourceId();
				Vector2I AtPos = TT.GetAtlasPos();

				int sourceid = tileSet.GetSourceId(ID);

				TileSetAtlasSource tileSetSource = tileSet.GetSource(sourceid) as TileSetAtlasSource;

				// get the tile
				var tile = tileSetSource.GetTileTextureRegion(AtPos);
				var fullTexture = tileSetSource.Texture.GetImage();
				var imageTexture = fullTexture.GetRegion(tile);

				// Insert in list such that it is in the correct order to draw
				// (Figure this out later)

				imgs.Add((imageTexture, PBD));
			}

			return imgs;
		}

		private static Image StitchImages(Image baseImg, Image addition, Vector2I offset) {

			// Define the source rectangle from the overlay image
			Rect2I addRect = new Rect2I(Vector2I.Zero, addition.GetSize());

			// Draw the addition on the new image with some offset
			baseImg.BlitRect(baseImg, addRect, offset);

			return baseImg;
		}

		// i.e. return a "texture" (or something) that displays all the textures of this object
		// arrayed in a manner that looks nice. will have to programatically generate (ideally) to
		// handle all four directions properly.
		public static Texture GetBigTexture(List<PlaceableBigData> data)
		{
			// Get the height and width of the big placable
			// Create a large texture based on this height and width
			// Get list of the textures of the cells
			// Place the textures on the large texture in the correct spot
			// 	This should be done is a specific order to ensure correct rendering

			// This will be used to construct the large stitched texture
			Vector2I Size = GetMaxOffsets(data);

			// We need to sort the textures to add them in the correct order
			Image StitchedImage = Image.Create(Size.X * 64, Size.Y * 32, false, Image.Format.Rgb8);

			List<(Image I, PlaceableBigData PBD)> imgs = GetSortedImgs(data); // We need tile tex to keep track of offset
			
			foreach (var D in imgs) {
				Image I = D.I;
				PlaceableBigData PBD = D.PBD;

				Vector2I off = PBD.GetOffset();

				off *= new Vector2I(64, 32); // multiply to get pixel offset

				StitchedImage = StitchImages(StitchedImage, I, off); // Stitch Images together
			}


			return ImageTexture.CreateFromImage(StitchedImage); // return the stitched image as a texture
		}
	}
}

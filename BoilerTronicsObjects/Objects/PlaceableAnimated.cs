using Godot;
using System;
using System.Collections;
using System.Collections.Generic;
using BoilerTronicsObjects.Layers;

/*
TODO: docstring
this is basically the evolution of the old "Framed" system
as this system essentially supports the same functionality as the "Frame" system, except with an
additional dictionary that permits a system to switch between different "sets" of frames
ex:
	- "default" animation goes through a "work" animation
	- "secondary" animation goes through an "explode" animation
	- this system supports picking both and etc
*/
namespace BoilerTronicsObjects.Placeable
{
	// note: this must extend "GodotObject" such that this is a proper Godot Variant
	public partial class PlaceableAnimationData : GodotObject {
		private PlaceableObject parentObj;
		private TileTex texture;	// should always be equal to the first item of 'frames'
		
		private int frameIndex = 0;
		private List<TileTex> frames = new List<TileTex>();
		
		// generic constructor
		public PlaceableAnimationData() {
			this.parentObj = null;
			this.texture = new TileTex();
			
			frames.Add(
				new TileTex(texture.GetAtlasPos(), texture.GetSourceId())
			);
		}
		
		// constructor that *should* be used
		public PlaceableAnimationData(TileTex texture, PlaceableObject parentObject) {
			this.parentObj = parentObject;
			this.texture = TileTex.Copy(texture);
			
			// by default, always add the default visuals into frame slot 0
			frames.Add(
				new TileTex(texture.GetAtlasPos(), texture.GetSourceId())
			);
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
		
		public TileTex GetFrame() {
			return frames[frameIndex];
		}
		public TileTex GetFrame(int index) {
			if (index < 0) { return null; }
			if (index >= frames.Count) { return null; }
			return frames[index];
		}
		
		public PlaceableObject GetParentObj() {
			return parentObj;
		}
		public void SetParentObj(PlaceableObject input) {
			if (input != null) {
				parentObj = input;
			}
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
		
		// steps by one frame, and loops around
		public void StepFrame() {
			frameIndex = (frameIndex + 1) % frames.Count;
		}
		
		// returns the # of frames
		public int GetFrameCount() {
			return frames.Count;
		}
		
		// resets the frame back to this object's original visuals
		public void ResetFrame() {
			frameIndex = 0;
		}
	}

	public abstract class PlaceableAnimated : PlaceableObject
	{	
		protected string activeState = "default";
		protected Godot.Collections.Dictionary<string, PlaceableAnimationData> animationData;
		protected PlaceableAnimationData currData;
		//protected TileTex currTex;
		
		public PlaceableAnimated(int OGX, int OGY, int sourceId, Vector2I atlasPos, int altTitle = 0) 
		: base(OGX, OGY, sourceId, atlasPos, altTitle)
		{
			// always add the "default" state
			animationData.Add(
				"default",
				new PlaceableAnimationData(new TileTex(atlasPos, sourceId), this)
			);
			ResetState();
			
			// child objects must use 'AddFrame' instead
			// ex: AddFrame(new TileTex(new Vector2I(0, 0), sourceId));
		}
		
		// sets this object's state to the following key
		// returns 'false' if not valid
		// also resets the animation state
		public bool SetState(string input) {
			if (input == null || !animationData.ContainsKey(input)) { return false; }
			
			// copy string, set to state
			activeState = String.Copy(input);
			currData = animationData[activeState];
			currData.ResetFrame();
			// currTex = currData.GetTileTex();
			
			return true;
		}
		
		// THE CRUX OF THIS SYSTEM
		// gets the currently active state
		// basically one should do all of the animation handling and etc here
		//
		// TODO: more documentation
		public PlaceableAnimationData GetState() {
			return currData;
		}
		
		// gets the currently visible frame
		public TileTex GetFrame() {
			return currData.GetTileTex();
		}
		
		// resets the object's state
		public void ResetState() {
			SetState("default");
		}
		
		public override int GetSourceID() {
			TileTex T = currData.GetTileTex();
			return T.GetSourceID();
		}
		
		public override Vector2I GetAtlasPos() {
			TileTex T = currData.GetTileTex();
			return T.GetAtlasPos();
		}
		
		// TODO: Keenan work this out!
		public override Texture GetTexture()
		{
			// This will get the texture of the current frame
			TileTex T = currData.GetTileTex();
			if (T == null) return base.GetTexture(); // null

			var tileSet = GD.Load<TileSet>("res://Resources/objects.tres");

			int ID = T.GetSourceId();
			Vector2I AtPos = T.GetAtlasPos();

			TileSetAtlasSource tileSetSource = tileSet.GetSource(ID) as TileSetAtlasSource;

			// get the tile
			var tile = tileSetSource.GetTileTextureRegion(AtPos);
			var fullTexture = tileSetSource.Texture.GetImage();
			var imageTexture = fullTexture.GetRegion(tile);
			var texture = new ImageTexture();
			texture.SetImage(imageTexture);

			return texture;
		}
		
	}
}

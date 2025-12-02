using Godot;
using System;
using System.Collections;
using System.Collections.Generic;
using BoilerTronicsObjects.Layers;
using BoilerTronicsObjects.Interfaces;

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
	/* 
		Contains the following information:
		- object's parent address
		- current active "texture"
		- the (Vector2I) offset of this texture (from the object's current position)
		- a List<TileTex> of the animation's frames
		- a frame index
		
		Constructor:
		public PlaceableAnimationData(TileTex texture, PlaceableObject parentObject, Vector2I inCoords, double iterTime)
	*/
	public partial class PlaceableAnimationData : GodotObject {
		private PlaceableObject parentObj;
		private TileTex texture;	// should always be equal to the first item of 'frames'
		
		private int frameIndex = 0;
		private List<TileTex> frames = new List<TileTex>();
		
		private Vector2I coordsOffset = new Vector2I(0, 0);
		
		private double frameTime = 0;	// when animating, how much time should be inbetween frames?
		
		// generic constructor
		public PlaceableAnimationData() {
			this.parentObj = null;
			this.texture = new TileTex();
			this.frameTime = 0;
			
			frames.Add(
				new TileTex(texture.GetAtlasPos(), texture.GetSourceId())
			);
		}
		
		// constructor that *should* be used
		public PlaceableAnimationData(TileTex texture, PlaceableObject parentObject, Vector2I inCoords, double iterTime) {
			this.parentObj = parentObject;
			this.texture = TileTex.Copy(texture);
			this.frameTime = iterTime;
			SetCoordsOffset(inCoords);
			
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
		
		public double GetFrameTime() {
			return this.frameTime;
		}
		public void SetFrameTime(double input) {
			this.frameTime = input;
		}
		
		public Vector2I GetCoordsOffset() {
			return new Vector2I(coordsOffset.X, coordsOffset.Y);
		}
		public void SetCoordsOffset(Vector2I inCoords) {
			coordsOffset.X = inCoords.X;
			coordsOffset.Y = inCoords.Y;
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
	
	// "process" function that'll actually do the animating and etc
	// extends "Area2D" for "QueueFree()" functionality and for "_Process()"
	/*
		The main idea of this object is that this is a "processing object" that does the following:
		- update a 'PlaceableAnimationData' input (should be 'currData' of a PlaceableAnimated object) to show the correct visuals
			- note: the object's "visuals" (i.e. GetSourceId(), GetAtlasPos()) will be that of 'currData' !!!
		- update the Layer's TileMap accordingly after an animation trigger
	
		Constructor: public AnimatingObject(PlaceableAnimationData input, AnimateType animType, double deltaTime)
	*/
	public partial class AnimatingObject : Node, TimeConsumingObject {
		private BoilerTronicsGlobalManager manager;
		private PlaceableAnimationData data;
		
		// get the coords offset from the original animation data
		private Vector2I coordsOffset = new Vector2I(0, 0); 
		
		private double perFrameTime = 0;
		private double deltaTime = 0;
		private double timeElapsed = 0;
		private int frameCount = 0;
		
		private bool halted = false;
		
		public enum AnimateType
		{
			Step = 1,			// when this object is generated, only "step" the animation once before releasing semaphore
			AnimateFull = 2,	// when this object is generated, "step" through the entire animation before releasing semaphore
		}
		
		private AnimateType animationType = AnimateType.Step; 
		
		
		// vector offset off the object's current position, and this tile should be the "animated" tile
		
		// deltaTime should be 'GlobalManager.currLevel.DeltaTime"
		public AnimatingObject(PlaceableAnimationData input, AnimateType animType, double deltaTime) {
			this.data = input;
			
			coordsOffset = input.GetCoordsOffset();
			
			timeElapsed = 0;
			halted = false;
			this.animationType = animType;
			this.deltaTime = deltaTime;
			frameCount = 0;
			
			if (animationType == AnimateType.AnimateFull) {
				// calculate per-frame speed
				
				// note: by default, "deltaTime == 1.0" given that the base speed is one action per 1.0s
				// at the fastest speed possible, "deltaTime == 0.005", as per the constants in BoilerTronicsLevel
				// this means all actions (claw movement, etc) occur in 0.005s, rather than 1.0s
				// therefore, let's just be lazy and consider that the per-frame speed is actually (frame speed * deltaTime)
				perFrameTime = data.GetFrameTime() * deltaTime;
			}
		}
		
		public override void _Ready() {
			manager = BoilerTronicsGlobalManager.GlobalManager;
			if (manager == null) {
				GD.PrintErr("PlaceableAnimated: _Ready(): Catastrophic Error: Global Manager Not Initialized!");
			}
			
			// grab semaphore, no steps are allowed to occur until this object is finished animating!
			manager.currLevel.runSem.Wait();
			base._Ready();
		}
		
		/*
			is called per frame and etc
			basically does the animation work:
			- updates layer TileMap sprites
			- updates the animation data's status/etc
				- timed for full animation, singular steps otherwise
		*/
		public override void _Process(double delta) {
			PlaceableObject obj = data.GetParentObj();
			Layer parentLayer = obj.GetParentLayer();
			
			// if halted, then don't do anything important!
			if (halted) {
				base._Process(delta);
				return;
			}
			
			// check time elapsed
			timeElapsed += delta;
			
			// TODO: multiple cases
			// FIRST CASE: iterate through animation until finished
			if (animationType == AnimateType.Step) {
				// "Step" Animation Type:
				// Simply step one frame and that's that for this in-game "step"
				
				data.StepFrame();
				
				// tell layer to update its visuals
				UpdateVisuals();
				
				// end this object
				End();
			} else if (animationType == AnimateType.AnimateFull) {
				// "AnimateFull" Animation Type:
				// Iterate through an entire loop of the animation before releasing the semaphore
				
				// only increment "frameCount" if we've passed enough time
				if (timeElapsed > perFrameTime) {
					frameCount++;
					timeElapsed -= perFrameTime;
					
					if (frameCount == data.GetFrameCount()) {
						// end the animation if we've successfully gone through all the frames
						
						End();
					} else {
						// otherwise, update the original object's sprites accordingly
						data.StepFrame();
						// tell layer to update its visuals
						UpdateVisuals();
					}
				}
				
				// end
			}
			
			// call parent _Process
			base._Process(delta);
		}
		
		// helper function that updates the target cell with the current animation state
		public void UpdateVisuals() {
			PlaceableObject obj = data.GetParentObj();
			Layer parentLayer = obj.GetParentLayer();
			TileTex currFrame = data.GetTileTex();
			
			// premature optimization for PlaceableBig objects
			parentLayer.SetCell(obj.GetCurrPos() + coordsOffset, currFrame.GetSourceID(), currFrame.GetAtlasPos());
		}
		
		// reset the original object accordingly!
		public void Reset() {
			PlaceableObject obj = data.GetParentObj();
			Layer parentLayer = obj.GetParentLayer();
			
			// reset visuals
			if (obj is PlaceableAnimated) {
				((PlaceableAnimated) obj).ResetState();
			} else {
				// TODO: big placeable version
			}
			
			// tell layer to update its visuals
			UpdateVisuals();
		}
		
		// de-register object from game state, release semaphore, then queue object deletion
		public void End() {
			// De-register object from the game state
			unregisterConsumingObject();

			// Release sem
			manager.currLevel.runSem.Release();

			// Destroy this object
			this.QueueFree();
		}
		
		// TIME CONSUMING OBJECT INTERFACE
		public void registerConsumingObject() {
			manager.currLevel.RegisterAnimating(this);
		}

		public void unregisterConsumingObject() {
			manager.currLevel.UnRegisterAnimating(this);
		}

		public void haultObject() {
			// TODO: hault object
			halted = true;
		}
	}

	// main class that objects should extend
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
			PlaceableAnimationData newData = new PlaceableAnimationData(new TileTex(atlasPos, sourceId), this, new Vector2I(0, 0), 0.0);
			// newData.AddFrame(new TileTex(atlasPos, sourceId));	// ex: adding more frames to animation data
			animationData.Add(
				"default",
				newData
			);
			ResetState();
		}
		
		// easy function to trigger animations
		// returns if animation was successfully triggered or not
		public bool TriggerAnimation(string key, AnimatingObject.AnimateType animationType) {
			bool res = SetState(key);
			if (!res) { return res; } // if we failed to set this object to the target state, return "false"
			
			// update 'currData'
			
			// tell parent layer to update its visuals
			GetParentLayer().SetCell(this.GetCurrPos() + currData.GetCoordsOffset(), this.GetSourceID(), this.GetAtlasPos());
			// GetParentLayer().UpdateObject(this);
			
			// create new AnimatingObject processor object
			BoilerTronicsGlobalManager manager = BoilerTronicsGlobalManager.GlobalManager;
			AnimatingObject aObj = new AnimatingObject(currData, animationType, manager.currLevel.DeltaTime);
			
			return true;
		}
		
		// sets this object's state to the following (string) key
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
		// other systems (i.e. the AnimatingPlaceable) should be responsible for calling the parent layer and etc
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
		
		
		// overrides visuals in accordance to what the expected visuals!
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

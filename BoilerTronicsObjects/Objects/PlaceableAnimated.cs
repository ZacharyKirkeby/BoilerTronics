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
		- current active "texture"	-- AVOID USING THIS FOR THE MOST PART
			- make sure to use "GetFrame()", and not "GettTileTex()"!!!
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
		
		// alternative constructor that accepts a list of textures to be used
		// note: these textures will be DEEP COPIED into this new object!
		public PlaceableAnimationData(List<TileTex> textures, PlaceableObject parentObject, Vector2I inCoords, double iterTime) {
			this.parentObj = parentObject;
			
			this.frames = TileTex.DeepCopyTileTexList(textures);
			this.texture = frames[0];	// base texture should always be the 0th frame
			
			this.frameTime = iterTime;
			SetCoordsOffset(inCoords);
		}
		
		
		// getters/setters
		public TileTex GetTileTex() {	// note: TRY TO AVOID USING THIS!!! Use 'GetFrame()' instead!!!
			return TileTex.Copy(this.texture);
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
		public void AddFrame(TileTex input) {
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
		public int GetFrameIndex() {
			return frameIndex;
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
		
		// mod the animation if needed
		protected Vector2I atlasMod = new Vector2I(0, 0);
		protected int sourceIdMod = 0;
		public void SetAtlasMod(Vector2I input) { atlasMod.X = input.X; atlasMod.Y = input.Y; }
		public Vector2I GetAtlasMod() { return new Vector2I(atlasMod.X, atlasMod.Y); }
		public void SetSourceIdMod(int i) {sourceIdMod = i;}
		public int GetSourceIdMod() { return sourceIdMod; }
		
		public enum AnimateType
		{
			Step = 1,			// when this object is generated, only "step" the animation once before releasing semaphore
			AnimateFull = 2,	// when this object is generated, "step" through the entire animation before releasing semaphore
		}
		
		private AnimateType animationType = AnimateType.Step; 
		
		// set up overrides
		private AnimatingObjectOverrides ownOverrides = null;
		
		// vector offset off the object's current position, and this tile should be the "animated" tile
		
		// deltaTime should be 'GlobalManager.currLevel.DeltaTime"
		public AnimatingObject(PlaceableAnimationData input, AnimateType animType, double deltaTime, AnimatingObjectOverrides overrides = null) {
			initialize(input, animType, deltaTime, overrides);
		}
		
		public AnimatingObject(PlaceableAnimationData input, AnimateType animType, double deltaTime, Vector2I atlasModIn, int sourceIdModIn, AnimatingObjectOverrides overrides = null){
			initialize(input, animType, deltaTime, overrides);
			SetAtlasMod(atlasModIn);
			SetSourceIdMod(sourceIdModIn);
		}
		
		// what does the actual "constructing" work
		private void initialize(PlaceableAnimationData input, AnimateType animType, double deltaTime, AnimatingObjectOverrides overrides = null) {
			// GD.Print("AnimatingObject: ", this, ": Creating Object");
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
				
				GD.Print("AnimatingObject: ", this, ": Per-Frame-Time: ", perFrameTime);
			}
			
			this.ownOverrides = overrides;
			// GD.Print("AnimatingObject: ", this, ": Created Object");
		}
		
		public override void _Ready() {
			manager = BoilerTronicsGlobalManager.GlobalManager;
			if (manager == null) {
				GD.PrintErr("PlaceableAnimated: _Ready(): Catastrophic Error: Global Manager Not Initialized!");
			}
			
			// register with the level; critical such that this object can be terminated at any point!
			registerConsumingObject();
			
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
				// GD.Print("AnimatingObject: ", this, ": Process halted, stalling.");
				base._Process(delta);
				return;
			}
			
			// OVERRIDES
			if (ownOverrides != null) {
				switch (ownOverrides.GetType()) {
					
					// skip to end case:
					case AnimatingObjectOverrides.OverrideType.SkipToEnd:
						data.SetFrameIndex(data.GetFrameCount() - 1);
						this.UpdateVisuals();
						End();
						return;
						break;
				}
			}
			
			// check time elapsed
			timeElapsed += delta;
			// GD.Print("AnimatingObject: ", this, ": timeElapsed: ", timeElapsed);
			
			// TODO: multiple cases
			// FIRST CASE: iterate through animation until finished
			if (animationType == AnimateType.Step) {
				// "Step" Animation Type:
				// Simply step one frame and that's that for this in-game "step"
				GD.Print("AnimatingObject: ", this, ": Step Animation Finished");
				data.StepFrame();
				
				// tell layer to update its visuals
				this.UpdateVisuals();
				
				// end this object
				End();
			} else if (animationType == AnimateType.AnimateFull) {
				// "AnimateFull" Animation Type:
				// Iterate through an entire loop of the animation before releasing the semaphore
				
				// only increment "frameCount" if we've passed enough time
				if (timeElapsed > perFrameTime) {
					GD.Print("AnimatingObject: ", this, ": Stepped Full Animation");
					frameCount++;
					timeElapsed -= perFrameTime;
					
					if (frameCount >= data.GetFrameCount()) {
						// end the animation if we've successfully gone through all the frames
						
						GD.Print("AnimatingObject: ", this, ": Finished Full Animation");
						End();
					} else {
						// otherwise, update the original object's sprites accordingly
						data.StepFrame();
						// tell layer to update its visuals
						this.UpdateVisuals();
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
			TileTex currFrame = data.GetFrame();
			
			// premature optimization for PlaceableBig objects
			Vector2I modPos = obj.GetCurrPos() + coordsOffset;
			int sourceId = currFrame.GetSourceId() + this.GetSourceIdMod();
			Vector2I atlasPos = currFrame.GetAtlasPos() + this.GetAtlasMod();
			parentLayer.SetCell(modPos, sourceId, atlasPos);
			GD.Print("AnimatingObject: ", this, ": Updated cell ", modPos, ", sourceID: ", sourceId, ", atlasPos: ", atlasPos.ToString());
		}
		
		
		// reset the original object accordingly
		public void Reset() {
			PlaceableObject obj = data.GetParentObj();
			Layer parentLayer = obj.GetParentLayer();
			
			// reset object's state
			if (obj is PlaceableAnimated) {
				((PlaceableAnimated) obj).ResetState();
			} else {
				// TODO: big placeable version
			}
			
			// note: the layer's "Reset()" functionality already perfectly handles visuals and etc
			// does not need to be handled here!
		}
		
		// de-register object from game state, release semaphore, then queue object deletion
		public void End() {
			// De-register object from the game state
			unregisterConsumingObject();

			// Release sem
			manager.currLevel.runSem.Release();
			
			// Stop all actions; failsafe!
			halted = true;

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
			GD.Print("AnimatingObject: ", this, ": Halting Object");
			// TODO: hault object
			halted = true;
		}
	}
	
	// extra little data tidbit -- to be expanded in the future -- that allows for specific configs
	// of a standard AnimatingObject trigger.
	// for now, this is to force a normal animation to "skip" to its last frame instantly.
	// this is very scuffed but this should set the groundwork for future (tm) stuff.
	public class AnimatingObjectOverrides {
		
		public enum OverrideType
		{
			SkipToEnd = 1,			// Overrides the animation to immediately skip to the end of the animation rather than playing it thorugh.
		}
		private OverrideType type;
		
		public AnimatingObjectOverrides(OverrideType input) {
			this.type = input;
		}
		
		public OverrideType GetType() {
			return type;
		}
		
	}

	/* 
		TODO: better documentation
		
		this is the class that "animated" objects should extend!!!
		(baring PlaceableBig type objects -- special version to come soon (tm)
	*/
	public abstract class PlaceableAnimated : PlaceableObject
	{	
		protected string activeState = "default";
		protected Godot.Collections.Dictionary<string, PlaceableAnimationData> animationData;
		protected PlaceableAnimationData currData;
		//protected TileTex currTex;
		
		// add to the played animations and etc
		protected Vector2I atlasMod = new Vector2I(0, 0);
		protected int sourceIdMod = 0;
		
		public void SetAtlasMod(Vector2I input) { atlasMod.X = input.X; atlasMod.Y = input.Y; }
		public Vector2I GetAtlasMod() { return new Vector2I(atlasMod.X, atlasMod.Y); }
		
		public void SetSourceIdMod(int i) {sourceIdMod = i;}
		public int GetSourceIdMod() { return sourceIdMod; }
		
		public PlaceableAnimated(int OGX, int OGY, int sourceId, Vector2I atlasPos, int altTitle = 0) 
		: base(OGX, OGY, sourceId, atlasPos, altTitle)
		{
			animationData = new Godot.Collections.Dictionary<string, PlaceableAnimationData>();
			
			// always add the "default" state
			List<TileTex> defaultAnim = new List<TileTex>{
				new TileTex(atlasPos, sourceId)
			};
			PlaceableAnimationData newData = new PlaceableAnimationData(
				defaultAnim, this,
				new Vector2I(0, 0), // (0, 0) offset
				0.0);				// each frame is 0.0s long
			animationData.Add(
				"default",
				newData
			);
			
			/*
			// alt init:
			PlaceableAnimationData newData = new PlaceableAnimationData(new TileTex(atlasPos, sourceId), this, new Vector2I(0, 0), 0.0);
			// newData.AddFrame(new TileTex(atlasPos, sourceId));	// ex: adding more frames to animation data
			animationData.Add(
				"default",
				newData
			);
			*/
			
			ResetState();
		}
		
		// easy function to trigger animations
		// returns if animation was successfully triggered or not
		public bool TriggerAnimation(string key, AnimatingObject.AnimateType animationType, AnimatingObjectOverrides overrides = null) {
			return TriggerAnimation(key, animationType, new Vector2I(0, 0), 0, overrides);
		}
		
		// note: the mod inputs ONLY AFFECT ANIMATIONS
		// not the object's raw sprites (change behavior or?)
		public bool TriggerAnimation(string key, AnimatingObject.AnimateType animationType, Vector2I modAtlas, int modSourceId, AnimatingObjectOverrides overrides = null) {
			bool res = SetState(key);
			if (!res) { return res; } // if we failed to set this object to the target state, return "false"
			
			GD.Print("PlaceableAnimated: ", this, ", Triggering Animation: ", key);
			// update 'currData'
			
			// tell parent layer to update its visuals
			GetParentLayer().SetCell(this.GetCurrPos() + currData.GetCoordsOffset(), 
				currData.GetFrame(0).GetSourceID() + modSourceId, 
				currData.GetFrame(0).GetAtlasPos() + modAtlas
			);
			// GetParentLayer().UpdateObject(this);
			
			// create new AnimatingObject processor object
			BoilerTronicsGlobalManager manager = BoilerTronicsGlobalManager.GlobalManager;
			AnimatingObject aObj = new AnimatingObject(currData, animationType, manager.currLevel.DeltaTime, modAtlas, modSourceId, overrides);
			
			// "add to the scene" such that _Process works as intended
			manager.currLevel.cLayer.GetParent().AddChild(aObj);
			
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
		
		// gets current state name
		public string GetStateName() {
			return new string(activeState);
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
			return currData.GetFrame().GetSourceID() + GetSourceIdMod();
		}
		
		public override Vector2I GetAtlasPos() {
			return currData.GetFrame().GetAtlasPos() + GetAtlasMod();
		}
		
		// TODO: Keenan please double check this!
		public override Texture GetTexture()
		{
			// This will get the texture of the current frame
			TileTex T = currData.GetFrame();
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

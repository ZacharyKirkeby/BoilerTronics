using Godot;
using System;
using System.Collections;
using System.Collections.Generic;
using BoilerTronicsObjects.Layers;

/*
	How this object should be handled:
	
Objects that change visuals while a solution is in action must extend this class.
The main function of this object is to hold a list of possible "frames" that an
object could show, i.e. a "frame" is an object's sprite (atlasPos + sourceId).

By using the function 'GetFrame()', one can get a TileTex representing an object's
current visual, or at least its internal visual. This is different from an object's
actual atlasPos/sourceId as these two variables correspond to an object's identity --
at least when seen from the saving/loading system.

Note that moving objects (Claw, etc) must only utilize 'GetFrame()' to adjust their
new in action visuals, while static objects (SomeChangingFactory) must also
update their layer tilemaps accordingly to temporarily show a new visual, and
upon some 'Reset()' call, must update their layer tilemaps back to their original
visuals.

TODO: make an actual child object to test
*/

namespace BoilerTronicsObjects.Placeable
{

	public abstract class PlaceableFramed : PlaceableObject
	{
		// what frame index (i.e. in 'frames') that this object should display
		private int frameIndex = 0;
		private List<TileTex> frames = new List<TileTex>();
		
		public PlaceableFramed(int OGX, int OGY, int sourceId, Vector2I atlasPos, int altTitle = 0) 
		: base(OGX, OGY, sourceId, atlasPos, altTitle)
		{
			// by default, always add the default visuals into frame slot 0
			frames.Add(
				new TileTex(atlasPos, sourceId)
			);
			
			// child objects must use 'AddFrame' instead
			// ex: AddFrame(new TileTex(new Vector2I(0, 0), sourceId));
		}
		
		// THE CRUX OF THIS SYSTEM
		// call on this to get this object's currently displayed visual!
		// chosen frame will be frames[frameIndex]
		// TODO: will need to update Layer, children, etc to actually make use of this system
		//
		// even objects that actually update their layer's TileMap should utilize this system
		// as to affect the actual 'sourceId' and 'atlasPosition' is to really mess with
		// the save system and etc. Not ideal!
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
		
		
		// TODO: Keenan work this out!
		public override Texture GetTexture()
		{
			// This will get the texture of the current frame
			TileTex T = frames[frameIndex];
			if (T == null) return base.GetTexture(); // null

			return null; // Replace this with the constructed texture
		}
		
	}
}

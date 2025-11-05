// TODO: implement in more detail
using Godot;
using System;
using BoilerTronicsObjects.Layers;
using BoilerTronicsObjects.Objects.FactoryLayerObjects;
using BoilerTronicsObjects.Placeable;
using BoilerTronicsObjects.Interfaces;
using BoilerTronicsObjects.Data;

namespace BoilerTronicsObjects.Objects.FactoryLayerObjects {

	public class FactoryOutputObject : PlaceableObject, Movable {

		/*
		 * Ideas that could work to make the output better:
		 *
		 * Table that would allow us to accept multiple items and need a diffrent amout of each item
		 *
		 */
		
		// default visuals
		static Vector2I objectAtlasPos = new Vector2I(0, 1);
		static int layerSourceId = 0;
		// reminder that the sourceID corresponds to the sprite sheet for a given layer
		// and every layer will have their own sprite sheet. Consequently, layer-specific
		// objects will have identical sourceIds.
		
		// "atlasPos" corresponds to the location on a given sprite sheet that a specific object
		// (i.e. "claw", "factory", "floor tile", "leftrail") will correspond to.
		private int _TargetObjectID;
		private int _TargetNum;

		private int _CurrNum;
		
		public PlaceableObject PickUp() {
			return null; // We can't pick something up from the out put
		}
		
		public bool Place(PlaceableObject obj) {
			int objSourceID = obj.GetSourceID();
			Vector2I objAtlasPos = obj.GetAtlasPos();
			int objID = BoilerTronicsData.objectMap[BoilerTronicsData.hashCoords(objSourceID, objAtlasPos)];
			GD.Print("Recived: ", objID);
			if (objID == _TargetObjectID) _CurrNum--;
			GD.Print("Objects Left: ", _CurrNum);
			if (_CurrNum == 0) GD.Print("You Won!"); // this will be a function call to the level later
			return true;
		}

		public override void ResetPos()
		{
			_CurrNum = _TargetNum;
			base.ResetPos();
		}

		public void setTargetNum(int newNum) {
			_TargetNum = newNum;
		}

		public int getTargetNum() {
			return _TargetNum;
		}

		public void setTargetID(int newID) {
			_TargetObjectID = newID;
		}

		public int getTargetID() {
			return _TargetObjectID;
		}

		public FactoryOutputObject(int OGX, int OGY, int altTitle = 0, int targetID = 200, int targetNum = 1) 
		: base(OGX, OGY, layerSourceId, objectAtlasPos, altTitle) {
			_TargetObjectID = targetID;
			_TargetNum = targetNum;
			_CurrNum = _TargetNum;
		}
	}
}

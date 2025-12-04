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
		
		public override int GetCost() { return 0; }
		public new static int GetCostStatic() { return 0; }

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
			if (_CurrNum == 0) {
				BoilerTronicsGlobalManager manager = BoilerTronicsGlobalManager.GlobalManager;
				manager.currLevel.UpdateSolutionStats();
				manager.currLevel.HaultObjects();
				manager.currLevel.Pause();
			}
			return true;
		}

		public override void ResetPos()
		{
			_CurrNum = _TargetNum;
			base.ResetPos();
		}

		public void setTargetNum(int newNum) {
			_TargetNum = newNum;
			_CurrNum = newNum;
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
			
			switch(_TargetObjectID) {
				case 200:
					// coal variant
					SetAtlasPos(new Vector2I(1, 1));
					break;
				case 201:
					// iron ore variant
					SetAtlasPos(new Vector2I(2, 1));
					break;
				case 202:
					// iron bar variant
					SetAtlasPos(new Vector2I(3, 1));
					break;
				case 203:
					// iron plate variant
					SetAtlasPos(new Vector2I(0, 3));
					break;
				case 204:
					// iron rod variant
					SetAtlasPos(new Vector2I(1, 3));
					break;
				case 205:
					// steel rod
					SetAtlasPos(new Vector2I(2, 3));
					break;
				case 206:
					// steel plate
					SetAtlasPos(new Vector2I(3, 3));
					break;
				case 207:
					// steel gear
					SetAtlasPos(new Vector2I(0, 1));
					break;
				default:
					SetAtlasPos(new Vector2I(2, 3)); // Place holder
					GD.Print("FactoryInputObject: Warning: InputObject does not have visual variant for object id: ", _TargetObjectID);
					break;;
			}
		}
	}
}

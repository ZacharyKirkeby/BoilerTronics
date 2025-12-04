using Godot;
using System;
using BoilerTronicsObjects.Placeable;
using BoilerTronicsObjects.Layers;
using BoilerTronicsObjects.Interfaces;

/*
	TODO:
	the conceit of this object is to generate a visual, 2D sprite that displays
	as the "selection" indicator of an object
*/
public partial class SelectingObject : Area2D
{
	BoilerTronicsGlobalManager manager;	// helper value
	
	PlaceableObject obj;	// holds pointer to object
	Layer layer;			// holds pointer to object's parent layer
	Vector2 pos;		// position of this sprite
	Sprite2D sprite;	// generated sprite
	Sprite2D highlight;	// highlight surrounding the generated sprite
	
	public SelectingObject(PlaceableObject obj, Layer layer) {
		this.obj = obj;
		this.layer = layer;
		
		// GD.Print("SelectingObject: obj pos:", obj.GetOGPos());
		Vector2 currLocalPos = layer.MapToLocal(obj.GetOGPos()); // we have to divide by 2 apparently? (MovingObject.cs)
		// GD.Print("SelectingObject: currLocalPos:", currLocalPos);
		// GD.Print("SelectingObject: layerPos:", layer.Position);
		
		// do we also actually need to do this here? idk man, just referring to MovingObject.cs
		this.pos = currLocalPos;// + layer.Position;
		
		manager = BoilerTronicsGlobalManager.GlobalManager;
		manager.selectingObject = this;
	}
	
	public override void _Ready() {
		GenerateSprite();
	}
	
	private void GenerateSprite() {
		// create sprite, attach to this object
		sprite = new Sprite2D();
		
		// set up correct offsets
		if (layer is ClawLayer || layer is RailLayer) {
			sprite.Offset = new Vector2(0, 24);
		} else {
			sprite.Offset = new Vector2(0, 8);
		}
		sprite.Position = this.pos;
		
		// set up correct textures
		if (this.obj is PlaceableBig bObj) {
			sprite.Texture = bObj.GetTexture() as Texture2D;
			
			// TODO: big placeables have some slightly *funky* behaviors in how they render
			// apply custom offset to ensure that the selection highlight renders properly!
			
			Texture2D tex = sprite.Texture;
			
			sprite.Offset = new Vector2(tex.GetWidth() / 2, tex.GetHeight() / 2);
			
			sprite.Offset -= DraggableObject.CalculatePlaceableBigOffset(bObj);
		} else {
			sprite.Texture = this.obj.GetTexture() as Texture2D;
		}
		
		// generate highlight sprite as appropriate
		highlight = new Sprite2D();
		highlight.Texture = sprite.Texture;
		highlight.Offset = sprite.Offset;
		highlight.Position = sprite.Position;
		highlight.Scale = new Vector2(1.2f, 1.2f);

		// add as child such that if this object is QueueFree()'d, this sprite will be too
		this.AddChild(highlight);
		this.AddChild(sprite);
		sprite.ZIndex = layer.ZIndex + 2;
		
		// TODO: apply correct visual effects
		highlight.Modulate = new Color("ffffffff");
		highlight.Modulate = new Color("000000ff");
	}
}

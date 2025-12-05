using Godot;
using System;
using System.Collections.Generic;
using BoilerTronicsObjects.Placeable;

public partial class DraggableObject : Node2D {
	
	private Vector2 mouse_offset;
	private PlaceableObject obj;
	private Sprite2D sprite;

	public DraggableObject(Vector2 mouse_offset, Sprite2D spritToDrag, PlaceableObject obj) {
		this.mouse_offset = mouse_offset;

		// Copy Sprite and make it a child
		this.sprite = spritToDrag.Duplicate() as Sprite2D;
		this.sprite.Scale = new Vector2I(1, 1);
		this.obj = obj; // This will keep track of the object that we are placing
		
		
		if (this.obj is PlaceableBig bObj) {
			Texture2D tex = sprite.Texture;
			sprite.Offset += new Vector2(tex.GetWidth() / 2, tex.GetHeight() / 2);
			
			// offset the sprite accordingly such that the mouse is over (0, 0) of the sprite
			// i.e. the tile map grid coordinates of the big placeable's origin
			sprite.Offset -= CalculatePlaceableBigOffset(bObj);
		}
		
		// Set to very high Z-index such that this block is visibly above all other blocks
		this.ZIndex = 1000;

		AddChild(this.sprite);
	}
	
	// assume that we are at the top-left of the image; we are to calculate
	// the offset from that position to the (0, 0) block.
	// reuses code from PlaceableBig's GetTexture() and related functions.
	public static Vector2 CalculatePlaceableBigOffset(PlaceableBig bObj) {
		List<PlaceableBigData> data = bObj.GetTextureGrid();
		
		// from PlaceableBig: GetBigTexture()
		const int tileWidth = 32;
		const int tileHeight = 16; 
		const int halfTileWidth = 16; 
		const int halfTileHeight = 8; 
		
		// reminder: this is in half-tiles!
		int minX = 0, minY = 0, maxX = 0, maxY = 0;
		foreach (PlaceableBigData pbd in data)
		{
			Vector2I off = pbd.GetOffset();
			int translatedX = 0;
			int translatedY = 0;
			translatedX += off.X; translatedY += off.X;		// "X" to our X
			translatedY += off.Y * 2;						// "Y" to our Y
			
			if (translatedX < minX) minX = translatedX;
			if (translatedY < minY) minY = translatedY;
			if (translatedX > maxX) maxX = translatedX;
			if (translatedY > maxY) maxY = translatedY;
		}
		
		// GD.Print("DraggableObject: (minX, minY): ", new Vector2I(minX, minY), ", (maxX, maxY): ", new Vector2I(maxX, maxY));
		
		// note: consider that we are moving the whole image texture, not a part of the image texture!
		// the current "origin" of the object is currently at (minX, 0)
		
		// calculate relative offset from the top-left corner of the image
		// this basically sets up the correct (0, 0) in a way
		Vector2I relativeOff = new Vector2I(0, 0) - new Vector2I(minX, minY);
		
		// consider that in this case, the texture's base position is at the top left corner!
		// we need to "nudge" it down one tile
		relativeOff += new Vector2I(1, 1);
		
		// Construct Position of tile; need to take the calculated coordinates (in widths) and translate to pixels
		Vector2I finalPixelPosition = new Vector2I(relativeOff.X * halfTileWidth, relativeOff.Y * halfTileHeight);

		return finalPixelPosition;
	}

	public override void _Ready() {
		// We may need to communicate somthing to the manager
		BoilerTronicsGlobalManager manager = BoilerTronicsGlobalManager.GlobalManager;

		manager.objectToMove = this.obj; // This is a refrence that will be used when we are actually placing the object
	}

	// this will allow for the draggable object to follow the mouse
	public override void _Process(double delta) {
		sprite.Position = GetGlobalMousePosition(); // add some mouse offset later
	}

	public override void _Input(InputEvent @event)
	{
		BoilerTronicsGlobalManager manager = BoilerTronicsGlobalManager.GlobalManager;
		// manager.objectToMove = null;

		// Check if we let go (attempt to place)
		if (@event is InputEventMouseButton buttonEvent && buttonEvent.ButtonIndex == MouseButton.Left && buttonEvent.IsReleased())
		{
			// If so we want to delete everything
			sprite.QueueFree();
			QueueFree();
			// reset layer transparency
			manager.layerClaw.Modulate = manager.layerDefaultVisibility;
			manager.layerFactory.Modulate = manager.layerDefaultVisibility;
			manager.layerFloor.Modulate =manager.layerDefaultVisibility;
			manager.layerRail.Modulate = manager.layerDefaultVisibility;
			manager.layerMovement.Modulate = manager.layerDefaultVisibility;
			Node2D subView = GetNode("../Node2D") as Node2D;
			subView._Input(@event);
		}

		// Always pass downward
		base._Input(@event);
	}
}

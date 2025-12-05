using Godot;
using System;
using System.Collections.Generic;
using BoilerTronicsObjects.Placeable;
using BoilerTronicsObjects.Objects;
using BoilerTronicsObjects.Data;

public partial class DraggableObject : Node2D {
	
	private Vector2 baseSpriteOffset;
	private Vector2 mouse_offset;
	private PlaceableObject obj;
	private Sprite2D sprite;
	private int rotateCount;
	
	// keep track of labels to free as needed and etc
	private Label costLabel;
	private Label labelQ;
	private Label labelE;
	
	BoilerTronicsGlobalManager manager;

	public DraggableObject(Vector2 mouse_offset, Sprite2D spritToDrag, PlaceableObject obj) {
		this.mouse_offset = mouse_offset;
		this.rotateCount = 0;

		// Copy Sprite and make it a child
		this.sprite = spritToDrag.Duplicate() as Sprite2D;
		this.sprite.Scale = new Vector2I(1, 1);
		this.obj = obj; // This will keep track of the object that we are placing
		
		// store base sprite offset
		baseSpriteOffset = sprite.Offset;
		
		
		if (this.obj is PlaceableBig bObj) {
			Texture2D tex = sprite.Texture;
			sprite.Offset += new Vector2(tex.GetWidth() / 2, tex.GetHeight() / 2);
			
			// offset the sprite accordingly such that the mouse is over (0, 0) of the sprite
			// i.e. the tile map grid coordinates of the big placeable's origin
			sprite.Offset -= CalculatePlaceableBigOffset(bObj, (int) bObj.GetDir());
		}
		
		// Set to very high Z-index such that this block is visibly above all other blocks
		this.ZIndex = 1000;

		AddChild(this.sprite);
	}
	
	// assume that we are at the top-left of the image; we are to calculate
	// the offset from that position to the (0, 0) block.
	// reuses code from PlaceableBig's GetTexture() and related functions.
	public static Vector2 CalculatePlaceableBigOffset(PlaceableBig bObj, int dir) {
		List<PlaceableBigData> data = bObj.GetTextureGrid((BoilerTronicsObjects.Placeable.PlaceableBig.Direction) dir);
		
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
		manager = BoilerTronicsGlobalManager.GlobalManager;

		manager.objectToMove = this.obj; // This is a refrence that will be used when we are actually placing the object
		
		// keep track of direction
		if (this.obj is PlaceableBig bObj) {
			manager.objectToMoveDir = (int) bObj.GetDir();
		} else {
			manager.objectToMoveDir = 0;
		}
		
		ClearOldLabels();
		GenerateTextPrompts();
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
		
		// keyboard events
		if (@event is InputEventKey keyEvent && keyEvent.Pressed) {
			
			switch (keyEvent.Keycode) {
				// key codes: https://docs.godotengine.org/en/latest/classes/class_%40globalscope.html#enum-globalscope-key
				case Key.E:
					// GD.Print("TODO: Rotate Image Right!");
					rotateCount = (rotateCount + 1) % 4;
					// GD.Print("rotateCount: ", rotateCount);
					if (obj is PlaceableBig) {
						UpdateBigSpriteTexture();
					}
				break;
				case Key.Q:
					// GD.Print("TODO: Rotate Image Left!");
					rotateCount = (rotateCount - 1) % 4;
					if (rotateCount < 0) rotateCount = 4 + rotateCount; // make sure we loop properly!
					// GD.Print("rotateCount: ", rotateCount);
					if (obj is PlaceableBig) {
						UpdateBigSpriteTexture();
					}
				break;
			}
		}

		// Always pass downward
		base._Input(@event);
	}
	
	// update internal sprite texture
	private void UpdateBigSpriteTexture() {
		PlaceableBig bObj = obj as PlaceableBig;
		int sourceId = obj.GetSourceID();
		Vector2I atlasPos = obj.GetAtlasPos();
		int dir = ((int) bObj.GetDir() + rotateCount) % 4;
		
		// update global var
		manager.objectToMoveDir = dir;
		
		List<PlaceableBigData> data = ObjectFactory.GetBigObjectTileMap(BoilerTronicsData.objectMap[BoilerTronicsData.hashCoords(sourceId, atlasPos)], (BoilerTronicsObjects.Placeable.PlaceableBig.Direction) dir);
		
		// failsafe: double check that 'data' isn't null!
		if (data != null) {
			// Then we can update the texture
			ImageTexture texture = PlaceableBig.GetBigTexture(data) as ImageTexture;
			
			// undo texture offset
			// sprite.Offset -= new Vector2(sprite.Texture.GetWidth() / 2, sprite.Texture.GetHeight() / 2);
			// sprite.Offset += CalculatePlaceableBigOffset(bObj, (int) bObj.GetDir());
			
			sprite.Texture = texture;			// assign new texture
			sprite.Offset = baseSpriteOffset; 	// reset offset
			
			// new texture offset
			sprite.Offset += new Vector2(sprite.Texture.GetWidth() / 2, sprite.Texture.GetHeight() / 2);
			sprite.Offset -= CalculatePlaceableBigOffset(bObj, dir);
			return;
		} else {
			GD.PrintErr("DraggableObject: UpdateBigSpriteTexture: Catastrophic error, GetBigTexture failed!");
			GD.PrintErr("DraggableObject: UpdateBigSpriteTexture: sourceID: ", sourceId, ", atlasPos: ", atlasPos, ", dir: ", dir);
		}
		
		ClearOldLabels();
		GenerateTextPrompts();
	}
	
	private void ClearOldLabels() {
		if (costLabel != null && IsInstanceValid(costLabel)) {
			costLabel.QueueFree();
			costLabel = null;
		}
		
		if (labelQ != null && IsInstanceValid(labelQ)) {
			labelQ.QueueFree();
			labelQ = null;
		}
		
		if (labelE != null && IsInstanceValid(labelE)) {
			labelE.QueueFree();
			labelE = null;
		}
	}
	
	private void GenerateTextPrompts() {
		
		// Cost label
		costLabel = new Label();
		costLabel.SetText("$" + obj.GetCost());
		costLabel.Position = this.Position;
		
		Theme inTheme = (Godot.Theme) GD.Load("res://Scenes/buttontheme.tres");
		
		// thanks: https://godotforums.org/d/33246-changing-font-size-of-the-label-through-code/3
		costLabel.AddThemeFontSizeOverride("font_size", 16);
		costLabel.SetTheme(inTheme);
		costLabel.Position += new Vector2(16, 16);
		
		sprite.AddChild(costLabel);
		
		// if PlaceableBig, add text prompts for rotation purposes
		if (obj is PlaceableBig) {
			ImageTexture tex = sprite.Texture as ImageTexture;
			
			// Q label
			labelQ = new Label();
			labelQ.SetText("<-- Q");
			labelQ.Position = this.Position;
			labelQ.AddThemeFontSizeOverride("font_size", 16);
			labelQ.SetTheme(inTheme);
			labelQ.Position += new Vector2(-tex.GetWidth() - 0, -tex.GetHeight() - 8);
			sprite.AddChild(labelQ);
			
			
			// E label
			labelE = new Label();
			labelE.SetText("E -->");
			labelE.Position = this.Position;
			labelE.AddThemeFontSizeOverride("font_size", 16);
			labelE.SetTheme(inTheme);
			labelE.Position += new Vector2(tex.GetWidth() - 16, -tex.GetHeight() - 8);
			sprite.AddChild(labelE);
		}
	}
}

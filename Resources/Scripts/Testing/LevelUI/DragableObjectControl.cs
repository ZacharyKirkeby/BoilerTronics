using Godot;
using System;

// this script will be 
public partial class DragableObjectControl : Control {
	
	// if false, then should block all drag attempts
	public static bool allowDrag = true;
	
	Sprite2D sprite;
	Vector2I atlasCords;
	int selection;
	static Vector2I visibleObjectScaling = new Vector2I(5, 5);

	public DragableObjectControl(ImageTexture texture, Vector2I atlasCords, int posX, int posY, int selection) {
		sprite = new Sprite2D();
		// get texture
		sprite.Texture = texture;
		sprite.Scale = visibleObjectScaling;
		sprite.Set(Sprite2D.PropertyName.Position, new Vector2I(posX, posY));
		AddChild(sprite);
		this.atlasCords = atlasCords;
		this.selection = selection;
	}

	public override void _Ready() {
		CustomMinimumSize = new Vector2(256, 256);
	}

	public override void _GuiInput(InputEvent @event)
	{
		if (@event is InputEventMouseButton buttonEvent && buttonEvent.ButtonIndex == MouseButton.Left && buttonEvent.Pressed
			&& allowDrag)
		{
			BoilerTronicsGlobalManager manager = BoilerTronicsGlobalManager.GlobalManager;
			if (manager.currLevel.StepCount != 0) return; // Don't allow placement while we are stepping

			// We want to spawn a new draggable object and pass in all the correct values
			var draggable = new DraggableObject(Position - GetGlobalMousePosition(), sprite, atlasCords);
			SubViewport subView = GetTree().Root.GetNode("/root/Node2D/MainVBox/TerminalLevelSplit/VBoxContainer/LevelContainer/SubViewport") as SubViewport;
			subView.AddChild(draggable);
			// spawn terminal perhap?
			
			GD.Print("Created new dragable:", draggable);
			manager.objectToPlace = atlasCords;
			manager.placingObject = 1;
			manager.currSlection = selection;
		}
		else
		{
			base._Input(@event); // pass downward
		}
	}
}

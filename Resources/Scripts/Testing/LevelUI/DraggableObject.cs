using Godot;
using System;

public partial class DraggableObject : Node2D {
	
	private Vector2 mouse_offset;
	private Sprite2D sprite;
	private Vector2I atlasCords;

	public DraggableObject(Vector2 mouse_offset, Sprite2D spritToDrag, Vector2I atlasCords) {
		this.mouse_offset = mouse_offset;
		// Copy Sprite and make it a child
		sprite = spritToDrag.Duplicate() as Sprite2D;
		sprite.Scale = new Vector2I(1, 1);
		AddChild(sprite);
	}

	public override void _Ready() {
		// We may need to communicate somthing to the manager
		BoilerTronicsGlobalManager manager = BoilerTronicsGlobalManager.GlobalManager;
		manager.objectToPlace = atlasCords;
	}

	// this will allow for the draggable object to follow the mouse
	public override void _Process(double delta) {
		sprite.Position = GetGlobalMousePosition(); // add some mouse offset later
	}

	public override void _Input(InputEvent @event)
	{
		BoilerTronicsGlobalManager manager = BoilerTronicsGlobalManager.GlobalManager;

		// Check if we let go (attempt to place)
		if (@event is InputEventMouseButton buttonEvent && buttonEvent.ButtonIndex == MouseButton.Left && buttonEvent.IsReleased())
		{
			// If so we want to delete everything
			sprite.QueueFree();
			QueueFree();

			Node2D subView = GetNode("../Node2D") as Node2D;
			subView._Input(@event);
		}

		// Always pass downward
		base._Input(@event);
	}
}

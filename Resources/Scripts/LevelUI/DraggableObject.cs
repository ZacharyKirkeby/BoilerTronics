using Godot;
using System;
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

		AddChild(this.sprite);
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

			Node2D subView = GetNode("../Node2D") as Node2D;
			subView._Input(@event);
		}

		// Always pass downward
		base._Input(@event);
	}
}

using Godot;

public partial class MovingObject : Area2D {
	
	public override void _Ready() {
		// Create collison object 2d
		// Create collison circle 2d
		// Create sprite
		AreaEntered += Collison; // add event for when we detect a collison
	}

    public override void _Process(double delta)
    {
        base._Process(delta);
    }

    public void Collison(Node2D body) {

    }
}
